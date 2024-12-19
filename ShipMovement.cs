using UnityEngine;

public class ShipMovement : MonoBehaviour
{
    public enum TargetType
    {
        None,
        Movement,
        Combat
    }

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
    [SerializeField] private float optimalCombatDistance = 15f;
    [SerializeField] private float repositionThreshold = 5f;
    [SerializeField] private float disengageTime = 8f;
    [SerializeField] private float minEngageTime = 2f;
    
    private Vector3 targetPosition;
    private float currentSpeed;
    private bool isMoving;
    private Rigidbody rb;
    private Ship targetShip;
    private Ship ownShip;
    private TargetType currentTargetType = TargetType.None;
    private bool inCombat;
    
    // Smoothing variables
    private Vector3 currentVelocity;
    private float rotationVelocity;
    private float heightVelocity;
    private float lastRepositionTime;
    private float lastFireTime;
    private float engageStartTime;
    private bool hasFired;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
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
        
        Debug.Log($"[ShipMovement] Initialized {gameObject.name} with maxSpeed={maxSpeed}, stoppingDistance={stoppingDistance}");
    }
    
    private void FixedUpdate()
    {
        if (isMoving)
        {
            if (currentTargetType == TargetType.Combat && targetShip != null && !targetShip.IsSinking)
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
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 5f);
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

    private void UpdateCombatMovement()
    {
        if (targetShip == null) return;

        Vector3 directionToTarget = (targetShip.transform.position - transform.position);
        float distanceToTarget = directionToTarget.magnitude;

        Vector3 idealPosition = CalculateIdealCombatPosition();
        bool needsRepositioning = ShouldReposition(distanceToTarget);

        if (needsRepositioning)
        {
            targetPosition = idealPosition;
            MoveTowardsTarget();
            lastRepositionTime = Time.time;
        }
        else
        {
            RotateTowardsTarget(targetShip.transform.position);
            if (ShouldFire())
            {
                FireAtTarget();
            }
        }

        if (ShouldDisengage())
        {
            ClearCombatTarget();
        }
    }

    private Vector3 CalculateIdealCombatPosition()
    {
        if (targetShip == null) return transform.position;

        Vector3 directionFromTarget = (transform.position - targetShip.transform.position).normalized;
        Vector3 idealPosition = targetShip.transform.position + directionFromTarget * optimalCombatDistance;
        idealPosition.y = waterLevel + buoyancyOffset;

        return idealPosition;
    }

    private bool ShouldReposition(float currentDistance)
    {
        if (Time.time - lastRepositionTime < minEngageTime) return false;

        bool distanceInvalid = Mathf.Abs(currentDistance - optimalCombatDistance) > repositionThreshold;
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

    private bool ShouldFire()
    {
        if (ownShip == null || targetShip == null) return false;
        if (!IsInFiringArc()) return false;
        if (Time.time - lastFireTime < minEngageTime) return false;
        return ownShip.CanFire;
    }

    private void FireAtTarget()
    {
        if (ownShip == null || targetShip == null) return;
        if (ownShip.CanFire)
        {
            ownShip.Fire(targetShip);
            lastFireTime = Time.time;
            hasFired = true;
        }
    }

    private bool ShouldDisengage()
    {
        if (ownShip == null || targetShip == null) return true;
        
        if (hasFired && Time.time - lastFireTime > disengageTime)
        {
            return true;
        }

        return false;
    }
    
    private void MoveTowardsTarget()
    {
        if (rb == null) return;
        
        Vector3 directionToTarget = (targetPosition - transform.position);
        directionToTarget.y = 0;
        float distanceToTarget = directionToTarget.magnitude;
        
        float currentStoppingDistance = currentTargetType == TargetType.Combat ? combatStoppingDistance : stoppingDistance;

        if (distanceToTarget <= currentStoppingDistance)
        {
            if (rb.velocity.magnitude < stoppingThreshold)
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
    }

    private void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 directionToTarget = (targetPos - transform.position);
        directionToTarget.y = 0;
        directionToTarget.Normalize();

        float targetAngle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
        float currentAngle = transform.eulerAngles.y;
        float smoothedRotation = Mathf.SmoothDampAngle(
            currentAngle,
            targetAngle,
            ref rotationVelocity,
            rotationSmoothTime
        );
        
        transform.rotation = Quaternion.Euler(0, smoothedRotation, 0);
    }
    
    private void StopShip()
    {
        if (currentTargetType != TargetType.Combat)
        {
            isMoving = false;
            currentSpeed = 0f;
            currentVelocity = Vector3.zero;
            rotationVelocity = 0f;
            
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            Debug.Log($"[ShipMovement] {gameObject.name} reached target and stopped");
        }
    }

    public void SetTargetPosition(Vector3 position)
    {
        position.y = waterLevel + buoyancyOffset;
        targetPosition = position;
        targetShip = null;
        isMoving = true;
        currentSpeed = rb.velocity.magnitude;
        currentTargetType = TargetType.Movement;
        inCombat = false;
        Debug.Log($"[ShipMovement] {gameObject.name} set movement target position to {position}");
    }

    public void SetTargetPosition(Vector3 position, Ship target)
    {
        position.y = waterLevel + buoyancyOffset;
        targetPosition = position;
        targetShip = target;
        isMoving = true;
        currentSpeed = rb.velocity.magnitude;
        currentTargetType = TargetType.Combat;
        inCombat = true;
        engageStartTime = Time.time;
        lastFireTime = Time.time - minEngageTime;
        hasFired = false;
        Debug.Log($"[ShipMovement] {gameObject.name} set combat target {target.ShipName} at position {position}");

        if (CombatSystem.Instance != null)
        {
            CombatSystem.Instance.SetCombatTarget(ownShip, target);
        }
    }

    public void ClearTargetPosition()
    {
        targetPosition = transform.position;
        targetShip = null;
        isMoving = false;
        currentSpeed = 0f;
        currentTargetType = TargetType.None;
        inCombat = false;
        hasFired = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Debug.Log($"[ShipMovement] {gameObject.name} cleared target position");
    }

    public void ClearCombatTarget()
    {
        if (currentTargetType == TargetType.Combat)
        {
            targetShip = null;
            currentTargetType = TargetType.Movement;
            inCombat = false;
            hasFired = false;
            Debug.Log($"[ShipMovement] {gameObject.name} cleared combat target, maintaining current movement");
        }
    }

    public void SetMovementDirection(Vector3 direction)
    {
        Vector3 worldDirection = direction.normalized;
        worldDirection.y = 0;
        
        Vector3 targetPos = transform.position + worldDirection * 10f;
        SetTargetPosition(targetPos);
        
        Debug.Log($"[ShipMovement] {gameObject.name} moving in direction {worldDirection}");
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosition, targetShip != null ? combatStoppingDistance : stoppingDistance);
        
        Gizmos.color = Color.blue;
        Vector3 waterPos = transform.position;
        waterPos.y = waterLevel;
        Gizmos.DrawWireCube(waterPos, new Vector3(2f, 0.1f, 2f));

        if (targetShip != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetShip.transform.position);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetShip.transform.position, optimalCombatDistance);
            
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(targetShip.transform.position, optimalCombatDistance + repositionThreshold);
            Gizmos.DrawWireSphere(targetShip.transform.position, optimalCombatDistance - repositionThreshold);
        }
    }

    public bool IsInCombat() => inCombat;
    public Ship GetTargetShip() => targetShip;
    public TargetType GetCurrentTargetType() => currentTargetType;
}