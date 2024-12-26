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
    public static ShipManager Instance { get; private set; }

    [Header("References")]
    public GameObject piratePrefab;

    [Header("Faction Settings")]
    public List<FactionShipData> factionShipData;
    public float minSpawnDistance = 50f;
    public int maxSpawnAttempts = 10;

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
        Debug.Log("[ShipManager] Awake start");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManager();
            Debug.Log("[ShipManager] Instance initialized");
        }
        else
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

        // Log faction configurations
        foreach (var data in factionShipData)
        {
            Debug.Log($"[ShipManager] Faction config - Type: {data.faction}, IsPlayerFaction: {data.isPlayerFaction}, Ships: {data.initialShipCount}, Pirates: {data.initialPirateCount}");
        }

        return factionShipData.TrueForAll(data => data.Validate());
    }

    void Start()
    {
        Debug.Log("[ShipManager] Start");
        if (Instance == this && isInitialized)
        {
            InitializeWaterBody();
            InitializePlayerFaction();
        }
    }

   private void InitializeWaterBody()
   {
       waterBody = FindAnyObjectByType<WaterBody>();
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
        
        playerInstance = FindAnyObjectByType<Player>();
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

    private void InitializePiratesForFaction(FactionShipData data)
    {
        Debug.Log($"[ShipManager] Initializing pirates for faction {data.faction}");
        for (int i = 0; i < data.initialPirateCount; i++)
        {
            if (SpawnPirateShip(data.faction) is Pirate pirate)
            {
                int shipsPerPirate = data.initialShipCount / data.initialPirateCount;
                Debug.Log($"[ShipManager] Spawned pirate for faction {data.faction}, assigning {shipsPerPirate} ships");
                
                for (int j = 0; j < shipsPerPirate; j++)
                {
                    if (SpawnShipForFaction(data.faction) is Ship ship)
                    {
                        pirate.AddShip(ship);
                        Debug.Log($"[ShipManager] Added ship {ship.ShipName} to pirate's fleet");
                    }
                }
            }
        }
    }

    public Ship SpawnShipForFaction(FactionType faction)
    {
        Debug.Log($"[ShipManager] Attempting to spawn ship for faction {faction}");
        var data = GetFactionShipData(faction);
        if (data == null)
        {
            Debug.LogError($"[ShipManager] No data found for faction {faction}");
            return null;
        }

        var prefab = data.shipPrefabs[Random.Range(0, data.shipPrefabs.Count)];
        var spawnPos = GetSafeSpawnPosition(data.spawnArea, data.spawnRadius);
        
        Debug.Log($"[ShipManager] Spawning ship at position {spawnPos}");
        var shipObj = Instantiate(prefab, spawnPos, Quaternion.Euler(0, Random.Range(0, 360), 0), shipsParent);
        var ship = shipObj.GetComponent<Ship>();
        
        if (ship != null)
        {
            string shipName = $"{faction}_Ship_{Random.Range(1000, 9999)}";
            ship.Initialize(faction, shipName);
            
            Debug.Log($"[ShipManager] Ship {shipName} initialized for faction {faction} (IsPlayerFaction: {data.isPlayerFaction})");
            
            if (!data.isPlayerFaction && shipObj.GetComponent<AIShipController>() == null)
            {
                shipObj.AddComponent<AIShipController>().Initialize(ship);
                Debug.Log($"[ShipManager] Added AI controller to {shipName}");
            }

            occupiedPositions.Add(spawnPos);
            return ship;
        }
        
        Debug.LogError($"[ShipManager] Failed to get Ship component from prefab for faction {faction}");
        Destroy(shipObj);
        return null;
    }

    private Pirate SpawnPirateShip(FactionType faction)
    {
        var pirateObj = Instantiate(piratePrefab, Vector3.zero, Quaternion.identity, piratesParent);
        var pirate = pirateObj.GetComponent<Pirate>();
        
        if (pirate == null)
        {
            Debug.LogError("[ShipManager] Pirate prefab missing Pirate component!");
            Destroy(pirateObj);
            return null;
        }

        pirateObj.name = $"Pirate_{faction}_{Random.Range(1000, 9999)}";
        pirate.SetFaction(faction);
        return pirate;
    }

    private Vector3 GetSafeSpawnPosition(Vector3 center, float radius)
    {
       if(waterBody == null) return center + Random.insideUnitSphere * radius * 0.5f;
        float waterLevel = waterBody.GetYBound();

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 randomPos = center + Random.insideUnitSphere * radius;
            randomPos.y = waterLevel;
            if (IsSafePosition(randomPos))
            {
                return randomPos;
            }
        }

        return center + Random.insideUnitSphere * radius * 0.5f;
    }

    private bool IsSafePosition(Vector3 position)
    {
        foreach (Vector3 occupied in occupiedPositions)
        {
            if (Vector3.Distance(position, occupied) < minSpawnDistance)
            {
                return false;
            }
        }
        return true;
    }

    private FactionShipData GetFactionShipData(FactionType faction)
    {
        return factionShipData.Find(data => data.faction == faction);
    }

    public void OnShipDestroyed(Ship ship)
    {
        if (ship != null)
        {
            Debug.Log($"[ShipManager] Ship {ship.ShipName} destroyed, removing from occupied positions");
            occupiedPositions.Remove(ship.transform.position);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void OnValidate()
    {
        if (minSpawnDistance < 0) minSpawnDistance = 50f;
        if (maxSpawnAttempts < 1) maxSpawnAttempts = 10;
    }
}