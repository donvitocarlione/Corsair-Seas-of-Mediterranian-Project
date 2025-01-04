using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSM.Base;

public class PirateManager : MonoBehaviour
{
    public static PirateManager Instance { get; private set; }

    [Header("Pirate Settings")]
    public List<PirateData> initialPirateData;

    private Dictionary<string, Pirate> pirates = new Dictionary<string, Pirate>();
    private bool isInitialized = false;

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
        if (initialPirateData == null || initialPirateData.Count == 0)
        {
            Debug.LogError("[PirateManager] No initial pirate data provided.");
            enabled = false;
            return;
        }

        CreatePlayerPirate();
        CreateInitialPirates();
        isInitialized = true;

        Debug.Log("[PirateManager] Initialized successfully");
    }

    #endregion

    #region Pirate Creation

    public Pirate CreatePirate(PirateData pirateData)
    {
         string uniqueName = GenerateUniquePirateName(pirateData.pirateName);

        GameObject pirateGameObject = new GameObject($"Pirate_{uniqueName}");
        Pirate pirateComponent = pirateGameObject.AddComponent<Pirate>();
        pirateComponent.SetName(uniqueName);
        pirateComponent.SetRank(pirateData.rank);

        pirates.Add(pirateComponent.EntityName, pirateComponent);
        return pirateComponent;
    }


     private string GenerateUniquePirateName(string baseName)
    {
        string uniqueName = baseName;
        int attempt = 1;

        while (pirates.ContainsKey(uniqueName))
        {
            uniqueName = $"{baseName}_{attempt}";
            attempt++;
        }
         return uniqueName;
    }
    #endregion

    #region Ship Spawning

    public Ship RequestShipSpawn(Pirate owner, Vector3? position = null)
    {
        if (owner == null)
        {
            Debug.LogError("[PirateManager] Cannot request ship spawn: Owner is null");
            return null;
        }

        PirateData pirateData = GetPirateData(owner);

        if (pirateData == null)
        {
            Debug.LogError($"[PirateManager] No pirate data found for owner {owner.OwnerName}. Cannot spawn ship");
            return null;
        }

        if (pirateData.preferredShipPrefabs == null || pirateData.preferredShipPrefabs.Count == 0)
        {
            Debug.LogError($"[PirateManager] No ship prefabs defined for {owner.OwnerName}. Cannot spawn ship");
            return null;
        }

        GameObject prefab = pirateData.preferredShipPrefabs[Random.Range(0, pirateData.preferredShipPrefabs.Count)];
        return ShipManager.Instance.SpawnShip(owner, prefab, position);
    }

    #endregion

    #region Helper Methods

    public Pirate GetPirate(string pirateId)
    {
        pirates.TryGetValue(pirateId, out Pirate pirate);
        return pirate;
    }

    public void RemovePirate(string pirateId)
    {
        pirates.Remove(pirateId);
    }

    private PirateData GetPirateData(Pirate pirate)
    {
        return initialPirateData.Find(data => data.pirateName == pirate.EntityName);
    }

    private void CreatePlayerPirate()
    {
        PirateData playerData = initialPirateData.Find(data => data.isPlayer);

        if (playerData == null)
        {
            Debug.LogError("[PirateManager] No player pirate data found. Cannot create player pirate.");
            return;
        }

        Pirate playerPirate = CreatePirate(playerData);

        if (playerPirate != null)
        {
            for (int i = 0; i < playerData.maxShips; i++)
            {
                RequestShipSpawn(playerPirate, playerData.spawnArea);
            }

            Debug.Log($"[PirateManager] Player Pirate {playerPirate.EntityName} created successfully.");
        }
        else
        {
            Debug.LogError("[PirateManager] Failed to create player pirate.");
        }
    }

    private void CreateInitialPirates()
    {
        foreach (var pirateData in initialPirateData)
        {
            if (pirateData.isPlayer) continue;

            Pirate pirate = CreatePirate(pirateData);
            if (pirate != null)
            {
                for (int i = 0; i < pirateData.maxShips; i++)
                {
                    RequestShipSpawn(pirate, pirateData.spawnArea);
                }

                Debug.Log($"[PirateManager] Pirate {pirate.EntityName} created successfully.");
            }
            else
            {
                Debug.LogError($"[PirateManager] Failed to create pirate {pirateData.pirateName}.");
            }
        }
    }

    #endregion

    #region Properties

    public bool IsInitialized => isInitialized;

    #endregion
}