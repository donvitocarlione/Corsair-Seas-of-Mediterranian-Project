 using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using CSM.Base; // Added namespace
using static ShipExtensions;
using System.Collections;

using Random = UnityEngine.Random;  // Add this at the top with your other using statements

[AddComponentMenu("Game/Ship Manager")]
public class ShipManager : MonoBehaviour
{
    public static ShipManager Instance { get; private set; }

    [Header("References")]
    public GameObject piratePrefab;
    [SerializeField] private FactionManager factionManager; // Keep existing factionManager reference

    [Header("Faction Settings")]
    public List<FactionShipData> factionShipData;
    public float minSpawnDistance = 50f;
    public int maxSpawnAttempts = 10;

    private Transform shipsParent;
    private Transform piratesParent;
    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
    private bool isInitialized;
    private WaterBody waterBody;


    private Player playerInstance;
    private Queue<(int count, int type)> pendingShipSpawns = new Queue<(int, int)>();

        // New dictionary for tracking ships by faction
 

    #region Unity Methods
    void Awake()
    {
         if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // Initialize manager only after ensuring factions are ready
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
        // Wait one frame to ensure FactionManager's Awake has completed
        yield return null;

        // Queue ship spawns instead of spawning immediately
        foreach (var factionData in factionShipData)
        {
            // Instead of using the old method, we will just use a null faction.
              pendingShipSpawns.Enqueue((factionData.initialShipCount, 0)); // Added dummy value for type
        }

         StartCoroutine(ProcessPendingShipSpawns());

    }
     private IEnumerator ProcessPendingShipSpawns()
    {
        while (pendingShipSpawns.Count > 0)
        {
            var (count, type) = pendingShipSpawns.Peek();

            // Wait for faction owner to be available
           IEntityOwner owner =  playerInstance;
        
            if (owner == null)
            {
                 Debug.Log($"[ShipManager] Waiting for owner for faction");
                 yield return new WaitForSeconds(0.1f);
                 continue;
            }
            
            pendingShipSpawns.Dequeue();


            for (int i = 0; i < count; i++)
            {
                SpawnShipForFaction();
                yield return null; // Spread spawning across frames
            }
        }
           Debug.Log("[ShipManager] All ships spawned successfully");
    }


    #region Initialization
      private void InitializeManager()
    {
        Debug.Log("[ShipManager] Initializing manager");
        CreateContainers();
        InitializeWaterBody();

        // Add this block
        if (factionManager == null)
        {
            factionManager = GameObject.FindAnyObjectByType<FactionManager>();
            if (factionManager == null)
            {
                Debug.LogError("[ShipManager] FactionManager not found!");
                enabled = false;
                return;
            }
        }


        isInitialized = ValidateConfiguration();

        if (isInitialized)
        {
           // Move ship spawning to the coroutine
        }
        else
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
        if (factionShipData == null || factionShipData.Count == 0)
        {
            Debug.LogError("[ShipManager] No faction ship data provided.");
            return false;
        }
        foreach(var data in factionShipData)
        {
            if(!data.Validate())
            {
                 return false;
            }
        }
       
        return true;
    }

   private void InitializeWaterBody()
    {
        waterBody = FindAnyObjectByType<WaterBody>(); // Changed to FindAnyObjectByType
        if (waterBody == null)
        {
            Debug.LogError("[ShipManager] WaterBody not found. Ships might not spawn correctly.");
        }
    }


   

    #endregion

    #region Ship Spawning
    //Modified to use Faction Owner instead of just Faction type.
      public Ship SpawnShipForFaction( Vector3? customPosition = null)
        {
            if (!isInitialized)
            {
                 pendingShipSpawns.Enqueue((1,0)); // Add dummy value to type
                return null;
            }
            
            IEntityOwner owner =  playerInstance;
            
            Debug.Log($"[ShipManager] Spawning ship with owner {owner?.GetType().Name}");
             var factionData = GetFactionShipData();
              var prefab = factionData.shipPrefabs[Random.Range(0, factionData.shipPrefabs.Count)];
              var position = customPosition ?? GetSafeSpawnPosition(factionData.spawnArea, factionData.spawnRadius);
            
            var shipInstance = Instantiate(prefab, position, Quaternion.identity, shipsParent);
            var ship = shipInstance.GetComponent<Ship>();
             if (ship != null)
            {
                string shipName = $"Ship_{Random.Range(1000, 9999)}";
                 ship.Initialize(shipName,  owner);
    
                    // Assign ship to faction's collection
            }
    
             return ship;
        }
        
