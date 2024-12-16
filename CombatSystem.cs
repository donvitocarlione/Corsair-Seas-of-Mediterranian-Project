using UnityEngine;
using System.Collections.Generic;

public class CombatSystem : MonoBehaviour
{
    private static CombatSystem instance;
    public static CombatSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CombatSystem>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("CombatSystem");
                    instance = obj.AddComponent<CombatSystem>();
                }
            }
            return instance;
        }
    }

    private Dictionary<Ship, Ship> combatTargets = new Dictionary<Ship, Ship>();
    private Dictionary<Ship, float> lastRangeCheckTime = new Dictionary<Ship, float>();
    private const float RANGE_CHECK_INTERVAL = 0.5f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        UpdateCombatStates();
    }

    public void SetCombatTarget(Ship attacker, Ship target)
    {
        if (attacker == null || target == null)
        {
            Debug.LogWarning("[CombatSystem] Attempted to set null attacker or target");
            return;
        }

        // Don't allow targeting of sinking ships
        if (target.IsSinking)
        {
            Debug.Log($"[CombatSystem] Cannot target sinking ship {target.ShipName}");
            return;
        }

        // Don't allow ships to target themselves
        if (attacker == target)
        {
            Debug.LogWarning("[CombatSystem] Ship cannot target itself");
            return;
        }

        // Check if ships are in the same faction
        if (attacker.ShipOwner != null && target.ShipOwner != null && 
            attacker.ShipOwner.Faction == target.ShipOwner.Faction)
        {
            Debug.Log("[CombatSystem] Cannot target ships in the same faction");
            return;
        }

        combatTargets[attacker] = target;
        lastRangeCheckTime[attacker] = 0f; // Force immediate range check
        Debug.Log($"[CombatSystem] {attacker.ShipName} targeting {target.ShipName}");
    }

    public void ClearCombatTarget(Ship attacker)
    {
        if (attacker != null && combatTargets.ContainsKey(attacker))
        {
            combatTargets.Remove(attacker);
            lastRangeCheckTime.Remove(attacker);
            Debug.Log($"[CombatSystem] Cleared combat target for {attacker.ShipName}");
        }
    }

    public Ship GetCurrentTarget(Ship attacker)
    {
        return combatTargets.TryGetValue(attacker, out Ship target) ? target : null;
    }

    private void UpdateCombatStates()
    {
        List<Ship> shipsToRemove = new List<Ship>();

        foreach (var kvp in combatTargets)
        {
            Ship attacker = kvp.Key;
            Ship target = kvp.Value;

            // Remove invalid targets
            if (attacker == null || target == null || attacker.IsSinking || target.IsSinking)
            {
                shipsToRemove.Add(attacker);
                continue;
            }

            // Check if it's time to update this ship's range check
            if (Time.time - lastRangeCheckTime.GetValueOrDefault(attacker, 0f) >= RANGE_CHECK_INTERVAL)
            {
                CheckRangeAndFire(attacker, target);
                lastRangeCheckTime[attacker] = Time.time;
            }
        }

        // Clean up invalid entries
        foreach (var ship in shipsToRemove)
        {
            ClearCombatTarget(ship);
        }
    }

    private void CheckRangeAndFire(Ship attacker, Ship target)
    {
        float distanceToTarget = Vector3.Distance(attacker.transform.position, target.transform.position);

        if (distanceToTarget <= attacker.AttackRange)
        {
            // Check if target is within firing arc
            Vector3 directionToTarget = (target.transform.position - attacker.transform.position).normalized;
            float angleToTarget = Vector3.Angle(attacker.transform.forward, directionToTarget);

            if (angleToTarget <= attacker.FiringArc * 0.5f)
            {
                if (attacker.CanFire)
                {
                    attacker.Fire(target);
                }
            }
            else
            {
                Debug.Log($"[CombatSystem] {attacker.ShipName} target not in firing arc (angle: {angleToTarget})");
            }
        }
    }

    public bool IsInCombat(Ship ship)
    {
        return combatTargets.ContainsKey(ship) || combatTargets.ContainsValue(ship);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        foreach (var kvp in combatTargets)
        {
            if (kvp.Key != null && kvp.Value != null)
            {
                // Draw line between ships in combat
                Gizmos.color = Color.red;
                Gizmos.DrawLine(kvp.Key.transform.position, kvp.Value.transform.position);

                // Draw attack range
                Gizmos.color = new Color(1, 0, 0, 0.2f);
                Gizmos.DrawWireSphere(kvp.Key.transform.position, kvp.Key.AttackRange);

                // Draw firing arc
                DrawFiringArc(kvp.Key);
            }
        }
    }

    private void DrawFiringArc(Ship ship)
    {
        if (ship == null) return;

        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Vector3 forward = ship.transform.forward;
        float radius = ship.AttackRange;
        float angleStep = 5f;

        for (float angle = -ship.FiringArc * 0.5f; angle <= ship.FiringArc * 0.5f; angle += angleStep)
        {
            Vector3 direction = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 nextDirection = Quaternion.Euler(0, angle + angleStep, 0) * forward;

            Vector3 point1 = ship.transform.position;
            Vector3 point2 = ship.transform.position + direction * radius;
            Vector3 point3 = ship.transform.position + nextDirection * radius;

            Gizmos.DrawLine(point1, point2);
            Gizmos.DrawLine(point2, point3);
        }
    }
}