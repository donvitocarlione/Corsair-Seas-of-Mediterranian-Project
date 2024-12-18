using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class FactionManager : MonoBehaviour
{
    private static FactionManager instance;
    public static FactionManager Instance => instance;
    
    [SerializeField] private Faction playerFaction;
    public Faction PlayerFaction => playerFaction;

    // Core data storage
    private Dictionary<Faction, Dictionary<Faction, float>> relationships;
    private Dictionary<Faction, float> influences;
    private Dictionary<Faction, HashSet<Ship>> factionShips;
    
    // Events
    public event Action<Faction, Faction, float> OnRelationshipChanged;
    public event Action<Ship, Faction> OnShipFactionChanged;
    public event Action<Faction, float> OnInfluenceUpdated;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFactionSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeFactionSystem()
    {
        relationships = new Dictionary<Faction, Dictionary<Faction, float>>();
        influences = new Dictionary<Faction, float>();
        factionShips = new Dictionary<Faction, HashSet<Ship>>();
        Debug.Log("[FactionManager] Initialized faction system");
    }

    public void RegisterFaction(Faction faction)
    {
        if (faction == null) return;

        if (!relationships.ContainsKey(faction))
        {
            relationships[faction] = new Dictionary<Faction, float>();
            influences[faction] = FactionConstants.DEFAULT_INFLUENCE;
            factionShips[faction] = new HashSet<Ship>();
            Debug.Log($"[FactionManager] Registered faction: {faction.FactionName}");
        }
    }

    // Relationship Management
    public float GetRelationship(Faction a, Faction b)
    {
        if (a == null || b == null || a == b) return FactionConstants.MAX_RELATIONSHIP;
        
        if (!relationships.ContainsKey(a) || !relationships.ContainsKey(b))
            return FactionConstants.DEFAULT_STARTING_RELATIONSHIP;

        if (relationships[a].TryGetValue(b, out float value))
            return value;

        return FactionConstants.DEFAULT_STARTING_RELATIONSHIP;
    }
    
    public void SetRelationship(Faction a, Faction b, float value)
    {
        if (a == null || b == null || a == b) return;
        
        value = Mathf.Clamp(value, FactionConstants.MIN_RELATIONSHIP, FactionConstants.MAX_RELATIONSHIP);
        
        if (!relationships.ContainsKey(a)) relationships[a] = new Dictionary<Faction, float>();
        if (!relationships.ContainsKey(b)) relationships[b] = new Dictionary<Faction, float>();
        
        relationships[a][b] = value;
        relationships[b][a] = value;
        
        OnRelationshipChanged?.Invoke(a, b, value);
        Debug.Log($"[FactionManager] Updated relationship between {a.FactionName} and {b.FactionName} to {value}");
    }
    
    // Ship Management
    public void OnShipRegistered(Ship ship, Faction faction)
    {
        if (ship == null || faction == null) return;

        if (!factionShips.ContainsKey(faction))
            factionShips[faction] = new HashSet<Ship>();

        factionShips[faction].Add(ship);
        OnShipFactionChanged?.Invoke(ship, faction);
        Debug.Log($"[FactionManager] Registered ship {ship.ShipName} to faction {faction.FactionName}");
    }

    public void OnShipDestroyed(Ship ship, Faction faction)
    {
        if (ship == null || faction == null) return;

        if (factionShips.ContainsKey(faction))
        {
            factionShips[faction].Remove(ship);
            Debug.Log($"[FactionManager] Removed destroyed ship {ship.ShipName} from faction {faction.FactionName}");
        }
    }
    
    // Influence Management
    public float GetInfluence(Faction faction)
    {
        if (faction == null) return 0f;
        return influences.TryGetValue(faction, out float value) ? value : FactionConstants.DEFAULT_INFLUENCE;
    }

    public void UpdateInfluence(Faction faction, float newValue)
    {
        if (faction == null) return;
        
        newValue = Mathf.Clamp(newValue, 0f, FactionConstants.MAX_RELATIONSHIP);
        influences[faction] = newValue;
        OnInfluenceUpdated?.Invoke(faction, newValue);
        Debug.Log($"[FactionManager] Updated influence for {faction.FactionName} to {newValue}");
    }
    
    // Utility Methods
    public IEnumerable<Faction> GetAllFactions()
    {
        return relationships.Keys;
    }

    public IReadOnlyCollection<Ship> GetFactionShips(Faction faction)
    {
        if (faction != null && factionShips.TryGetValue(faction, out var ships))
            return ships;
        return new HashSet<Ship>();
    }
}
