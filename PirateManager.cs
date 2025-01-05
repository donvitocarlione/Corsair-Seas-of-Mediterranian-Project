using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CSM.Base;

public class PirateManager : MonoBehaviour
{
    public static PirateManager Instance { get; private set; }

    [Header("Pirate Settings")]
    [SerializeField] private List<PirateData> pirateDataList;
    [SerializeField] private GameObject piratePrefab;
    [SerializeField] private GameObject playerPrefab;


    private List<Pirate> activePirates = new List<Pirate>();
    private Player player;

    #region Unity Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(InitializeWhenShipManagerReady());
    }

    #endregion

    #region Initialization

     private IEnumerator InitializeWhenShipManagerReady()
    {
        while (!ShipManager.Instance.IsInitialized)
        {
            yield return null;
        }

         InitializePirateManager();
    }

    private void InitializePirateManager()
    {
         if (pirateDataList == null || pirateDataList.Count == 0)
        {
            Debug.LogError("[PirateManager] No initial pirate data provided.");
           enabled = false;
           return;
        }

         SpawnPlayer();
         SpawnPirates();

        Debug.Log("[PirateManager] Initialized successfully");
    }
    #endregion

     #region Pirate Creation

    private void SpawnPlayer()
    {
       var playerData = pirateDataList.Find(p => p.isPlayer);
        if (playerData == null)
        {
           Debug.LogError("[PirateManager] No player data found!");
            return;
        }

        var playerObj = Instantiate(playerPrefab);
        player = playerObj.GetComponent<Player>();
        player.Initialize(playerData);
        activePirates.Add(player);

       SpawnInitialShips(player, playerData);
    }

    private void SpawnPirates()
    {
       foreach (var pirateData in pirateDataList.Where(p => !p.isPlayer))
        {
            var pirateObj = Instantiate(piratePrefab);
            var pirate = pirateObj.GetComponent<Pirate>();
            pirate.Initialize(pirateData);
           activePirates.Add(pirate);

           SpawnInitialShips(pirate, pirateData);
       }
   }

    private void SpawnInitialShips(Pirate pirate, PirateData pirateData)
    {
        for (int i = 0; i < pirateData.maxShips; i++)
        {
            // Request ship spawn from ShipManager
            var randomPrefab = pirateData.preferredShipPrefabs[UnityEngine.Random.Range(0, pirateData.preferredShipPrefabs.Count)];
            var spawnPos = GetRandomSpawnPosition(pirateData.spawnArea, pirateData.spawnRadius);
            ShipManager.Instance.SpawnShip(pirate, randomPrefab, spawnPos);
        }
    }


    private Vector3 GetRandomSpawnPosition(Vector3 center, float radius)
    {
       var randomAngle = UnityEngine.Random.Range(0f, 360f);
        var randomDistance = UnityEngine.Random.Range(0f, radius);
        var offset = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward * randomDistance;
        return center + offset;
    }

    #endregion

     #region Helper Methods
      public List<Pirate> GetActivePirates()
     {
         return activePirates;
     }
    #endregion
}