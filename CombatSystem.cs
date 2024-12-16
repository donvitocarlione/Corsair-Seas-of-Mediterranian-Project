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

    [Header("Debug Settings")]
    [SerializeField] private bool ignoreFactionChecks = true; // Added for testing
    [SerializeField] private bool debugMode = true;

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

    private void Start()
    {
        // Force factions to be at war for testing
        if (ignoreFactionChecks && FactionManager.Instance != null)
        {
            FactionManager.Instance.UpdateFactionRelation(FactionType.Pirates, FactionType.Merchants, 0);
            FactionManager.Instance.UpdateFactionRelation(FactionType.Pirates, FactionType.RoyalNavy, 0);
            FactionManager.Instance.UpdateFactionRelation(FactionType.Pirates, FactionType.Ottomans, 0);
            FactionManager.Instance.UpdateFactionRelation(FactionType.Pirates, FactionType.Venetians, 0);
        }
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

        if (target.IsSinking)
        {
            Debug.Log($"[CombatSystem] Cannot target sinking ship {target.ShipName}");
            return;
        }

        if (attacker == target)
        {
            Debug.LogWarning("[CombatSystem] Ship cannot target itself");
            return;
        }

        // Check faction relationships
        if (!ignoreFactionChecks && attacker.ShipOwner != null && target.ShipOwner != null)
        {
            var attackerFaction = attacker.ShipOwner.Faction;
            var targetFaction = target.ShipOwner.Faction;

            if (debugMode)
            {
                Debug.Log($"[CombatSystem] Checking factions: {attackerFaction} vs {targetFaction}");
                if (FactionManager.Instance != null)
                {
                    float relation = FactionManager.Instance.GetRelationBetweenFactions(attackerFaction, targetFaction);
                    Debug.Log($"[CombatSystem] Faction relation: {relation}");
                    Debug.Log($"[CombatSystem] Are factions at war: {FactionManager.Instance.AreFactionsAtWar(attackerFaction, targetFaction)}");
                }
            }

            if (attackerFaction == targetFaction)
            {
                Debug.Log("[CombatSystem] Cannot target ships in the same faction");
                return;
            }

            if (FactionManager.Instance != null && !FactionManager.Instance.AreFactionsAtWar(attackerFaction, targetFaction))
            {
                Debug.Log($"[CombatSystem] Factions {attackerFaction} and {targetFaction} are not at war");
                if (!ignoreFactionChecks) return;
            }
        }

        combatTargets[attacker] = target;
        lastRangeCheckTime[attacker] = 0f;
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

            if (attacker == null || target == null || attacker.IsSinking || target.IsSinking)
            {
                shipsToRemove.Add(attacker);
                continue;
            }

            if (Time.time - lastRangeCheckTime.GetValueOrDefault(attacker, 0f) >= RANGE_CHECK_INTERVAL)
            {
                CheckRangeAndFire(attacker, target);
                lastRangeCheckTime[attacker] = Time.time;
            }
        }

        foreach (var ship in shipsToRemove)
        {
            ClearCombatTarget(ship);
        }
    }

    private void CheckRangeAndFire(Ship attacker, Ship target)
    {
        float distanceToTarget = Vector3.Distance(attacker.transform.position, target.transform.position);

        if (debugMode)
        {
            Debug.Log($"[CombatSystem] Distance to target: {distanceToTarget}, Attack Range: {attacker.AttackRange}");
        }

        if (distanceToTarget <= attacker.AttackRange)
        {
            Vector3 directionToTarget = (target.transform.position - attacker.transform.position).normalized;
            float angleToTarget = Vector3.Angle(attacker.transform.forward, directionToTarget);

            if (debugMode)
            {
                Debug.Log($"[CombatSystem] Angle to target: {angleToTarget}, Firing Arc: {attacker.FiringArc}");
            }

            if (angleToTarget <= attacker.FiringArc * 0.5f)
            {
                if (attacker.CanFire)
                {
                    attacker.Fire(target);
                }
                else if (debugMode)
                {
                    Debug.Log($"[CombatSystem] {attacker.ShipName} cannot fire (reload/ammo)");
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
                Gizmos.color = Color.red;
                Gizmos.DrawLine(kvp.Key.transform.position, kvp.Value.transform.position);

                Gizmos.color = new Color(1, 0, 0, 0.2f);
                Gizmos.DrawWireSphere(kvp.Key.transform.position, kvp.Key.AttackRange);

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