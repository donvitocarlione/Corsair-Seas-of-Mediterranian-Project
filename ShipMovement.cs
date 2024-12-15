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
    
    private Vector3 targetPosition;
    private float currentSpeed;
    private bool isMoving;
    private Rigidbody rb;
    private Buoyancy buoyancy;
    
    // Smoothing variables
    private Vector3 currentVelocity;
    private float rotationVelocity;
    
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
        
        targetPosition = transform.position;
        Debug.Log($"[ShipMovement] Initialized on {gameObject.name}");
    }
    
    private void FixedUpdate()
    {
        if (isMoving && (buoyancy == null || buoyancy.isInWater))
        {
            MoveTowardsTarget();
        }
    }
    
    public void SetTargetPosition(Vector3 position)
    {
        // Keep the target at water level
        if (buoyancy != null)
        {
            position.y = buoyancy.WaterLevel;
        }
        
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
            // Prevent jittering when very close to target
            return;
        }
        
        // Normalize direction after calculating distance
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
        
        // Apply force instead of direct position change
        rb.AddForce(movementForce, ForceMode.Acceleration);
        
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
    }
}
