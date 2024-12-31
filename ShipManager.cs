using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CSM.Base; // Added namespace
using static ShipExtensions;
using System.Collections;

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
    private FactionType playerFaction;
    private WaterBody waterBody;


    private Player playerInstance;
    private Dictionary<Ship, FactionType> registeredShips = new Dictionary<Ship, FactionType>();


    #region Unity Methods
    void Awake()
    {
         if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // Initialize manager only after ensuring factions are ready
            StartCoroutine(InitializeManagerWhenFactionsReady());
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

     private IEnumerator InitializeManagerWhenFactionsReady()
    {
        // Wait one frame to ensure FactionManager's Awake has completed
        yield return null;

        // Verify all required factions are initialized before proceeding
        if (!VerifyFactionsInitialized())
        {
            Debug.LogError("[ShipManager] Cannot initialize - some factions are not ready!");
            yield break;
        }

        InitializeManager();
    }

    private bool VerifyFactionsInitialized()
    {
        // Check each faction type that we need
        foreach (FactionType factionType in Enum.GetValues(typeof(FactionType)))
        {
            if (!factionManager.IsFactionInitialized(factionType))
            {
                Debug.LogError($"[ShipManager] Faction not initialized: {factionType}");
                return false;
            }
        }
        return true;
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
            InitializeAllFactions();
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


    private void InitializeAllFactions()
    {
        foreach (var factionData in factionShipData)
        {
             if (factionData.isPlayerFaction)
            {
                 InitializePlayerFaction(factionData);
            }
            else
            {
                  InitializePiratesForFaction(factionData);
            }

             for (int i = 0; i < factionData.initialShipCount; i++)
            {
                SpawnShipForFaction(factionData.faction);
            }
        }
    }
    private void InitializePlayerFaction(FactionShipData factionData)
    {
        playerFaction = factionData.faction;
    }
    private void InitializePiratesForFaction(FactionShipData factionData)
    {
        for (int i = 0; i < factionData.initialPirateCount; i++)
        {
            SpawnPirateShip(factionData.faction);
        }
    }

    #endregion

    #region Ship Spawning
    //Modified to use Faction Owner instead of just Faction type.
     public Ship SpawnShipForFaction(FactionType faction, Vector3? customPosition = null)
    {
        if (!isInitialized)
        {
            Debug.LogError($"[ShipManager] Cannot spawn ship. Manager is not initialized.");
            return null;
        }

        var factionData = GetFactionShipData(faction);
        if (factionData == null)
        {
            Debug.LogError($"[ShipManager] No faction data for {faction}. Cannot spawn ship.");
            return null;
        }

        GameObject prefab = factionData.shipPrefabs[Random.Range(0, factionData.shipPrefabs.Count)];

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
                Debug.LogWarning($"[ShipManager] No available position found for {faction}. Cannot spawn ship.");
                return null;
            }
        }

        occupiedPositions.Add(spawnPosition);
        GameObject shipInstance = Instantiate(prefab, spawnPosition, Quaternion.identity, shipsParent);

        Ship ship = shipInstance.GetComponent<Ship>();
        if (ship != null)
        {
            string shipName = $"{faction}_Ship_{Random.Range(1000, 9999)}";
            
            // Get Pirate owner for the faction
            Pirate pirateOwner = factionManager.GetFactionOwner(faction);
            if (pirateOwner == null)
            {
                Debug.LogError($"[ShipManager] Could not get or create Pirate owner for faction {faction}");
                Destroy(shipInstance);
                return null;
            }
             //Initialize ship with correct owner
            ship.Initialize(faction, shipName, pirateOwner);
           
        }
        return ship;
    }

     public Ship SpawnPirateShip(FactionType faction, Vector3? customPosition = null)
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


        var factionData = GetFactionShipData(faction);
        if (factionData == null)
        {
            Debug.LogError($"[ShipManager] No faction data for {faction}. Cannot spawn pirate ship.");
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
                Debug.LogWarning($"[ShipManager] No available position found for {faction}. Cannot spawn pirate ship.");
                return null;
            }
        }
        occupiedPositions.Add(spawnPosition);

        GameObject shipInstance = Instantiate(piratePrefab, spawnPosition, Quaternion.identity, piratesParent);
        Ship ship = shipInstance.GetComponent<Ship>();
        if (ship != null)
        {
             string shipName = $"{faction}_Pirate_{Random.Range(1000, 9999)}";
             // Get Pirate owner for the faction
             Pirate pirateOwner = factionManager.GetFactionOwner(faction);
             if (pirateOwner == null)
             {
                Debug.LogError($"[ShipManager] Could not get or create Pirate owner for faction {faction}");
                 Destroy(shipInstance);
                  return null;
             }
            
            ship.Initialize(faction, shipName, pirateOwner);
           

            Debug.Log($"[ShipManager] Pirate Ship {shipName} initialized and registered for faction {faction}");
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

         if (factionManager != null)
        {
              factionManager.RegisterPirate(player.Faction, player);
         }

          foreach (var ship in player.GetOwnedShips())
        {
            RegisterShip(ship);
        }

        Debug.Log($"Player registered with ShipManager and faction {player.Faction}");
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
            throw new System.ArgumentNullException(nameof(ship));

        if (!registeredShips.ContainsKey(ship))
        {
            FactionType shipFaction = ship.Faction;
            registeredShips.Add(ship, shipFaction);

            if (factionManager != null)
            {
                factionManager.RegisterShip(shipFaction, ship);
                Debug.Log($"Ship {ship.ShipName()} registered with faction {shipFaction}");
            }
            else
            {
                Debug.LogWarning($"FactionManager not available - Ship {ship.ShipName()} registered only with ShipManager");
            }
        }
    }


    public void UnregisterShip(Ship ship)
    {
        if (ship == null)
            throw new System.ArgumentNullException(nameof(ship));

        if (registeredShips.TryGetValue(ship, out FactionType faction))
        {
            registeredShips.Remove(ship);
            if (factionManager != null)
            {
                factionManager.UnregisterShip(faction, ship);
                Debug.Log($"Ship {ship.ShipName()} unregistered from faction {faction}");
            }
        }
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
    private FactionShipData GetFactionShipData(FactionType faction)
    {
        return factionShipData.FirstOrDefault(data => data.faction == faction);
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
                Debug.LogError($"[ShipManager] No ship prefabs for faction {faction}.");
                return false;
            }

            if (initialShipCount < 0)
            {
                Debug.LogError($"[ShipManager] Initial ship count must be a positive value for faction {faction}.");
                return false;
            }

            if (initialPirateCount < 0)
            {
                Debug.LogError($"[ShipManager] Initial pirate count must be a positive value for faction {faction}.");
                return false;
            }
            if (spawnRadius <= 0)
            {
                Debug.LogError($"[ShipManager] Spawn radius must be a positive value for faction {faction}.");
                return false;
            }
            return true;
        }
    }

    #endregion
}