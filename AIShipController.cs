using UnityEngine;

public class AIShipController : MonoBehaviour
{
    private Ship controlledShip;
    private ShipMovement shipMovement;
    private Vector3 homePosition;
    private Vector3 currentTarget;

    [SerializeField] private bool debugMode = false;
    [SerializeField] private float patrolRadius = 100f;
    [SerializeField] private float targetReachedThreshold = 5f;

    public void Initialize(Ship ship)
    {
        controlledShip = ship;
        shipMovement = ship.GetComponent<ShipMovement>();
        homePosition = transform.position;
        SetNewPatrolTarget();
        
        if (debugMode)
        {
            Debug.Log($"[AIShipController] Initialized on {gameObject.name} at {homePosition}");
        }
    }

    private void Update()
    {
        if (shipMovement == null) return;

        // Check if we've reached the current target
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget);
        if (distanceToTarget <= targetReachedThreshold)
        {
            SetNewPatrolTarget();
        }

        // Move towards the current target
        Vector3 direction = (currentTarget - transform.position).normalized;
        shipMovement.SetMovementDirection(direction);
    }

    private void SetNewPatrolTarget()
    {
        // Generate a random point within patrol radius
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        currentTarget = homePosition + new Vector3(randomCircle.x, 0, randomCircle.y);

        if (debugMode)
        {
            Debug.Log($"[AIShipController] New patrol target set at {currentTarget}");
        }
    }
}
