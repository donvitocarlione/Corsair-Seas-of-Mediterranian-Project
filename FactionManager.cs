using UnityEngine;
using System.Collections.Generic;
using System;
using static ShipExtensions;

[DefaultExecutionOrder(-1)]
[AddComponentMenu("Game/Faction Manager")]
public class FactionManager : MonoBehaviour
{
    public static FactionManager Instance { get; protected set; }

    [SerializeField] protected FactionConfiguration _configuration;
    public FactionConfiguration configuration => _configuration;
    
    [SerializeField] protected List<FactionDefinitionAsset> factionDefinitions;
    
    public FactionEventSystem EventSystem { get; protected set; }
    
    protected Dictionary<FactionType, FactionDefinition> factions = new();
    private Dictionary<FactionType, HashSet<Faction>> factionEntities = new();
    private Dictionary<FactionType, HashSet<Ship>> factionShips = new(); 
    
    // Add new fields for pirate management
    private Dictionary<FactionType, List<Pirate>> factionPirates = new();
    private Dictionary<FactionType, Pirate> factionLeaders = new();

    protected void Awake()
    {
        ValidateSingleton();
        EventSystem = new FactionEventSystem();
        InitializeFactions();
        
        // Add debug logging to verify initialization
        Debug.Log($"Initialized factions: {string.Join(", ", factions.Keys)}");
    }

    protected void ValidateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Multiple FactionManager instances found! Destroying duplicate on {gameObject.name}");
            Destroy(this);
            return;
        }
        Instance = this;
    }

    protected void InitializeFactions()
    {
        Debug.Log("Starting faction initialization...");
        
        // First validate configuration
        if (_configuration == null)
        {
            Debug.LogError("Faction configuration is missing!");
            return;
        }

        // Initialize from faction definition assets if available
        if (factionDefinitions != null && factionDefinitions.Count > 0)
        {
            foreach (var definition in factionDefinitions)
            {
                if (definition != null)
                {
                    InitializeFactionFromAsset(definition);
                    InitializeFactionContainers(definition.type);
                    Debug.Log($"Initialized faction from asset: {definition.displayName} ({definition.type})");
                }
                else
                {
                    Debug.LogWarning("Null faction definition found in factionDefinitions list");
                }
            }
        }
        else
        {
            Debug.LogWarning("No faction definitions found, initializing default factions...");
        }

        // Initialize any missing faction types with default values
        foreach (FactionType factionType in Enum.GetValues(typeof(FactionType)))
        {
            if (!factions.ContainsKey(factionType))
            {
                InitializeDefaultFaction(factionType);
                InitializeFactionContainers(factionType);
                Debug.Log($"Initialized default faction: {factionType}");
            }
        }

        Debug.Log($"Faction initialization complete. Total factions: {factions.Count}");
    }

    private void InitializeFactionContainers(FactionType factionType)
    {
        // Initialize containers for the faction if they don't exist
        if (!factionEntities.ContainsKey(factionType))
            factionEntities[factionType] = new HashSet<Faction>();
            
        if (!factionShips.ContainsKey(factionType))
            factionShips[factionType] = new HashSet<Ship>();
            
        if (!factionPirates.ContainsKey(factionType))
            factionPirates[factionType] = new List<Pirate>();
    }

    // Rest of your existing FactionManager code...

    // Your existing methods continue here...
}
