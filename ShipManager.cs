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
    [Header("References")]
    public GameObject piratePrefab;

    [Header("Faction Settings")]
    public List<FactionShipData> factionShipData;
    public float minSpawnDistance = 50f;
    public int maxSpawnAttempts = 10;
    
    [Header("Spawn Settings")]
    public float defaultSpawnHeightAboveWater = 5f;
    public bool debugSpawnPositions = true;

    private static ShipManager instance;
    public static ShipManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ShipManager>();
                if (instance == null)
                {
                    Debug.LogError("ShipManager instance not found!");
                }
            }
            return instance;
        }
    }

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

    // ... [Previous methods remain unchanged] ...

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
