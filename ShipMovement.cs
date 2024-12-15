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
    [SerializeField] private float heightSmoothTime = 0.5f;  // Added for smooth height adjustments
    
    [Header("Water Interaction")]
    [SerializeField] private float waterLevel = 0f;  // Default water level
    [SerializeField] private float buoyancyOffset = 0.5f;  // How much of the ship sits below water
    [SerializeField] private float waterDrag = 0.95f;  // Added water resistance
    
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
        rb.useGravity = false;  // We'll handle vertical positioning ourselves
        
        Debug.Log($"[ShipMovement] Initialized on {gameObject.name}");
    }
    
    private void FixedUpdate()
    {
        if (isMoving)
        {
            MoveTowardsTarget();
        }
        
        // Always maintain proper height above water
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
        // Ensure target is at water level
        position.y = waterLevel + buoyancyOffset;
        targetPosition = position;
        isMoving = true;
        Debug.Log($"[ShipMovement] Set target position for {gameObject.name} to {position}");
    }
    
    private void MoveTowardsTarget()
    {
        if (rb == null) return;
        
        Vector3 directionToTarget = (targetPosition - transform.position);
        directionToTarget.y = 0; // Keep movement in horizontal plane
        float distanceToTarget = directionToTarget.magnitude;
        
        if (distanceToTarget < 0.1f)
        {
            return;
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
        float targetSpeed = distanceToTarget > stoppingDistance ? maxSpeed : maxSpeed * (distanceToTarget / stoppingDistance);
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref currentVelocity.x, velocitySmoothTime);
        
        // Calculate movement force
        Vector3 movementForce = transform.forward * currentSpeed;
        
        // Apply force with water resistance
        rb.AddForce(movementForce * (1f - waterDrag * Time.fixedDeltaTime), ForceMode.Acceleration);
        
        // Handle stopping
        if (distanceToTarget <= stoppingDistance && currentSpeed < 0.1f)
        {
            isMoving = false;
            currentSpeed = 0f;
            Debug.Log($"[ShipMovement] {gameObject.name} reached target");
        }
        
        if (Debug.isDebugBuild)
        {
            Debug.DrawLine(transform.position, targetPosition, Color.yellow);
            Debug.DrawRay(transform.position, movementForce, Color.green);
        }
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