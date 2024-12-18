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
  
    public Ship SpawnShip(Faction faction, GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject shipObj = Instantiate(prefab, position, rotation, shipParent);
        Ship ship = shipObj.GetComponent<Ship>();
        return SetupShip(ship, faction);
    }

    public Ship SpawnShip(Faction faction, Vector3 position, Quaternion rotation)
    {
        GameObject shipObj = Instantiate(shipPrefab, position, rotation, shipParent);
        Ship ship = shipObj.GetComponent<Ship>();
        return SetupShip(ship, faction);
    }
    
    private Ship SetupShip(Ship ship, Faction faction)
    {
        if (ship == null)
        {
            Debug.LogError("[ShipSpawner] Ship component not found on prefab!");
            return null;
        }
    
        // Set up AI controller for non-player ships
        if (faction != FactionManager.Instance.PlayerFaction)
        {
            var aiController = ship.gameObject.AddComponent<AIShipController>();
            aiController.Initialize(ship);
            ship.transform.SetParent(pirateParent, true);
        }
    
        // Set faction
        var factionMember = ship.gameObject.GetComponent<FactionMember>();
        if (factionMember == null)
        {
            factionMember = ship.gameObject.AddComponent<FactionMember>();
        }
        factionMember.SetFaction(faction);

        // Register the ship if registry exists
        if (shipRegistry != null)
        {
            shipRegistry.RegisterShip(ship);
        }
    
        if (debugMode)
        {
            Debug.Log($"[ShipSpawner] Spawned ship of faction {faction.FactionName} at {ship.transform.position}");
        }
    
        return ship;
    }

    public Ship SpawnShipAtRandomPoint(Faction faction)
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        return SpawnShip(faction, shipPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}