using UnityEngine;
using CSM.Base;

public class AIShipController : MonoBehaviour
{
    public Ship controlledShip;
    public float decisionInterval = 2f;
    public float patrolRadius = 100f;
    public float detectionRange = 50f;

    private IMoveable movement;  // Changed to IMoveable
    private Vector3 homePosition;
    private float nextDecisionTime;

    public void Initialize(Ship ship)
    {
        controlledShip = ship;
        movement = GetComponent<ShipMovement>();  // Correctly gets ShipMovement which implements IMoveable
        if (movement == null)
        {
            Debug.LogError("IMoveable component missing!");
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
             if (!movement.IsMoving) // Changed to IsMoving property
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
        movement.SetDestination(newPosition);  // Changed to SetDestination method
    }
}