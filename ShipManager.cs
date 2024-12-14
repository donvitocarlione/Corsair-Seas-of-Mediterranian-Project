using UnityEngine;
using System.Collections.Generic;
using CSM.Base;

[RequireComponent(typeof(ShipManagerConfig))]
[RequireComponent(typeof(ShipSpawner))]
[RequireComponent(typeof(ShipRegistry))]
[RequireComponent(typeof(FactionInitializer))]
public class ShipManager : MonoBehaviour
{
    private static ShipManager instance;
    public static ShipManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ShipManager>();
                if (instance == null)
                {
                    Debug.LogError("[ShipManager] Instance not found!");
                }
            }
            return instance;
        }
    }

    private ShipManagerConfig config;
    private ShipSpawner spawner;
    private ShipRegistry registry;
    private FactionInitializer factionInitializer;

    private Transform shipsParent;
    private Transform piratesParent;
    private FactionType playerFaction;

    public FactionType PlayerFaction => playerFaction;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManager();
            Debug.Log("[ShipManager] Instance initialized");
        }
        else if (instance != this)
        {
            Debug.Log("[ShipManager] Duplicate instance destroyed");
            Destroy(gameObject);
        }
    }

    private void InitializeManager()
    {
        Debug.Log("[ShipManager] Initializing manager");
        CreateContainers();

        // Get required components
        config = GetComponent<ShipManagerConfig>();
        spawner = GetComponent<ShipSpawner>();
        registry = GetComponent<ShipRegistry>();
        factionInitializer = GetComponent<FactionInitializer>();

        if (!ValidateComponents())
        {
            Debug.LogError("[ShipManager] Initialization failed - missing components");
            enabled = false;
            return;
        }

        if (!config.ValidateConfiguration())
        {
            Debug.LogError("[ShipManager] Initialization failed - invalid configuration");
            enabled = false;
            return;
        }

        if (Application.isPlaying)
        {
            InitializeWaterBody();
        }
    }

    private void Start()
    {
        if (instance == this && enabled && Application.isPlaying)
        {
            InitializeGame();
        }
    }

    private void CreateContainers()
    {
        shipsParent = new GameObject("Ships").transform;
        shipsParent.parent = transform;
        
        piratesParent = new GameObject("Pirates").transform;
        piratesParent.parent = transform;
        Debug.Log("[ShipManager] Created containers");
    }

    private bool ValidateComponents()
    {
        if (config == null)
        {
            Debug.LogError("[ShipManager] ShipManagerConfig component missing!");
            return false;
        }

        if (spawner == null)
        {
            Debug.LogError("[ShipManager] ShipSpawner component missing!");
            return false;
        }

        if (registry == null)
        {
            Debug.LogError("[ShipManager] ShipRegistry component missing!");
            return false;
        }

        if (factionInitializer == null)
        {
            Debug.LogError("[ShipManager] FactionInitializer component missing!");
            return false;
        }

        return true;
    }

    private void InitializeWaterBody()
    {
        var waterBody = FindFirstObjectByType<WaterBody>();
        if (waterBody == null)
        {
            Debug.LogError("[ShipManager] No WaterBody found in scene!");
            enabled = false;
            return;
        }

        spawner.Initialize(shipsParent, piratesParent, waterBody, registry);
        Debug.Log("[ShipManager] WaterBody initialized");
    }

    private void InitializeGame()
    {
        var playerData = config.GetPlayerFactionData();
        if (playerData == null)
        {
            Debug.LogError("[ShipManager] No player faction configured!");
            return;
        }

        playerFaction = playerData.Faction;
        var playerInstance = FindFirstObjectByType<Player>();
        if (playerInstance == null)
        {
            Debug.LogError("[ShipManager] No Player component found!");
            return;
        }

        var factionManager = FindFirstObjectByType<FactionManager>();
        if (factionManager == null)
        {
            Debug.LogError("[ShipManager] No FactionManager found!");
            return;
        }

        factionInitializer.Initialize(spawner, factionManager, playerInstance, config.FactionData);
        factionInitializer.InitializePlayerFaction(playerFaction);
        factionInitializer.InitializeAllFactions();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) return;

        // Component validation will be handled by Unity due to RequireComponent attributes
        if (config != null)
        {
            config.ValidateConfiguration();
        }
    }

    // Public interface for ship destruction
    public void OnShipDestroyed(Ship ship)
    {
        if (ship != null && registry != null)
        {
            registry.OnShipDestroyed(ship);
        }
    }
}