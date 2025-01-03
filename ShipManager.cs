using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using CSM.Base;
using static ShipExtensions;
using System.Collections;

using Random = UnityEngine.Random;

[AddComponentMenu("Game/Ship Manager")]
public class ShipManager : MonoBehaviour
{
    public static ShipManager Instance { get; private set; }

    [Header("References")]
    public GameObject piratePrefab;

    [Header("Ship Spawning Settings")]
    public List<InitialShipData> initialShipData;
    public float minSpawnDistance = 50f;
    public int maxSpawnAttempts = 10;

    private Transform shipsParent;
    private Transform piratesParent;
    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
    private bool isInitialized;
    private WaterBody waterBody;

    private Player playerInstance;
    private Queue<(int count, bool isPirate)> pendingShipSpawns = new Queue<(int, bool)>();

    #region Unity Methods
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(InitializeManagerWhenReady());
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    private IEnumerator InitializeManagerWhenReady()
    {
        yield return null; // Wait one frame

        // Queue initial ship spawns
        foreach (var shipData in initialShipData)
        {
            pendingShipSpawns.Enqueue((shipData.initialShipCount, false)); // Player Ships
            pendingShipSpawns.Enqueue((shipData.initialPirateCount, true)); //Pirate Ships
        }


        StartCoroutine(ProcessPendingShipSpawns());
    }


    private IEnumerator ProcessPendingShipSpawns()
    {
        while (pendingShipSpawns.Count > 0)
        {
            var (count, isPirate) = pendingShipSpawns.Dequeue();


            for (int i = 0; i < count; i++)
            {
                if (isPirate)
                {
                    SpawnPirateShip();
                }
                else
                {
                  
                        SpawnShipForPlayer();
                    
                }

               yield return null;
            }
        }
        Debug.Log("[ShipManager] All initial ships spawned successfully");
    }

    #region Initialization
    private void InitializeManager()
    {
        Debug.Log("[ShipManager] Initializing manager");
        CreateContainers();
        InitializeWaterBody();
        isInitialized = ValidateConfiguration();

        if (!isInitialized)
        {
            Debug.LogError("[ShipManager] Invalid configuration, disabling manager.");
            enabled = false;
        }
    }

    private void CreateContainers()
    {
        shipsParent = new GameObject("Ships Container").transform;
        shipsParent.SetParent(transform);
        piratesParent = new GameObject("Pirates Container").transform;
        piratesParent.SetParent(transform);
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
    public Ship SpawnShipForPlayer(Vector3? customPosition = null)
    {
         if (!isInitialized)
        {
            pendingShipSpawns.Enqueue((1, false));
            return null;
        }
          if (playerInstance == null)
        {
             Debug.Log($"[ShipManager] Waiting for player to be initialized");
            return null;
        }
            Debug.Log($"[ShipManager] Spawning ship with owner {playerInstance?.GetType().Name}");
            var shipData = GetInitialShipData();
        if (shipData == null)
        {
             Debug.LogError($"[ShipManager] No ship data found!");
                return null;
        }
            var prefab = shipData.shipPrefabs[Random.Range(0, shipData.shipPrefabs.Count)];
            var position = customPosition ?? GetSafeSpawnPosition(shipData.spawnArea, shipData.spawnRadius);
            var shipInstance = Instantiate(prefab, position, Quaternion.identity, shipsParent);
            var ship = shipInstance.GetComponent<Ship>();
            if (ship != null)
            {
                string shipName = $"Ship_{Random.Range(1000, 9999)}";
                 ship.Initialize(shipName, playerInstance);
                
            }

            return ship;
    }


    public Ship SpawnPirateShip(Vector3? customPosition = null)
    {
         if (!isInitialized)
        {
             pendingShipSpawns.Enqueue((1, true));
            return null;
        }

        if (piratePrefab == null)
        {
            Debug.LogError("[ShipManager] Pirate prefab not assigned!");
            return null;
        }

        if (!piratePrefab.GetComponent<Ship>())
        {
            Debug.LogError("[ShipManager] Pirate prefab must have Ship component!");
            return null;
        }


        var shipData = GetInitialShipData();
         if (shipData == null)
        {
             Debug.LogError($"[ShipManager] No ship data found!");
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
                Debug.LogWarning($"[ShipManager] No available position found. Cannot spawn pirate ship.");
                return null;
            }
        }
        occupiedPositions.Add(spawnPosition);

        GameObject shipInstance = Instantiate(piratePrefab, spawnPosition, Quaternion.identity, piratesParent);
        Ship ship = shipInstance.GetComponent<Ship>();
        if (ship != null)
        {
            string shipName = $"Pirate_{Random.Range(1000, 9999)}";
            IEntityOwner owner = null;
            if (owner == null)
            {
                Debug.LogWarning($"[ShipManager] Could not get or create Pirate owner.");
            }

            ship.Initialize(shipName, owner);
             Debug.Log($"[ShipManager] Pirate Ship {shipName} initialized and registered");
        }
        else
        {
            Debug.LogError($"[ShipManager] Could not find Ship component on prefab!");
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
    #endregion

    #region Existing Ship Registration
    public void RegisterPlayer(Player player)
    {
        if (player == null)
            throw new ArgumentNullException(nameof(player));

        if (playerInstance != null && playerInstance != player)
        {
            Debug.LogWarning("Attempting to register a new player while one is already registered. Unregistering previous player.");
            UnregisterPlayer();
        }

        playerInstance = player;
         if(isInitialized)
        {
            
            foreach (var ship in player.GetOwnedShips())
            {
                RegisterShip(ship);
            }
        }

        Debug.Log($"Player registered with ShipManager");
    }

    public void UnregisterPlayer()
    {
        if (playerInstance != null)
        {
             foreach (var ship in playerInstance.GetOwnedShips())
            {
                if (ship != null)
                {
                    UnregisterShip(ship);
                }
            }


            playerInstance = null;
            Debug.Log("Player unregistered from ShipManager");
        }
    }


      public void RegisterShip(Ship ship)
    {
        if (ship == null)
            throw new ArgumentNullException(nameof(ship));
        if (ship.Owner
            == null)
        {
            Debug.LogError($"[ShipManager] Ship {ship.Name} has no owner during registration");
            return;
        }


    }

    public void UnregisterShip(Ship ship)
    {
        if (ship == null)
            throw new System.ArgumentNullException(nameof(ship));
    }


    public void OnShipDestroyed(Ship ship)
    {
        if (ship != null)
        {
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

     void OnValidate()
    {
        if (minSpawnDistance < 0) minSpawnDistance = 50f;
        if (maxSpawnAttempts < 1) maxSpawnAttempts = 10;
    }

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
           if (shipPrefabs == null || shipPrefabs.Count == 0)
            {
                return false;
            }
           if (initialShipCount < 0)
            {
               return false;
            }

            if (initialPirateCount < 0)
            {
                return false;
            }
            if (spawnRadius <= 0)
            {
                return false;
            }
            return true;
        }

    }
    #endregion
}