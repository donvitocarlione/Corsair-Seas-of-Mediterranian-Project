using UnityEngine;
using System.Collections.Generic;
using CSM.Base;

public class ShipSpawner : MonoBehaviour
{
    [SerializeField] private GameObject piratePrefab;
    [SerializeField] private float minSpawnDistance = 50f;
    [SerializeField] private int maxSpawnAttempts = 10;
    [SerializeField] private float defaultSpawnHeightAboveWater = 5f;
    [SerializeField] private bool debugSpawnPositions = true;

    private Transform shipsParent;
    private Transform piratesParent;
    private WaterBody waterBody;
    private ShipRegistry shipRegistry;

    public void Initialize(Transform shipsParent, Transform piratesParent, WaterBody waterBody, ShipRegistry registry)
    {
        this.shipsParent = shipsParent;
        this.piratesParent = piratesParent;
        this.waterBody = waterBody;
        this.shipRegistry = registry;

        if (waterBody == null)
        {
            Debug.LogError("[ShipSpawner] WaterBody reference is required!");
            enabled = false;
        }

        ValidateConfiguration();
    }

    private bool ValidateConfiguration()
    {
        if (piratePrefab == null)
        {
            Debug.LogError("[ShipSpawner] Missing pirate prefab!");
            return false;
        }

        if (shipsParent == null || piratesParent == null)
        {
            Debug.LogError("[ShipSpawner] Parent transforms not set!");
            return false;
        }

        return true;
    }

    public Ship SpawnShipForFaction(FactionType faction, FactionShipData data)
    {
        Debug.Log($"[ShipSpawner] Attempting to spawn ship for faction {faction}");
        if (data == null)
        {
            Debug.LogError($"[ShipSpawner] No data found for faction {faction}");
            return null;
        }

        var prefab = data.ShipPrefabs[Random.Range(0, data.ShipPrefabs.Count)];
        var spawnPos = GetSafeSpawnPosition(data.SpawnArea, data.SpawnRadius, data.SpawnHeightAboveWater);
        
        Debug.Log($"[ShipSpawner] Spawning ship at position {spawnPos}");
        var shipObj = Instantiate(prefab, spawnPos, Quaternion.Euler(0, Random.Range(0, 360), 0), shipsParent);
        var ship = shipObj.GetComponent<Ship>();
        
        if (ship != null)
        {
            string shipName = $"{faction}_Ship_{Random.Range(1000, 9999)}";
            ship.Initialize(faction, shipName);
            
            Debug.Log($"[ShipSpawner] Ship {shipName} initialized for faction {faction} (IsPlayerFaction: {data.IsPlayerFaction})");
            
            if (!data.IsPlayerFaction && shipObj.GetComponent<AIShipController>() == null)
            {
                shipObj.AddComponent<AIShipController>().Initialize(ship);
                Debug.Log($"[ShipSpawner] Added AI controller to {shipName}");
            }

            shipRegistry.RegisterShipPosition(spawnPos);
            return ship;
        }
        
        Debug.LogError($"[ShipSpawner] Failed to get Ship component from prefab for faction {faction}");
        Destroy(shipObj);
        return null;
    }


    public Pirate SpawnPirateShip(FactionType faction)
    {
        var pirateObj = Instantiate(piratePrefab, Vector3.zero, Quaternion.identity, piratesParent);
        var pirate = pirateObj.GetComponent<Pirate>();
        
        if (pirate == null)
        {
            Debug.LogError("[ShipSpawner] Pirate prefab missing Pirate component!");
            Destroy(pirateObj);
            return null;
        }

        pirateObj.name = $"Pirate_{faction}_{Random.Range(1000, 9999)}";
        pirate.SetFaction(faction);
        return pirate;
    }

    private Vector3 GetSafeSpawnPosition(Vector3 center, float radius, float spawnHeightAboveWater)
    {
        if (waterBody == null)
        {
            Debug.LogError("[ShipSpawner] WaterBody is null when getting spawn position");
            return center + Random.insideUnitSphere * radius * 0.5f;
        }

        float waterLevel = waterBody.GetYBound();
        if (debugSpawnPositions)
        {
            Debug.Log($"[ShipSpawner] Water level at: {waterLevel}");
        }

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 randomPos = new Vector3(
                center.x + randomCircle.x,
                0f,
                center.z + randomCircle.y
            );

            float heightAboveWater = Mathf.Max(spawnHeightAboveWater, defaultSpawnHeightAboveWater);
            randomPos.y = waterLevel + heightAboveWater;

            if (debugSpawnPositions)
            {
                Debug.Log($"[ShipSpawner] Trying spawn position: {randomPos}, Height above water: {heightAboveWater}");
            }

            if (shipRegistry.IsSafePosition(randomPos, minSpawnDistance))
            {
                if (debugSpawnPositions)
                {
                    Debug.Log($"[ShipSpawner] Found safe spawn position at {randomPos}");
                }
                return randomPos;
            }
        }

        Vector3 fallbackPos = center + new Vector3(Random.Range(-radius * 0.5f, radius * 0.5f), 0f, Random.Range(-radius * 0.5f, radius * 0.5f));
        fallbackPos.y = waterLevel + defaultSpawnHeightAboveWater;
        
        if (debugSpawnPositions)
        {
            Debug.LogWarning($"[ShipSpawner] Using fallback spawn position: {fallbackPos}");
        }
        return fallbackPos;
    }

    private void OnValidate()
    {
        if (minSpawnDistance < 0) minSpawnDistance = 50f;
        if (maxSpawnAttempts < 1) maxSpawnAttempts = 10;
        if (defaultSpawnHeightAboveWater < 1f) defaultSpawnHeightAboveWater = 5f;
    }
}