        public Ship SpawnPirateShip(Vector3? customPosition = null)
        {
            if (!isInitialized)
            {
                Debug.LogError($"[ShipManager] Cannot spawn pirate ship. Manager is not initialized.");
                return null;
            }
    
            //Add this block
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
    
    
            var factionData = GetFactionShipData();
            if (factionData == null)
            {
                Debug.LogError($"[ShipManager] No faction data. Cannot spawn pirate ship.");
                return null;
            }
            Vector3 spawnPosition;
            if (customPosition.HasValue)
            {
                spawnPosition = customPosition.Value;
            }
            else
            {
                spawnPosition = GetSafeSpawnPosition(factionData.spawnArea, factionData.spawnRadius);
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
                  // Assign ship to faction's collection
    
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
    
                float waterSurfaceHeight = waterBody.GetWaterSurfaceHeight(); // Use water surface height
                for (int i = 0; i < maxSpawnAttempts; i++)
                {
                    // Generate random position only in X and Z
                    float randomX = center.x + Random.Range(-radius, radius);
                    float randomZ = center.z + Random.Range(-radius, radius);
                    
                    // Create position vector with exact water surface height
                    Vector3 spawnPosition = new Vector3(randomX, waterSurfaceHeight, randomZ);
    
                    if (IsSafePosition(spawnPosition))
                    {
                        Debug.Log($"[ShipManager] Found safe spawn position at {spawnPosition}, water height: {waterSurfaceHeight}");
                        return spawnPosition;
                    }
                }
    
                // Fallback to a position closer to center if no safe position found
                Vector3 fallbackPosition = new Vector3(
                    center.x + Random.Range(-radius * 0.5f, radius * 0.5f),
                    waterSurfaceHeight,
                    center.z + Random.Range(-radius * 0.5f, radius * 0.5f)
                );
                
                Debug.LogWarning($"[ShipManager] Could not find safe position after {maxSpawnAttempts} attempts. Using fallback position: {fallbackPosition}");
                return fallbackPosition;
            }
            private bool IsSafePosition(Vector3 position) // Removed radius parameter
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
                throw new System.ArgumentNullException(nameof(player));
    
            if (playerInstance != null && playerInstance != player)
            {
                Debug.LogWarning("Attempting to register a new player while one is already registered. Unregistering previous player.");
                UnregisterPlayer();
            }
    
            playerInstance = player;
    
              foreach (var ship in player.GetOwnedShips())
            {
                RegisterShip(ship);
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
     
        private FactionShipData GetFactionShipData()
        {
            return factionShipData.FirstOrDefault();
        }
    
        void OnValidate()
        {
            if (minSpawnDistance < 0) minSpawnDistance = 50f;
            if (maxSpawnAttempts < 1) maxSpawnAttempts = 10;
        }
        #endregion
    
        #region Editor Classes
        [System.Serializable]
        public class FactionShipData
        {
            public List<GameObject> shipPrefabs;
            public Vector3 spawnArea;
            public float spawnRadius = 100f;
            public int initialShipCount = 3;
          //  public bool isPlayerFaction;
            public int initialPirateCount = 2;
            public bool Validate()
            {
                if (shipPrefabs == null || shipPrefabs.Count == 0)
                {
                    // Debug.LogError($"[ShipManager] No ship prefabs for faction {faction}.");
                    return false;
                }
    
                if (initialShipCount < 0)
                {
                    //Debug.LogError($"[ShipManager] Initial ship count must be a positive value for faction {faction}.");
                    return false;
                }
    
                if (initialPirateCount < 0)
                {
                    // Debug.LogError($"[ShipManager] Initial pirate count must be a positive value for faction {faction}.");
                    return false;
                }
                if (spawnRadius <= 0)
                {
                    //Debug.LogError($"[ShipManager] Spawn radius must be a positive value for faction {faction}.");
                    return false;
                }
                return true;
            }
        }
    
        #endregion
    }