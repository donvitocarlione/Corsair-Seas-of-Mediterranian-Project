using UnityEngine;

public class ShipSpawner : MonoBehaviour
{
    [SerializeField] private GameObject shipPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool debugMode = false;

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

    public Ship SpawnShip(FactionType faction, Vector3 position, Quaternion rotation)
    {
        GameObject shipObj = Instantiate(shipPrefab, position, rotation);
        Ship ship = shipObj.GetComponent<Ship>();

        if (ship == null)
        {
            Debug.LogError("[ShipSpawner] Ship component not found on prefab!");
            Destroy(shipObj);
            return null;
        }

        // Set up AI controller for non-player ships
        if (faction != FactionType.Player)
        {
            var aiController = shipObj.AddComponent<AIShipController>();
            aiController.Initialize(ship);
        }

        // Set faction
        Pirate pirate = shipObj.GetComponent<Pirate>();
        if (pirate != null)
        {
            pirate.SetFaction(faction);
        }

        if (debugMode)
        {
            Debug.Log($"[ShipSpawner] Spawned ship of faction {faction} at {position}");
        }

        return ship;
    }

    public Ship SpawnShipAtRandomPoint(FactionType faction)
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        return SpawnShip(faction, spawnPoint.position, spawnPoint.rotation);
    }
}
