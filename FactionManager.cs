using UnityEngine;
using System.Collections.Generic;
using System;

[AddComponentMenu("Game/Faction Manager")]
public class FactionManager : MonoBehaviour
{
    [SerializeField] private List<FactionShipData> factionConfigurations = new List<FactionShipData>();

    private static FactionManager instance;
    public static FactionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<FactionManager>();
                if (instance == null)
                {
                    Debug.LogError("No FactionManager found in scene!");
                }
            }
            return instance;
        }
    }

    private Dictionary<FactionType, FactionDefinition> factions = new();
    
    // Events
    public event Action<FactionType, Ship> OnShipRegistered;
    public event Action<FactionType, Ship> OnShipUnregistered;
    public event Action<FactionType, FactionType, float> OnRelationChanged;
    public event Action<FactionType, int> OnInfluenceChanged;
    public event Action<FactionType, Port> OnPortCaptured;

    // Constants
    private const float MIN_RELATION = 0f;
    private const float MAX_RELATION = 100f;
    private const float NEUTRAL_RELATION = 50f;
    private const float WAR_THRESHOLD = 25f;
    private const float ALLY_THRESHOLD = 75f;
    private const float TRADE_RELATION_MULTIPLIER = 0.1f;

    public List<FactionShipData> GetFactionConfigurations() => factionConfigurations;

    private void Awake()
    {
        ValidateSingleton();
        InitializeFactions();
    }

    private void Start()
    {
        // Initialize ship spawning
        var shipSpawner = FindAnyObjectByType<ShipSpawner>();
        var player = FindAnyObjectByType<Player>();

        if (shipSpawner != null)
        {
            var factionInitializer = gameObject.GetComponent<FactionInitializer>();
            if (factionInitializer == null)
            {
                factionInitializer = gameObject.AddComponent<FactionInitializer>();
            }
            factionInitializer.Initialize(shipSpawner, this, player, factionConfigurations);
            factionInitializer.InitializeAllFactions();
        }
        else
        {
            Debug.LogError("[FactionManager] ShipSpawner not found in scene!");
        }
    }

    private void ValidateSingleton()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"Multiple FactionManager instances found! Destroying duplicate on {gameObject.name}");
            Destroy(this);
            return;
        }
        instance = this;
    }

    private void InitializeFactions()
    {
        foreach (FactionType faction in Enum.GetValues(typeof(FactionType)))
        {
            if (!factions.ContainsKey(faction))
            {
                InitializeDefaultFaction(faction);
            }
        }

        // Validate faction configurations
        foreach (var config in factionConfigurations)
        {
            if (!config.Validate())
            {
                Debug.LogError($"[FactionManager] Invalid configuration for faction {config.Faction}");
            }
        }
    }

    // ... [rest of the existing FactionManager code remains the same]
}
