using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FactionShipData
{
    public FactionType faction;
    public List<GameObject> shipPrefabs;
    public Vector3 spawnArea;
    public float spawnRadius = 100f;
    public int initialShipCount = 3;
    public bool isPlayerFaction;
    public int initialPirateCount = 2;
    public float spawnHeightAboveWater = 5f;

    public bool Validate()
    {
        if (shipPrefabs == null || shipPrefabs.Count == 0)
        {
            Debug.LogError($"Missing ship prefabs for faction {faction}");
            return false;
        }

        foreach (var prefab in shipPrefabs)
        {
            if (prefab == null || prefab.GetComponent<Ship>() == null)
            {
                Debug.LogError($"Invalid ship prefab configuration for faction {faction}");
                return false;
            }
        }

        return spawnRadius > 0;
    }
}

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
                    Debug.LogError("ShipManager instance not found!");
                }
            }
            return instance;
        }
    }

    [Header("References")]
    public GameObject piratePrefab;

    [Header("Faction Settings")]
    public List<FactionShipData> factionShipData;
    public float minSpawnDistance = 50f;
    public int maxSpawnAttempts = 10;
    
    [Header("Spawn Settings")]
    public float defaultSpawnHeightAboveWater = 5f;
    public bool debugSpawnPositions = true;

    private Transform shipsParent;
    private Transform piratesParent;
    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
    private bool isInitialized;
    private FactionType playerFaction;
    private Player playerInstance;
    private WaterBody waterBody;

    public FactionType PlayerFaction => playerFaction;

    void Awake()
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
        isInitialized = ValidateConfiguration();
        
        if (!isInitialized)
        {
            Debug.LogError("[ShipManager] Initialization failed");
            enabled = false;
            return;
        }

        if (Application.isPlaying)
        {
            InitializeWaterBody();
        }
    }

    void Start()
    {
        if (instance == this && isInitialized && Application.isPlaying)
        {
            InitializePlayerFaction();
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

    private bool ValidateConfiguration()
    {
        Debug.Log("[ShipManager] Validating configuration");
        if (piratePrefab == null)
        {
            Debug.LogError("[ShipManager] Missing pirate prefab!");
            return false;
        }

        if (factionShipData == null || factionShipData.Count == 0)
        {
            Debug.LogError("[ShipManager] No faction data configured!");
            return false;
        }

        foreach (var data in factionShipData)
        {
            Debug.Log($"[ShipManager] Faction config - Type: {data.faction}, IsPlayerFaction: {data.isPlayerFaction}, Ships: {data.initialShipCount}, Pirates: {data.initialPirateCount}");
        }

        return factionShipData.TrueForAll(data => data.Validate());
    }

    private void InitializeWaterBody()
    {
        waterBody = FindFirstObjectByType<WaterBody>();
        if (waterBody == null)
        {
            Debug.LogError("[ShipManager] No WaterBody found in scene!");
            enabled = false;
        }
        Debug.Log("[ShipManager] WaterBody initialized");
    }

    private void InitializePlayerFaction()
    {
        Debug.Log("[ShipManager] Initializing player faction");
        var playerData = factionShipData.Find(data => data.isPlayerFaction);
        if (playerData == null)
        {
            Debug.LogError("[ShipManager] No player faction configured!");
            return;
        }

        playerFaction = playerData.faction;
        Debug.Log($"[ShipManager] Player faction set to: {playerFaction}");
        
        playerInstance = FindFirstObjectByType<Player>();
        if (playerInstance == null)
        {
            Debug.LogError("[ShipManager] No Player component found!");
            return;
        }

        playerInstance.SetFaction(playerFaction);
        Debug.Log($"[ShipManager] Player instance found and faction set to {playerFaction}");
        
        InitializeAllFactions();
    }

    private void InitializeAllFactions()
    {
        Debug.Log("[ShipManager] Initializing all factions");
        foreach (var data in factionShipData)
        {
            Debug.Log($"[ShipManager] Initializing faction: {data.faction} (IsPlayerFaction: {data.isPlayerFaction})");
            if (data.isPlayerFaction)
            {
                InitializePlayerShips(data);
            }
            else
            {
                InitializePiratesForFaction(data);
            }
        }
    }

    private void InitializePlayerShips(FactionShipData data)
    {
        Debug.Log($"[ShipManager] Initializing player ships for faction {data.faction}");
        if (playerInstance == null)
        {
            Debug.LogError("[ShipManager] Cannot initialize player ships - playerInstance is null");
            return;
        }

        for (int i = 0; i < data.initialShipCount; i++)
        {
            Debug.Log($"[ShipManager] Spawning player ship {i + 1}/{data.initialShipCount}");
            if (SpawnShipForFaction(data.faction) is Ship ship)
            {
                playerInstance.AddShip(ship);
                Debug.Log($"[ShipManager] Added ship {ship.ShipName} to player fleet");
            }
        }
    }

    public void OnShipDestroyed(Ship ship)
    {
        if (ship != null)
        {
            Debug.Log($"[ShipManager] Ship {ship.ShipName} destroyed, removing from occupied positions");
            occupiedPositions.Remove(ship.transform.position);
            Destroy(ship.gameObject, 2f); // Delayed destruction for effects
        }
    }

    // ... [Previous spawn and initialization methods remain unchanged] ...

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void OnValidate()
    {
        if (minSpawnDistance < 0) minSpawnDistance = 50f;
        if (maxSpawnAttempts < 1) maxSpawnAttempts = 10;
        if (defaultSpawnHeightAboveWater < 1f) defaultSpawnHeightAboveWater = 5f;

        if (factionShipData != null)
        {
            foreach (var data in factionShipData)
            {
                if (data.spawnHeightAboveWater < 1f)
                {
                    data.spawnHeightAboveWater = defaultSpawnHeightAboveWater;
                }
            }
        }
    }
}