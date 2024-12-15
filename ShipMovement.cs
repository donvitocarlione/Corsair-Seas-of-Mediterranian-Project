using UnityEngine;

public class ShipMovement : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float stoppingDistance = 1f;

    private Vector3 targetPosition;
    private float currentSpeed;
    private bool isMoving;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError($"[ShipMovement] No Rigidbody found on {gameObject.name}");
        }
        targetPosition = transform.position;
        Debug.Log($"[ShipMovement] Initialized on {gameObject.name}");
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            MoveTowardsTarget();
        }
    }

    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
        isMoving = true;
        Debug.Log($"[ShipMovement] Set target position for {gameObject.name} to {position}");
    }

    private void MoveTowardsTarget()
    {
        if (rb == null) return;

        Vector3 direction = (targetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPosition);

        // Rotate towards target
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        // Update speed
        if (distance > stoppingDistance)
        {
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.fixedDeltaTime, maxSpeed);
        }
        else
        {
            currentSpeed = Mathf.Max(currentSpeed - acceleration * Time.fixedDeltaTime, 0);
            if (currentSpeed == 0)
            {
                isMoving = false;
                Debug.Log($"[ShipMovement] {gameObject.name} reached target");
            }
        }

        // Apply movement
        Vector3 movement = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        Debug.Log($"[ShipMovement] {gameObject.name} moving at speed {currentSpeed} towards {targetPosition}");
    }
}