using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CSM.Base;
using Random = UnityEngine.Random;

[AddComponentMenu("Game/Ship Manager")]
public class ShipManager : MonoBehaviour
{
    public static ShipManager Instance { get; private set; }

    [Header("Ship Spawning Settings")]
    public List<InitialShipData> initialShipData;
    public float minSpawnDistance = 50f;
    public int maxSpawnAttempts = 10;

    private Transform shipsParent;
    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
    private bool isInitialized;
    private WaterBody waterBody;

    #region Unity Methods

    private void Awake()
    {
       if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(InitializeManager());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    #region Initialization

    private IEnumerator InitializeManager()
    {
        yield return null;

        CreateContainers();
        InitializeWaterBody();
        isInitialized = ValidateConfiguration();

        if (!isInitialized)
        {
            Debug.LogError("[ShipManager] Invalid configuration, disabling manager.");
            enabled = false;
        }

        Debug.Log("[ShipManager] Initialized successfully");
    }

    private void CreateContainers()
    {
        shipsParent = new GameObject("Ships Container").transform;
        shipsParent.SetParent(transform);
    }

    private bool ValidateConfiguration()
    {
         if (initialShipData == null || initialShipData.Count == 0)
        {
            Debug.LogError("[ShipManager] No initial ship data provided.");
            return false;
        }

        foreach (var data in initialShipData)
        {
            if (!data.Validate())
            {
                return false;
            }
        }

        return true;
    }

    private void InitializeWaterBody()
    {
        waterBody = FindAnyObjectByType<WaterBody>();
        if (waterBody == null)
        {
            Debug.LogError("[ShipManager] WaterBody not found. Ships might not spawn correctly.");
        }
    }

    #endregion

    #region Ship Spawning

    public Ship SpawnShip(IEntityOwner owner, GameObject prefab, Vector3? customPosition = null)
    {
        if (!isInitialized)
        {
            Debug.LogError("[ShipManager] Cannot spawn ship - manager is not initialized");
            return null;
        }

        if (prefab == null)
        {
            Debug.LogError("[ShipManager] Ship prefab not assigned!");
            return null;
        }

        if (!prefab.GetComponent<Ship>())
        {
            Debug.LogError("[ShipManager] Prefab must have Ship component!");
            return null;
        }

        var shipData = GetInitialShipData();
        if (shipData == null)
        {
            Debug.LogError("[ShipManager] No ship data found!");
            return null;
        }

        Vector3 spawnPosition;
        if (customPosition.HasValue)
        {
            spawnPosition = customPosition.Value;
        }
        else
        {
            spawnPosition = GetSafeSpawnPosition(shipData.spawnArea, shipData.spawnRadius);
            if (spawnPosition == Vector3.zero)
            {
                Debug.LogWarning("[ShipManager] No available position found. Cannot spawn ship.");
                return null;
            }
        }

        occupiedPositions.Add(spawnPosition);
        GameObject shipInstance = Instantiate(prefab, spawnPosition, Quaternion.identity, shipsParent);
        Ship ship = shipInstance.GetComponent<Ship>();
        if (ship != null)
        {
            string shipName = $"Ship_{Random.Range(1000, 9999)}";
            try
            {
                ship.Initialize(shipName, owner);
                if(owner is Pirate pirate)
                {
                    pirate.AddShip(ship);
                }
               
                Debug.Log($"[ShipManager] Ship {shipName} initialized and registered for owner {owner.OwnerName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ShipManager] Failed to initialize ship: {e.Message}");
                Destroy(shipInstance);
                return null;
            }
        }
        else
        {
            Debug.LogError("[ShipManager] Could not find Ship component on prefab!");
            Destroy(shipInstance);
        }

        return ship;
    }

    private Vector3 GetSafeSpawnPosition(Vector3 center, float radius)
    {
       if (waterBody == null)
        {
            Debug.LogError("[ShipManager] No WaterBody found - cannot determine water level for ship placement!");
            return center;
        }

       float waterSurfaceHeight = waterBody.GetWaterSurfaceHeight();
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            float randomX = center.x + Random.Range(-radius, radius);
            float randomZ = center.z + Random.Range(-radius, radius);
            Vector3 spawnPosition = new Vector3(randomX, waterSurfaceHeight, randomZ);

             if (IsSafePosition(spawnPosition))
            {
               Debug.Log($"[ShipManager] Found safe spawn position at {spawnPosition}, water height: {waterSurfaceHeight}");
                return spawnPosition;
            }
        }
         Vector3 fallbackPosition = new Vector3(
            center.x + Random.Range(-radius * 0.5f, radius * 0.5f),
           waterSurfaceHeight,
            center.z + Random.Range(-radius * 0.5f, radius * 0.5f)
       );
       Debug.LogWarning($"[ShipManager] Could not find safe position after {maxSpawnAttempts} attempts. Using fallback position: {fallbackPosition}");
        return fallbackPosition;
    }

    private bool IsSafePosition(Vector3 position)
    {
        foreach (var occupiedPosition in occupiedPositions)
        {
            if (Vector3.Distance(position, occupiedPosition) < minSpawnDistance)
            {
               return false;
            }
        }
        return true;
    }

    public void UnregisterShip(Ship ship)
    {
        if (ship == null)
            throw new ArgumentNullException(nameof(ship));
    }

    public void OnShipDestroyed(Ship ship)
    {
        if (ship != null)
        {
            if(ship.Owner != null){
                if(ship.Owner is Pirate pirate){
                    pirate.RemoveShip(ship);
                }

               ship.ClearOwner();
            }
            UnregisterShip(ship);
            occupiedPositions.Remove(ship.transform.position);
            Debug.Log($"[ShipManager] Ship {ship.ShipName()} destroyed, removing from occupied positions");
        }
    }

    #endregion

    #region Helper Methods

    private InitialShipData GetInitialShipData()
    {
        return initialShipData.FirstOrDefault();
    }

    private void OnValidate()
    {
        if (minSpawnDistance < 0) minSpawnDistance = 50f;
        if (maxSpawnAttempts < 1) maxSpawnAttempts = 10;
    }

    #endregion

    #region Properties

    public bool IsInitialized => isInitialized;

    #endregion

    #region Editor Classes

    [System.Serializable]
    public class InitialShipData
    {
       public List<GameObject> shipPrefabs;
        public Vector3 spawnArea;
        public float spawnRadius = 100f;
        public int initialShipCount = 3;
        public int initialPirateCount = 2;

        public bool Validate()
        {
            if (shipPrefabs == null)
            {
                Debug.LogError($"[ShipManager] Invalid Ship Data: Prefabs are null");
                return false;
            }
            if (shipPrefabs.Count == 0)
            {
                Debug.LogError($"[ShipManager] Invalid Ship Data: Prefabs list is empty");
                return false;
            }
            if (initialShipCount < 0)
            {
                Debug.LogError($"[ShipManager] Invalid Ship Data: initialShipCount is less than 0: {initialShipCount}");
                return false;
            }
            
             if (initialPirateCount < 0)
             {
                Debug.LogError($"[ShipManager] Invalid Ship Data: initialPirateCount is less than 0: {initialPirateCount}");
                return false;
             }
             if (spawnRadius <= 0)
             {
                Debug.LogError($"[ShipManager] Invalid Ship Data: spawnRadius is less than or equal to 0: {spawnRadius}");
                 return false;
            }
            return true;
        }
    }

    #endregion
}