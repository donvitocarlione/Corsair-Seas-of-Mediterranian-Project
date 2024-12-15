using UnityEngine;

public class AIShipController : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 50f;
    [SerializeField] private float targetUpdateInterval = 5f;
    [SerializeField] private float minDistanceToTarget = 5f;
    [SerializeField] private bool debugMode = false;
    
    private Ship controlledShip;
    private ShipMovement shipMovement;
    private Vector3 homePosition;
    private Vector3 currentTarget;
    private float nextTargetUpdate;
    private bool isWaitingAtPoint;
    private float waitStartTime;
    private const float waitTimeAtPoint = 3f;
    
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
        
        if (isWaitingAtPoint)
        {
            if (Time.time - waitStartTime >= waitTimeAtPoint)
            {
                isWaitingAtPoint = false;
                SetNewPatrolTarget();
                if (debugMode)
                {
                    Debug.Log($"[AIShipController] {gameObject.name} finished waiting, moving to new target");
                }
            }
            return;
        }
        
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget);
        
        if (distanceToTarget <= minDistanceToTarget)
        {
            isWaitingAtPoint = true;
            waitStartTime = Time.time;
            if (debugMode)
            {
                Debug.Log($"[AIShipController] {gameObject.name} reached target, waiting at point");
            }
            return;
        }
        
        if (Time.time >= nextTargetUpdate)
        {
            SetNewPatrolTarget();
            nextTargetUpdate = Time.time + targetUpdateInterval;
        }
        
        shipMovement.SetTargetPosition(currentTarget);
    }
    
    private void SetNewPatrolTarget()
    {
        // Generate points until we find one that's not too close to current position
        Vector3 newTarget;
        int attempts = 0;
        const int maxAttempts = 10;
        
        do
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(patrolRadius * 0.3f, patrolRadius);
            newTarget = homePosition + new Vector3(randomCircle.x, 0, randomCircle.y);
            attempts++;
        }
        while (Vector3.Distance(transform.position, newTarget) < minDistanceToTarget && attempts < maxAttempts);
        
        currentTarget = newTarget;
        
        if (debugMode)
        {
            Debug.Log($"[AIShipController] {gameObject.name} new patrol target set to {currentTarget}");
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        // Draw patrol radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(homePosition, patrolRadius);
        
        // Draw current target
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(currentTarget, 1f);
        Gizmos.DrawLine(transform.position, currentTarget);
        
        // Draw minimum distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minDistanceToTarget);
    }
}
