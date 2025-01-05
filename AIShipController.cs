// AIShipController.cs
using UnityEngine;
using CSM.Base;

public class AIShipController : MonoBehaviour
{
    [SerializeField] private float decisionInterval = 2f;
    [SerializeField] private float patrolRadius = 100f;
    [SerializeField] private float detectionRange = 50f;

    private IMoveable _movement;
    private Vector3 _homePosition;
    private float _nextDecisionTime;
    private Ship _controlledShip;

    public void Initialize(Ship ship)
    {
        _controlledShip = ship;
         _movement = GetComponent<ShipMovement>();
         if (_movement == null)
        {
           Debug.LogError("IMoveable component missing!");
            enabled = false;
            return;
       }

        _homePosition = transform.position;
       _nextDecisionTime = Time.time + Random.Range(0f, decisionInterval);
    }

    void Update()
    {
        if (Time.time >= _nextDecisionTime)
        {
             if (!_movement.IsMoving)
            {
                Patrol();
            }
           _nextDecisionTime = Time.time + decisionInterval;
        }
    }

    private void Patrol()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        Vector3 newPosition = _homePosition + new Vector3(randomCircle.x, 0, randomCircle.y);
       _movement.SetDestination(newPosition);
    }
}