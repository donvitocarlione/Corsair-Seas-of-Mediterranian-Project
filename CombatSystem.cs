using UnityEngine;
using System.Collections.Generic;

public class CombatSystem : MonoBehaviour
{
    private static CombatSystem instance;
    public static CombatSystem Instance => instance;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool ignoreFactionChecks = false;
    [SerializeField] private bool logCombatDetails = true;
    
    [Header("Combat Parameters")]
    [SerializeField] private float combatUpdateInterval = 0.5f;
    [SerializeField] private float maxCombatRange = 200f;
    [SerializeField] private float minDisengageDistance = 250f;

    private Dictionary<Ship, Ship> combatTargets = new Dictionary<Ship, Ship>();
    private Dictionary<Ship, float> lastRangeCheckTime = new Dictionary<Ship, float>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        LogSystem("CombatSystem initialized");
    }

    private void Start()
    {
        ValidateFactionsSetup();
    }

    private void ValidateFactionsSetup()
    {
        if (FactionManager.Instance == null)
        {
            LogError("FactionManager not found in scene!");
            return;
        }

        var factions = System.Enum.GetValues(typeof(FactionType));
        LogSystem("Checking faction relations:");
        foreach (FactionType faction1 in factions)
        {
            foreach (FactionType faction2 in factions)
            {
                if (faction1 != faction2)
                {
                    float relation = FactionManager.Instance.GetRelationBetweenFactions(faction1, faction2);
                    LogSystem($"{faction1} vs {faction2}: Relation = {relation}, At War = {FactionManager.Instance.AreFactionsAtWar(faction1, faction2)}");
                }
            }
        }
    }

    public void SetCombatTarget(Ship attacker, Ship target)
    {
        if (!ValidateCombatParticipants(attacker, target)) return;
        
        if (ValidateFactionRelations(attacker, target))
        {
            InitiateCombat(attacker, target);
        }
    }

    private bool ValidateCombatParticipants(Ship attacker, Ship target)
    {
        if (attacker == null || target == null)
        {
            LogError("Invalid combat participants: Null attacker or target");
            return false;
        }

        if (target.IsSinking)
        {
            LogWarning($"Cannot target sinking ship: {target.ShipName}");
            return false;
        }

        if (attacker == target)
        {
            LogWarning("Ship cannot target itself");
            return false;
        }

        return true;
    }

    private bool ValidateFactionRelations(Ship attacker, Ship target)
    {
        if (ignoreFactionChecks) return true;

        var attackerFaction = attacker.ShipOwner?.Faction ?? FactionType.None;
        var targetFaction = target.ShipOwner?.Faction ?? FactionType.None;

        if (attackerFaction == targetFaction)
        {
            LogWarning($"Friendly fire prevented: {attackerFaction}");
            return false;
        }

        bool atWar = FactionManager.Instance?.AreFactionsAtWar(attackerFaction, targetFaction) ?? false;
        LogSystem($"Faction check: {attackerFaction} vs {targetFaction}, At War: {atWar}");
        return atWar || ignoreFactionChecks;
    }

    private void InitiateCombat(Ship attacker, Ship target)
    {
        combatTargets[attacker] = target;
        lastRangeCheckTime[attacker] = 0f;
        
        // Notify ship movement system
        var movement = attacker.GetComponent<ShipMovement>();
        if (movement != null)
        {
            movement.SetTargetPosition(target.transform.position);
            LogSystem($"{attacker.ShipName} pursuing {target.ShipName}");
        }
        
        LogSystem($"Combat initiated: {attacker.ShipName} targeting {target.ShipName}");
    }

    private void Update()
    {
        UpdateCombatStates();
    }

    private void UpdateCombatStates()
    {
        var shipsToRemove = new List<Ship>();

        foreach (var kvp in combatTargets)
        {
            Ship attacker = kvp.Key;
            Ship target = kvp.Value;

            if (!ValidateContinuedCombat(attacker, target))
            {
                shipsToRemove.Add(attacker);
                continue;
            }

            if (ShouldUpdateCombatCheck(attacker))
            {
                ProcessCombatAction(attacker, target);
            }
        }

        foreach (var ship in shipsToRemove)
        {
            DisengageCombat(ship);
        }
    }

    private bool ValidateContinuedCombat(Ship attacker, Ship target)
    {
        if (attacker == null || target == null || attacker.IsSinking || target.IsSinking)
        {
            LogSystem($"Combat invalidated: Ship is null or sinking");
            return false;
        }

        float distance = Vector3.Distance(attacker.transform.position, target.transform.position);
        
        // Check if target is beyond max combat range
        if (distance > maxCombatRange)
        {
            LogSystem($"{attacker.ShipName} disengaging - Target beyond max combat range ({distance:F1} units)");
            return false;
        }

        // Check if target is beyond disengage distance
        if (distance > minDisengageDistance)
        {
            LogSystem($"{attacker.ShipName} disengaging - Target beyond disengage distance ({distance:F1} units)");
            return false;
        }

        return true;
    }

    public void ClearCombatTarget(Ship ship)
    {
        if (ship != null)
        {
            if (combatTargets.TryGetValue(ship, out Ship target))
            {
                LogSystem($"Combat target cleared: {ship.ShipName} no longer targeting {target?.ShipName}");
            }
            
            combatTargets.Remove(ship);
            lastRangeCheckTime.Remove(ship);

            // Reset ship movement if applicable
            var movement = ship.GetComponent<ShipMovement>();
            if (movement != null)
            {
                movement.ClearTargetPosition();
            }
        }
    }

    private void ProcessCombatAction(Ship attacker, Ship target)
    {
        float distance = Vector3.Distance(attacker.transform.position, target.transform.position);
        
        if (distance <= attacker.AttackRange)
        {
            Vector3 directionToTarget = (target.transform.position - attacker.transform.position).normalized;
            float angleToTarget = Vector3.Angle(attacker.transform.forward, directionToTarget);

            if (angleToTarget <= attacker.FiringArc * 0.5f)
            {
                AttemptToFire(attacker, target, angleToTarget);
            }
            else
            {
                LogCombat($"{attacker.ShipName} cannot fire: Target outside firing arc ({angleToTarget:F1}°)");
            }
        }
        else
        {
            LogCombat($"{attacker.ShipName} pursuing target: Distance {distance:F1} units");
            UpdatePursuit(attacker, target);
        }

        lastRangeCheckTime[attacker] = Time.time;
    }

    private void AttemptToFire(Ship attacker, Ship target, float angleToTarget)
    {
        if (attacker.CanFire)
        {
            LogCombat($"{attacker.ShipName} firing at {target.ShipName} (Angle: {angleToTarget:F1}°)");
            attacker.Fire(target);
        }
        else
        {
            LogCombat($"{attacker.ShipName} cannot fire: {(attacker.CurrentAmmo <= 0 ? "No ammo" : "Reloading")}");
        }
    }

    private void UpdatePursuit(Ship attacker, Ship target)
    {
        var movement = attacker.GetComponent<ShipMovement>();
        if (movement != null)
        {
            movement.SetTargetPosition(target.transform.position);
        }
    }

    private void DisengageCombat(Ship ship)
    {
        ClearCombatTarget(ship);
    }

    private bool ShouldUpdateCombatCheck(Ship ship)
    {
        return Time.time - lastRangeCheckTime.GetValueOrDefault(ship, 0f) >= combatUpdateInterval;
    }

    public bool IsInCombat(Ship ship)
    {
        return combatTargets.ContainsKey(ship) || combatTargets.ContainsValue(ship);
    }

    #region Debug Methods
    private void LogSystem(string message)
    {
        if (logCombatDetails)
        {
            Debug.Log($"[CombatSystem] {message}");
        }
    }

    private void LogCombat(string message)
    {
        if (logCombatDetails)
        {
            Debug.Log($"[Combat] {message}");
        }
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning($"[CombatSystem] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[CombatSystem] {message}");
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !showDebugGizmos) return;

        foreach (var kvp in combatTargets)
        {
            if (kvp.Key != null && kvp.Value != null)
            {
                DrawCombatGizmos(kvp.Key, kvp.Value);
            }
        }
    }

    private void DrawCombatGizmos(Ship attacker, Ship target)
    {
        // Combat connection line
        Gizmos.color = Color.red;
        Gizmos.DrawLine(attacker.transform.position, target.transform.position);

        // Attack range
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawWireSphere(attacker.transform.position, attacker.AttackRange);

        // Maximum combat range
        Gizmos.color = new Color(1, 0.5f, 0, 0.1f);
        Gizmos.DrawWireSphere(attacker.transform.position, maxCombatRange);

        // Firing arc
        DrawFiringArc(attacker);
    }

    private void DrawFiringArc(Ship ship)
    {
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
    #endregion
}
