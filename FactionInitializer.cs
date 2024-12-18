using UnityEngine;
using System.Collections.Generic;

public class FactionInitializer : MonoBehaviour
{
    [System.Serializable]
    public class InitialFactionData
    {
        public Faction faction;
        public int initialShipCount = 3;
        public List<GameObject> shipPrefabs;
        public Dictionary<Faction, float> initialRelations;

        public GameObject GetRandomShipPrefab()
        {
            if (shipPrefabs == null || shipPrefabs.Count == 0) return null;
            return shipPrefabs[Random.Range(0, shipPrefabs.Count)];
        }
    }

    [SerializeField] private List<InitialFactionData> factionData;
    [SerializeField] private float defaultNeutralRelation = 50f;
    [SerializeField] private bool debugMode = false;

    private ShipSpawner shipSpawner;

    public void Initialize(ShipSpawner spawner)
    {
        shipSpawner = spawner;
        if (debugMode)
        {
            Debug.Log("[FactionInitializer] Components initialized");
        }
    }

    public void InitializeAllFactions()
    {
        if (FactionManager.Instance == null || shipSpawner == null)
        {
            Debug.LogError("[FactionInitializer] Cannot initialize factions - missing components");
            return;
        }

        // First, register all factions with FactionManager
        RegisterFactions();

        // Then set up initial relations and spawn ships
        foreach (var data in factionData)
        {
            if (data.faction != null)
            {
                SetupFactionRelations(data);
                SpawnInitialShips(data);
            }
        }

        if (debugMode)
        {
            Debug.Log("[FactionInitializer] All factions initialized");
        }
    }

    private void RegisterFactions()
    {
        foreach (var data in factionData)
        {
            if (data.faction != null)
            {
                FactionManager.Instance.RegisterFaction(data.faction);
                if (debugMode)
                {
                    Debug.Log($"[FactionInitializer] Registered faction: {data.faction.FactionName}");
                }
            }
        }
    }

    private void SetupFactionRelations(InitialFactionData data)
    {
        if (data.initialRelations != null)
        {
            foreach (var relation in data.initialRelations)
            {
                if (relation.Key != null)
                {
                    FactionManager.Instance.SetRelationship(data.faction, relation.Key, relation.Value);
                    if (debugMode)
                    {
                        Debug.Log($"[FactionInitializer] Set relationship between {data.faction.FactionName} and {relation.Key.FactionName} to {relation.Value}");
                    }
                }
            }
        }
        else
        {
            // Set default neutral relations with all other factions
            foreach (var otherData in factionData)
            {
                if (otherData.faction != null && otherData.faction != data.faction)
                {
                    FactionManager.Instance.SetRelationship(data.faction, otherData.faction, defaultNeutralRelation);
                }
            }
        }
    }

    private void SpawnInitialShips(InitialFactionData data)
    {
        if (shipSpawner != null && data.faction != null)
        {
            for (int i = 0; i < data.initialShipCount; i++)
            {
                GameObject shipPrefab = data.GetRandomShipPrefab();
                if (shipPrefab != null)
                {
                    Ship ship = shipSpawner.SpawnShip(
                        data.faction,
                        shipPrefab,
                        GetRandomSpawnPosition(),
                        Quaternion.Euler(0, Random.Range(0f, 360f), 0)
                    );
                    
                    if (ship != null && debugMode)
                    {
                        Debug.Log($"[FactionInitializer] Successfully spawned {ship.ShipName} for {data.faction.FactionName}");
                    }
                }
            }

            if (debugMode)
            {
                Debug.Log($"[FactionInitializer] Spawned {data.initialShipCount} ships for {data.faction.FactionName}");
            }
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float spawnRadius = 100f;
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        return new Vector3(randomCircle.x, 0, randomCircle.y);
    }
}
