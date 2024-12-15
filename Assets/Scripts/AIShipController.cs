using UnityEngine;

public class AIShipController : MonoBehaviour
{
    private Ship controlledShip;
    private ShipMovement shipMovement;
    private Vector3 homePosition;
    private float patrolRadius = 50f;
    private Vector3 currentTarget;
    private float targetUpdateInterval = 5f;
    private float nextTargetUpdate;

    public void Initialize(Ship ship)
    {
        controlledShip = ship;
        shipMovement = ship.GetComponent<ShipMovement>();
        homePosition = transform.position;
        SetNewPatrolTarget();
    }

    private void Update()
    {
        if (Time.time >= nextTargetUpdate)
        {
            SetNewPatrolTarget();
            nextTargetUpdate = Time.time + targetUpdateInterval;
        }

        if (shipMovement != null)
        {
            shipMovement.SetTargetPosition(currentTarget);
        }
    }

    private void SetNewPatrolTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        currentTarget = homePosition + new Vector3(randomCircle.x, 0, randomCircle.y);
    }
}