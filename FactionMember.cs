using UnityEngine;
using CorsairGame;

[RequireComponent(typeof(Ship))]
public class FactionMember : MonoBehaviour
{
    [SerializeField] private FactionType factionType = FactionType.None;
    public FactionType FactionType => factionType;

    private Ship ship;
    private Faction faction;

    private void Awake()
    {
        ship = GetComponent<Ship>();
    }

    private void Start()
    {
        if (FactionManager.Instance != null)
        {
            // Get faction from the type
            faction = FactionManager.Instance.GetFactionByType(factionType);
            if (faction != null)
            {
                FactionManager.Instance.RegisterShip(ship, faction);
            }
            else
            {
                Debug.LogError($"[FactionMember] Could not find faction for type {factionType}");
            }
        }
    }

    public void SetFaction(FactionType newFactionType)
    {
        if (newFactionType == factionType) return;

        var oldFactionType = factionType;
        factionType = newFactionType;

        if (FactionManager.Instance != null)
        {
            var oldFaction = FactionManager.Instance.GetFactionByType(oldFactionType);
            var newFaction = FactionManager.Instance.GetFactionByType(newFactionType);

            if (oldFaction != null)
            {
                FactionManager.Instance.UnregisterShip(ship, oldFaction);
            }

            if (newFaction != null)
            {
                FactionManager.Instance.RegisterShip(ship, newFaction);
                faction = newFaction;
            }
        }
    }

    public bool IsFriendly(FactionMember other)
    {
        if (other == null || faction == null)
            return false;

        if (FactionManager.Instance == null) return false;

        return FactionManager.Instance.AreFactionsAllied(faction, other.faction);
    }

    public bool IsHostile(FactionMember other)
    {
        if (other == null || faction == null)
            return false;

        if (FactionManager.Instance == null) return false;

        return FactionManager.Instance.AreFactionsAtWar(faction, other.faction);
    }

    private void OnDestroy()
    {
        if (FactionManager.Instance != null && faction != null)
        {
            FactionManager.Instance.UnregisterShip(ship, faction);
        }
    }
}