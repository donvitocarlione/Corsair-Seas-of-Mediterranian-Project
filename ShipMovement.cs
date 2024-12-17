using UnityEngine;

public class ShipMovement : MonoBehaviour
{
    // [Previous code remains the same until OnDrawGizmosSelected method]
    
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        // Draw target position
        Gizmos.color = currentTargetType == TargetType.Combat ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
        
        // Draw stopping distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosition, currentTargetType == TargetType.Combat ? combatStoppingDistance : stoppingDistance);
        
        // Draw water level
        Gizmos.color = Color.blue;
        Vector3 waterPos = transform.position;
        waterPos.y = waterLevel;
        Gizmos.DrawWireCube(waterPos, new Vector3(2f, 0.1f, 2f));

        if (targetShip != null && currentTargetType == TargetType.Combat)
        {
            // Draw optimal combat distance
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetShip.transform.position, optimalCombatDistance);

            // Draw reposition threshold
            Gizmos.color = new Color(1, 0.5f, 0, 0.5f);
            Gizmos.DrawWireSphere(targetShip.transform.position, optimalCombatDistance + repositionThreshold);
            Gizmos.DrawWireSphere(targetShip.transform.position, optimalCombatDistance - repositionThreshold);

            // Draw ideal combat position
            Vector3 idealPos = CalculateIdealCombatPosition();
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(idealPos, 1f);
            Gizmos.DrawLine(transform.position, idealPos);

            // Draw firing arc
            if (ownShip != null)
            {
                Gizmos.color = IsInFiringArc() ? Color.green : Color.red;
                Vector3 forward = transform.forward;
                float radius = ownShip.AttackRange;
                float arcAngle = ownShip.FiringArc * 0.5f;
                int segments = 20;

                for (int i = 0; i < segments; i++)
                {
                    float angle1 = -arcAngle + (arcAngle * 2 * i) / segments;
                    float angle2 = -arcAngle + (arcAngle * 2 * (i + 1)) / segments;

                    Vector3 direction1 = Quaternion.Euler(0, angle1, 0) * forward;
                    Vector3 direction2 = Quaternion.Euler(0, angle2, 0) * forward;

                    Vector3 point1 = transform.position;
                    Vector3 point2 = transform.position + direction1 * radius;
                    Vector3 point3 = transform.position + direction2 * radius;

                    Gizmos.DrawLine(point1, point2);
                    Gizmos.DrawLine(point2, point3);
                }
            }
        }
    }

    public bool IsInCombat() => inCombat;
    public Ship GetTargetShip() => targetShip;
    public TargetType GetCurrentTargetType() => currentTargetType;
}
