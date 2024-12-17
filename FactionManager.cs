using UnityEngine;
using System.Collections.Generic;

public class FactionManager : MonoBehaviour
{
    private static FactionManager instance;
    public static FactionManager Instance => instance;

    private Dictionary<FactionType, FactionDefinition> factions = new Dictionary<FactionType, FactionDefinition>();
    
    public const float MIN_RELATION = -100f;
    public const float MAX_RELATION = 100f;

    // Events
    public delegate void OnShipUnregisteredDelegate(Ship ship);
    public delegate void OnRelationChangedDelegate(FactionType factionA, FactionType factionB, float newValue);
    public delegate void OnInfluenceChangedDelegate(FactionType faction, int newValue);
    public delegate void OnPortCapturedDelegate(Port port, FactionType oldOwner, FactionType newOwner);
    public delegate void OnShipRegisteredDelegate(Ship ship);

    public event OnShipUnregisteredDelegate OnShipUnregistered;
    public event OnRelationChangedDelegate OnRelationChanged;
    public event OnInfluenceChangedDelegate OnInfluenceChanged;
    public event OnPortCapturedDelegate OnPortCaptured;
    public event OnShipRegisteredDelegate OnShipRegistered;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFactions();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeFactions()
    {
        factions.Clear();
        foreach (FactionType faction in System.Enum.GetValues(typeof(FactionType)))
        {
            InitializeDefaultFaction(faction);
        }
    }

    private void InitializeDefaultFaction(FactionType faction)
    {
        if (faction == FactionType.None) return;
        
        string factionName = faction.ToString();
        factions[faction] = new FactionDefinition(faction, factionName);
        OnInfluenceChanged?.Invoke(faction, 100); // Set default influence to 100
        Debug.Log($"[FactionManager] Initialized default faction: {faction} with name {factionName}");
    }

    public FactionDefinition GetFactionData(FactionType factionType)
    {
        if (factions.TryGetValue(factionType, out FactionDefinition faction))
        {
            return faction;
        }
        Debug.LogWarning($"[FactionManager] Faction not found: {factionType}");
        return null;
    }

    public void UpdateFactionRelation(FactionType factionA, FactionType factionB, float value)
    {
        if (factionA == FactionType.None || factionB == FactionType.None)
        {
            Debug.LogWarning($"[FactionManager] Cannot update relation to none faction");
            return;
        }
        if (!factions.ContainsKey(factionA) || !factions.ContainsKey(factionB))
        {
            Debug.LogWarning($"[FactionManager] Cannot update relation to non existing faction: {factionA} or {factionB}");
            return;
        }

        factions[factionA].SetRelation(factionB, Mathf.Clamp(value, MIN_RELATION, MAX_RELATION));
        factions[factionB].SetRelation(factionA, Mathf.Clamp(value, MIN_RELATION, MAX_RELATION));

        OnRelationChanged?.Invoke(factionA, factionB, value);
        Debug.Log($"[FactionManager] Relation updated between {factionA} and {factionB} to {value}");
    }

    // Add methods to invoke the unused events
    public void RegisterShip(Ship ship)
    {
        OnShipRegistered?.Invoke(ship);
        Debug.Log($"[FactionManager] Ship registered: {ship.ShipName}");
    }

    public void UnregisterShip(Ship ship)
    {
        OnShipUnregistered?.Invoke(ship);
        Debug.Log($"[FactionManager] Ship unregistered: {ship.ShipName}");
    }

    public void NotifyPortCaptured(Port port, FactionType oldOwner, FactionType newOwner)
    {
        OnPortCaptured?.Invoke(port, oldOwner, newOwner);
        Debug.Log($"[FactionManager] Port captured from {oldOwner} by {newOwner}");
    }
}