using UnityEngine;

public class ShipSpawner : MonoBehaviour
{
    [SerializeField] private GameObject shipPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool debugMode = false;

    private Transform shipParent;
    private Transform pirateParent;
    private WaterBody waterBody;
    private ShipRegistry shipRegistry;

    public void Initialize(Transform shipParent, Transform pirateParent, WaterBody waterBody, ShipRegistry shipRegistry)
    {
        Debug.Log($"[ShipSpawner] Initialized with parent {shipParent.name}");
        this.shipParent = shipParent;
        this.pirateParent = pirateParent;
        this.waterBody = waterBody;
        this.shipRegistry = shipRegistry;
    }

    private void Start()
    {
        if (shipPrefab == null)
        {
            Debug.LogError("[ShipSpawner] Ship prefab not assigned!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[ShipSpawner] No spawn points assigned, using spawner position.");
            spawnPoints = new Transform[] { transform };
        }
    }
  
    public Ship SpawnShip(FactionType faction, GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject shipObj = Instantiate(prefab, position, rotation, shipParent);
        Ship ship = shipObj.GetComponent<Ship>();
        return SetupShip(ship, faction);
    }

    public Ship SpawnShip(FactionType faction, Vector3 position, Quaternion rotation)
    {
        GameObject shipObj = Instantiate(shipPrefab, position, rotation, shipParent);
        Ship ship = shipObj.GetComponent<Ship>();
        return SetupShip(ship, faction);
    }
    
    private Ship SetupShip(Ship ship, FactionType faction)
    {
        if (ship == null)
        {
            Debug.LogError("[ShipSpawner] Ship component not found on prefab!");
            return null;
        }
    
        // Set up AI controller for non-player ships
        if (faction != FactionType.Player)
        {
            var aiController = ship.gameObject.AddComponent<AIShipController>();
            aiController.Initialize(ship);
            ship.transform.SetParent(pirateParent, true);
        }
    
        // Set faction
        Pirate pirate = ship.gameObject.GetComponent<Pirate>();
        if (pirate != null)
        {
            pirate.SetFaction(faction);
        }

        // Register the ship if registry exists
        if (shipRegistry != null)
        {
            shipRegistry.RegisterShip(ship);
        }
    
        if (debugMode)
        {
            Debug.Log($"[ShipSpawner] Spawned ship of faction {faction} at {ship.transform.position}");
        }
    
        return ship;
    }

    public Ship SpawnShipAtRandomPoint(FactionType faction)
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        return SpawnShip(faction, shipPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}