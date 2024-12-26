using UnityEngine;

public class AIShipController : MonoBehaviour
{
    public Ship controlledShip;
    public float decisionInterval = 2f;
    public float patrolRadius = 100f;
    public float detectionRange = 50f;

    private ShipMovement movement;
    private Vector3 homePosition;
    private float nextDecisionTime;

    public void Initialize(Ship ship)
    {
        controlledShip = ship;
        movement = GetComponent<ShipMovement>();
        if (movement == null)
        {
            Debug.LogError("ShipMovement component missing!");
            enabled = false;
            return;
        }

        homePosition = transform.position;
        nextDecisionTime = Time.time + Random.Range(0f, decisionInterval);
    }

    void Update()
    {
        if (Time.time >= nextDecisionTime)
        {
            // Simple patrol behavior
            if (!movement.isMoving)
            {
                Patrol();
            }
            nextDecisionTime = Time.time + decisionInterval;
        }
    }

    private void Patrol()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        Vector3 newPosition = homePosition + new Vector3(randomCircle.x, 0, randomCircle.y);
        movement.SetTargetPosition(newPosition);
    }
}
