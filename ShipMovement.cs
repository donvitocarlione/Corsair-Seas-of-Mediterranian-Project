using UnityEngine;

public class ShipMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float stoppingDistance = 1f;
    
    [Header("Movement Smoothing")]
    [SerializeField] private float rotationSmoothTime = 0.3f;
    [SerializeField] private float velocitySmoothTime = 0.3f;
    [SerializeField] private float heightSmoothTime = 0.5f;
    
    [Header("Water Interaction")]
    [SerializeField] private float waterLevel = 0f;
    [SerializeField] private float buoyancyOffset = 0.5f;
    [SerializeField] private float waterDrag = 0.95f;
    [SerializeField] private float stoppingThreshold = 0.1f;

    [Header("Combat Movement")]
    [SerializeField] private float combatStoppingDistance = 10f;
    [SerializeField] private float optimalCombatDistance = 12f;
    [SerializeField] private float repositionThreshold = 5f;
    
    private Vector3 targetPosition;
    private float currentSpeed;
    private bool isMoving;
    private Rigidbody rb;
    private Buoyancy buoyancy;
    private Ship targetShip;
    private Ship ownShip;
    
    // Smoothing variables
    private Vector3 currentVelocity;
    private float rotationVelocity;
    private float heightVelocity;
    private float lastRepositionTime;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        buoyancy = GetComponent<Buoyancy>();
        ownShip = GetComponent<Ship>();
        
        if (rb == null)
        {
            Debug.LogError($"[ShipMovement] No Rigidbody found on {gameObject.name}");
            enabled = false;
            return;
        }
        
        Vector3 startPos = transform.position;
        startPos.y = waterLevel + buoyancyOffset;
        transform.position = startPos;
        targetPosition = startPos;
        
        rb.drag = waterDrag;
        rb.angularDrag = waterDrag;
        rb.useGravity = false;
        
        Debug.Log($"[ShipMovement] Initialized on {gameObject.name}");
    }
    
    private void FixedUpdate()
    {
        if (isMoving)
        {
            if (targetShip != null && !targetShip.IsSinking)
            {
                UpdateCombatMovement();
            }
            else
            {
                MoveTowardsTarget();
            }
        }
        else
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 5f);
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 5f);
        }
        
        MaintainWaterLevel();
    }
    
    private void MaintainWaterLevel()
    {
        float targetHeight = waterLevel + buoyancyOffset;
        Vector3 newPosition = rb.position;
        newPosition.y = Mathf.SmoothDamp(rb.position.y, targetHeight, ref heightVelocity, heightSmoothTime);
        rb.MovePosition(newPosition);
    }
    
    public void SetTargetPosition(Vector3 position)
    {
        position.y = waterLevel + buoyancyOffset;
        targetPosition = position;
        targetShip = null;
        isMoving = true;
        currentSpeed = rb.linearVelocity.magnitude;
        Debug.Log($"[ShipMovement] Set target position for {gameObject.name} to {position}");
    }

    public void SetTargetPosition(Vector3 position, Ship target)
    {
        position.y = waterLevel + buoyancyOffset;
        targetPosition = position;
        targetShip = target;
        isMoving = true;
        currentSpeed = rb.linearVelocity.magnitude;
        Debug.Log($"[ShipMovement] Set combat target for {gameObject.name} to {target.ShipName}");

        // Set combat target in the combat system
        if (CombatSystem.Instance != null)
        {
            CombatSystem.Instance.SetCombatTarget(ownShip, target);
        }
    }
    
    private void UpdateCombatMovement()
    {
        if (targetShip == null) return;

        Vector3 directionToTarget = (targetShip.transform.position - transform.position);
        float distanceToTarget = directionToTarget.magnitude;

        // Calculate ideal combat position
        Vector3 idealPosition = CalculateIdealCombatPosition();
        
        // Check if we need to reposition
        bool needsRepositioning = ShouldReposition(distanceToTarget);

        if (needsRepositioning)
        {
            // Move towards ideal position
            targetPosition = idealPosition;
            MoveTowardsTarget();
            lastRepositionTime = Time.time;
        }
        else
        {
            // Maintain position and rotate to face target
            RotateTowardsTarget(targetShip.transform.position);
        }
    }

    private Vector3 CalculateIdealCombatPosition()
    {
        if (targetShip == null) return transform.position;

        // Get the direction from target to our ship
        Vector3 directionFromTarget = (transform.position - targetShip.transform.position).normalized;
        
        // Calculate the ideal position at optimal combat distance
        Vector3 idealPosition = targetShip.transform.position + directionFromTarget * optimalCombatDistance;
        idealPosition.y = waterLevel + buoyancyOffset;

        return idealPosition;
    }

    private bool ShouldReposition(float currentDistance)
    {
        // Check if we're too close or too far from optimal distance
        bool distanceInvalid = Mathf.Abs(currentDistance - optimalCombatDistance) > repositionThreshold;
        
        // Check if we're within the firing arc
        bool inFiringArc = IsInFiringArc();

        return distanceInvalid || !inFiringArc;
    }

    private bool IsInFiringArc()
    {
        if (targetShip == null || ownShip == null) return false;

        Vector3 directionToTarget = (targetShip.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        return angle <= ownShip.FiringArc * 0.5f;
    }
    
    private void MoveTowardsTarget()
    {
        if (rb == null) return;
        
        Vector3 directionToTarget = (targetPosition - transform.position);
        directionToTarget.y = 0;
        float distanceToTarget = directionToTarget.magnitude;
        
        float currentStoppingDistance = targetShip != null ? combatStoppingDistance : stoppingDistance;

        if (distanceToTarget <= currentStoppingDistance)
        {
            if (rb.linearVelocity.magnitude < stoppingThreshold)
            {
                StopShip();
                return;
            }
        }
        
        directionToTarget.Normalize();
        RotateTowardsTarget(targetPosition);
        
        float targetSpeed = distanceToTarget > currentStoppingDistance ? 
            maxSpeed : 
            maxSpeed * (distanceToTarget / currentStoppingDistance);
        
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref currentVelocity.x, velocitySmoothTime);
        
        float distanceRatio = Mathf.Clamp01(distanceToTarget / currentStoppingDistance);
        Vector3 movementForce = transform.forward * currentSpeed * distanceRatio;
        
        rb.AddForce(movementForce * (1f - waterDrag * Time.fixedDeltaTime), ForceMode.Acceleration);
        
        if (Debug.isDebugBuild)
        {
            Debug.DrawLine(transform.position, targetPosition, Color.yellow);
            Debug.DrawRay(transform.position, movementForce, Color.green);
        }
    }

    private void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 directionToTarget = (targetPos - transform.position);
        directionToTarget.y = 0;
        directionToTarget.Normalize();

        float targetAngle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
        float smoothedRotation = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetAngle,
            ref rotationVelocity,
            rotationSmoothTime
        );
        
        transform.rotation = Quaternion.Euler(0, smoothedRotation, 0);
    }
    
    private void StopShip()
    {
        isMoving = false;
        currentSpeed = 0f;
        currentVelocity = Vector3.zero;
        rotationVelocity = 0f;
        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        Debug.Log($"[ShipMovement] {gameObject.name} reached target and stopped");
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        // Draw target position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
        
        // Draw stopping distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosition, targetShip != null ? combatStoppingDistance : stoppingDistance);
        
        // Draw water level
        Gizmos.color = Color.blue;
        Vector3 waterPos = transform.position;
        waterPos.y = waterLevel;
        Gizmos.DrawWireCube(waterPos, new Vector3(2f, 0.1f, 2f));

        // Draw combat-related gizmos
        if (targetShip != null)
        {
            // Draw optimal combat distance
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetShip.transform.position, optimalCombatDistance);

            // Draw reposition threshold
            Gizmos.color = new Color(1, 0.5f, 0, 0.5f);
            Gizmos.DrawWireSphere(targetShip.transform.position, optimalCombatDistance + repositionThreshold);
            Gizmos.DrawWireSphere(targetShip.transform.position, optimalCombatDistance - repositionThreshold);

            // Draw ideal combat position
            Vector3 idealPos = CalculateIdealCombatPosition();
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(idealPos, 1f);
            Gizmos.DrawLine(transform.position, idealPos);
        }
    }
}