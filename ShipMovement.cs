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
    private Ship targetShip;
    private Ship ownShip;
    
    // Smoothing variables
    private Vector3 currentVelocity;
    private float rotationVelocity;
    private float heightVelocity;
    private float lastRepositionTime;
    
    void Start()
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
    
    void FixedUpdate()
    {
        Debug.Log($"[ShipMovement] {gameObject.name} FixedUpdate - isMoving: {isMoving}, velocity: {rb.velocity}, position: {transform.position}, targetPosition: {targetPosition}");
        
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
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 5f);
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 5f);
        }
        
        MaintainWaterLevel();
    }
    
    void MaintainWaterLevel()
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
        currentSpeed = rb.velocity.magnitude;
        Debug.Log($"[ShipMovement] {gameObject.name} set target position to {position}, current speed: {currentSpeed}");
    }

    public void SetTargetPosition(Vector3 position, Ship target)
    {
        position.y = waterLevel + buoyancyOffset;
        targetPosition = position;
        targetShip = target;
        isMoving = true;
        currentSpeed = rb.velocity.magnitude;
        Debug.Log($"[ShipMovement] {gameObject.name} set combat target {target.ShipName} at position {position}, current speed: {currentSpeed}");

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
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Debug.Log($"[ShipMovement] {gameObject.name} cleared target position");
    }
    
    void UpdateCombatMovement()
    {
        if (targetShip == null) return;

        Vector3 directionToTarget = (targetShip.transform.position - transform.position);
        float distanceToTarget = directionToTarget.magnitude;

        Debug.Log($"[ShipMovement] {gameObject.name} combat update - distance to {targetShip.ShipName}: {distanceToTarget}");

        Vector3 idealPosition = CalculateIdealCombatPosition();
        bool needsRepositioning = ShouldReposition(distanceToTarget);

        if (needsRepositioning)
        {
            Debug.Log($"[ShipMovement] {gameObject.name} repositioning to ideal position {idealPosition}");
            targetPosition = idealPosition;
            MoveTowardsTarget();
            lastRepositionTime = Time.time;
        }
        else
        {
            Debug.Log($"[ShipMovement] {gameObject.name} maintaining position and rotating to face target");
            RotateTowardsTarget(targetShip.transform.position);
        }
    }

    Vector3 CalculateIdealCombatPosition()
    {
        if (targetShip == null) return transform.position;

        Vector3 directionFromTarget = (transform.position - targetShip.transform.position).normalized;
        Vector3 idealPosition = targetShip.transform.position + directionFromTarget * optimalCombatDistance;
        idealPosition.y = waterLevel + buoyancyOffset;

        Debug.Log($"[ShipMovement] {gameObject.name} calculated ideal combat position: {idealPosition}");
        return idealPosition;
    }

    bool ShouldReposition(float currentDistance)
    {
        bool distanceInvalid = Mathf.Abs(currentDistance - optimalCombatDistance) > repositionThreshold;
        bool inFiringArc = IsInFiringArc();

        Debug.Log($"[ShipMovement] {gameObject.name} reposition check - Distance valid: {!distanceInvalid}, In firing arc: {inFiringArc}");
        return distanceInvalid || !inFiringArc;
    }

    bool IsInFiringArc()
    {
        if (targetShip == null || ownShip == null) return false;

        Vector3 directionToTarget = (targetShip.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        Debug.Log($"[ShipMovement] {gameObject.name} firing arc check - Angle to target: {angle}, Required arc: {ownShip.FiringArc * 0.5f}");
        return angle <= ownShip.FiringArc * 0.5f;
    }
    
    void MoveTowardsTarget()
    {
        if (rb == null) return;
        
        Vector3 directionToTarget = (targetPosition - transform.position);
        directionToTarget.y = 0;
        float distanceToTarget = directionToTarget.magnitude;
        
        float currentStoppingDistance = targetShip != null ? combatStoppingDistance : stoppingDistance;

        Debug.Log($"[ShipMovement] {gameObject.name} moving - Distance: {distanceToTarget}, Stopping distance: {currentStoppingDistance}");

        if (distanceToTarget <= currentStoppingDistance)
        {
            if (rb.velocity.magnitude < stoppingThreshold)
            {
                Debug.Log($"[ShipMovement] {gameObject.name} reached target - stopping");
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
        
        Debug.Log($"[ShipMovement] {gameObject.name} movement - Target speed: {targetSpeed}, Current speed: {currentSpeed}, Force: {movementForce}");
        
        rb.AddForce(movementForce * (1f - waterDrag * Time.fixedDeltaTime), ForceMode.Acceleration);
    }

    void RotateTowardsTarget(Vector3 targetPos)
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
        
        Debug.Log($"[ShipMovement] {gameObject.name} rotation - Current: {currentAngle}, Target: {targetAngle}, Smoothed: {smoothedRotation}");
        transform.rotation = Quaternion.Euler(0, smoothedRotation, 0);
    }
    
    void StopShip()
    {
        isMoving = false;
        currentSpeed = 0f;
        currentVelocity = Vector3.zero;
        rotationVelocity = 0f;
        
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        Debug.Log($"[ShipMovement] {gameObject.name} stopped at position {transform.position}");
    }
    
    void OnDrawGizmosSelected()
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