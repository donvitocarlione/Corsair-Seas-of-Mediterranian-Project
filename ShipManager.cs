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
    public float spawnHeightAboveWater = 5f; // New field for spawn height

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
    
    [Header("Spawn Settings")]
    public float defaultSpawnHeightAboveWater = 5f; // Default height above water
    public bool debugSpawnPositions = true; // Toggle for spawn position debugging

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

    // ... [Previous methods remain unchanged until GetSafeSpawnPosition] ...

    private Vector3 GetSafeSpawnPosition(Vector3 center, float radius)
    {
        if (waterBody == null)
        {
            Debug.LogError("[ShipManager] WaterBody is null when getting spawn position");
            return center + Random.insideUnitSphere * radius * 0.5f;
        }

        float waterLevel = waterBody.GetYBound();
        if (debugSpawnPositions)
        {
            Debug.Log($"[ShipManager] Water level at: {waterLevel}");
        }

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            // Get random position within radius but constrained to XZ plane
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 randomPos = new Vector3(
                center.x + randomCircle.x,
                0f, // Will be set properly below
                center.z + randomCircle.y
            );

            // Set height above water
            float spawnHeight = GetSpawnHeightForPosition(center, waterLevel);
            randomPos.y = spawnHeight;

            if (debugSpawnPositions)
            {
                Debug.Log($"[ShipManager] Trying spawn position: {randomPos}, Height above water: {spawnHeight - waterLevel}");
            }

            if (IsSafePosition(randomPos))
            {
                if (debugSpawnPositions)
                {
                    Debug.Log($"[ShipManager] Found safe spawn position at {randomPos}");
                }
                return randomPos;
            }
        }

        // Fallback position
        Vector3 fallbackPos = center + new Vector3(Random.Range(-radius * 0.5f, radius * 0.5f), 0f, Random.Range(-radius * 0.5f, radius * 0.5f));
        fallbackPos.y = GetSpawnHeightForPosition(center, waterLevel);
        
        if (debugSpawnPositions)
        {
            Debug.LogWarning($"[ShipManager] Using fallback spawn position: {fallbackPos}");
        }
        return fallbackPos;
    }

    private float GetSpawnHeightForPosition(Vector3 center, float waterLevel)
    {
        // Get the faction data for the spawn area
        var data = factionShipData.Find(d => Vector3.Distance(d.spawnArea, center) < 0.1f);
        float heightAboveWater = data != null ? data.spawnHeightAboveWater : defaultSpawnHeightAboveWater;
        
        // Ensure we're at least 1 unit above water
        return Mathf.Max(waterLevel + heightAboveWater, waterLevel + 1f);
    }

    private bool IsSafePosition(Vector3 position)
    {
        // Check distance from other ships
        foreach (Vector3 occupied in occupiedPositions)
        {
            if (Vector3.Distance(new Vector3(position.x, 0f, position.z), 
                                new Vector3(occupied.x, 0f, occupied.z)) < minSpawnDistance)
            {
                if (debugSpawnPositions)
                {
                    Debug.Log($"[ShipManager] Position {position} too close to occupied position {occupied}");
                }
                return false;
            }
        }

        // Add any additional safety checks here (e.g., terrain collision)
        return true;
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
        if (defaultSpawnHeightAboveWater < 1f) defaultSpawnHeightAboveWater = 5f;
        
        // Validate spawn heights in faction data
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