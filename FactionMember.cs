using UnityEngine;

[RequireComponent(typeof(Ship))]
public class FactionMember : MonoBehaviour
{
    [SerializeField] private Faction faction;
    public Faction Faction => faction;

    private Ship ship;

    private void Awake()
    {
        ship = GetComponent<Ship>();
    }

    private void Start()
    {
        if (faction != null && FactionManager.Instance != null)
        {
            FactionManager.Instance.OnShipRegistered(ship, faction);
        }
    }

    public void SetFaction(Faction newFaction)
    {
        if (newFaction == faction) return;

        var oldFaction = faction;
        faction = newFaction;

        if (FactionManager.Instance != null)
        {
            if (oldFaction != null)
            {
                FactionManager.Instance.OnShipDestroyed(ship, oldFaction);
            }

            if (newFaction != null)
            {
                FactionManager.Instance.OnShipRegistered(ship, newFaction);
            }
        }
    }

    public bool IsFriendly(FactionMember other)
    {
        if (other == null || faction == null || other.faction == null)
            return false;

        return faction.IsFriendlyWith(other.faction);
    }

    public bool IsHostile(FactionMember other)
    {
        if (other == null || faction == null || other.faction == null)
            return false;

        return faction.IsHostileWith(other.faction);
    }

    private void OnDestroy()
    {
        if (faction != null && FactionManager.Instance != null)
        {
            FactionManager.Instance.OnShipDestroyed(ship, faction);
        }
    }
}
