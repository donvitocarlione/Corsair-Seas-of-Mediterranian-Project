using UnityEngine;

public class ShipMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float stoppingDistance = 1f;
    
    [Header("Movement Smoothing")]
    [SerializeField] private float rotationSmoothTime = 0.3f;
    [SerializeField] private float velocitySmoothTime = 0.3f;
    [SerializeField] private float heightSmoothTime = 0.5f;
    
    [Header("Water Interaction")]
    [SerializeField] private float waterLevel = 0f;
    [SerializeField] private float buoyancyOffset = 0.5f;
    [SerializeField] private float waterDrag = 0.95f;
    [SerializeField] private float stoppingThreshold = 0.1f;  // Added threshold for stopping
    
    private Vector3 targetPosition;
    private float currentSpeed;
    private bool isMoving;
    private Rigidbody rb;
    private Buoyancy buoyancy;
    
    // Smoothing variables
    private Vector3 currentVelocity;
    private float rotationVelocity;
    private float heightVelocity;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        buoyancy = GetComponent<Buoyancy>();
        
        if (rb == null)
        {
            Debug.LogError($"[ShipMovement] No Rigidbody found on {gameObject.name}");
            enabled = false;
            return;
        }
        
        // Initialize position at correct water level
        Vector3 startPos = transform.position;
        startPos.y = waterLevel + buoyancyOffset;
        transform.position = startPos;
        targetPosition = startPos;
        
        // Configure rigidbody for water movement
        rb.drag = waterDrag;
        rb.angularDrag = waterDrag;
        rb.useGravity = false;
        
        Debug.Log($"[ShipMovement] Initialized on {gameObject.name}");
    }
    
    private void FixedUpdate()
    {
        if (isMoving)
        {
            MoveTowardsTarget();
        }
        else
        {
            // When not moving, actively dampen any residual velocity
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
    
    public void SetTargetPosition(Vector3 position)
    {
        position.y = waterLevel + buoyancyOffset;
        targetPosition = position;
        isMoving = true;
        currentSpeed = rb.velocity.magnitude;  // Initialize with current speed for smooth transitions
        Debug.Log($"[ShipMovement] Set target position for {gameObject.name} to {position}");
    }
    
    private void MoveTowardsTarget()
    {
        if (rb == null) return;
        
        Vector3 directionToTarget = (targetPosition - transform.position);
        directionToTarget.y = 0;
        float distanceToTarget = directionToTarget.magnitude;
        
        // Check if we should stop
        if (distanceToTarget <= stoppingDistance)
        {
            if (rb.velocity.magnitude < stoppingThreshold)
            {
                StopShip();
                return;
            }
        }
        
        directionToTarget.Normalize();
        
        // Calculate target rotation
        float targetAngle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
        float smoothedRotation = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetAngle,
            ref rotationVelocity,
            rotationSmoothTime
        );
        
        // Apply rotation
        transform.rotation = Quaternion.Euler(0, smoothedRotation, 0);
        
        // Update speed based on distance
        float targetSpeed = distanceToTarget > stoppingDistance ? 
            maxSpeed : 
            maxSpeed * (distanceToTarget / stoppingDistance);
        
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref currentVelocity.x, velocitySmoothTime);
        
        // Calculate movement force with gradual reduction near target
        float distanceRatio = Mathf.Clamp01(distanceToTarget / stoppingDistance);
        Vector3 movementForce = transform.forward * currentSpeed * distanceRatio;
        
        // Apply force with water resistance
        rb.AddForce(movementForce * (1f - waterDrag * Time.fixedDeltaTime), ForceMode.Acceleration);
        
        if (Debug.isDebugBuild)
        {
            Debug.DrawLine(transform.position, targetPosition, Color.yellow);
            Debug.DrawRay(transform.position, movementForce, Color.green);
        }
    }
    
    private void StopShip()
    {
        isMoving = false;
        currentSpeed = 0f;
        currentVelocity = Vector3.zero;
        rotationVelocity = 0f;
        
        // Immediately zero out physics velocities
        rb.velocity = Vector3.zero;
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
        Gizmos.DrawWireSphere(targetPosition, stoppingDistance);
        
        // Draw water level
        Gizmos.color = Color.blue;
        Vector3 waterPos = transform.position;
        waterPos.y = waterLevel;
        Gizmos.DrawWireCube(waterPos, new Vector3(2f, 0.1f, 2f));
    }
}