using UnityEngine;
using System.Collections.Generic;
using CSM.Base;

public class ShipManagerConfig : MonoBehaviour
{
    [Header("Faction Settings")]
    [SerializeField] private List<FactionShipData> factionShipData;
    
    [Header("Spawn Settings")]
    [SerializeField] private float minSpawnDistance = 50f;
    [SerializeField] private int maxSpawnAttempts = 10;
    [SerializeField] private float defaultSpawnHeightAboveWater = 5f;
    
    [Header("Debug Settings")]
    [SerializeField] private bool debugSpawnPositions = true;

    // Public accessors
    public List<FactionShipData> FactionData => factionShipData;
    public float MinSpawnDistance => minSpawnDistance;
    public int MaxSpawnAttempts => maxSpawnAttempts;
    public float DefaultSpawnHeightAboveWater => defaultSpawnHeightAboveWater;
    public bool DebugSpawnPositions => debugSpawnPositions;

    private void OnValidate()
    {
        ValidateSpawnSettings();
        ValidateFactionData();
    }

    private void ValidateSpawnSettings()
    {
        if (minSpawnDistance < 0)
        {
            minSpawnDistance = 50f;
            Debug.LogWarning("[ShipManagerConfig] MinSpawnDistance cannot be negative, reset to 50");
        }

        if (maxSpawnAttempts < 1)
        {
            maxSpawnAttempts = 10;
            Debug.LogWarning("[ShipManagerConfig] MaxSpawnAttempts must be at least 1, reset to 10");
        }

        if (defaultSpawnHeightAboveWater < 1f)
        {
            defaultSpawnHeightAboveWater = 5f;
            Debug.LogWarning("[ShipManagerConfig] DefaultSpawnHeightAboveWater must be at least 1, reset to 5");
        }
    }

    private void ValidateFactionData()
    {
        if (factionShipData == null || factionShipData.Count == 0)
        {
            Debug.LogError("[ShipManagerConfig] No faction data configured!");
            return;
        }

        bool hasPlayerFaction = false;
        foreach (var data in factionShipData)
        {
            if (data == null) continue;

            if (data.IsPlayerFaction)
            {
                if (hasPlayerFaction)
                {
                    Debug.LogError("[ShipManagerConfig] Multiple player factions detected!");
                }
                hasPlayerFaction = true;
            }

            if (data.SpawnHeightAboveWater < 1f)
            {
                Debug.LogWarning($"[ShipManagerConfig] Spawn height for faction {data.Faction} is too low, will use default value");
            }

            if (data.ShipPrefabs == null || data.ShipPrefabs.Count == 0)
            {
                Debug.LogError($"[ShipManagerConfig] No ship prefabs configured for faction {data.Faction}");
            }
            else
            {
                foreach (var prefab in data.ShipPrefabs)
                {
                    if (prefab == null)
                    {
                        Debug.LogError($"[ShipManagerConfig] Null ship prefab in faction {data.Faction}");
                        continue;
                    }

                    if (prefab.GetComponent<Ship>() == null)
                    {
                        Debug.LogError($"[ShipManagerConfig] Ship prefab missing Ship component in faction {data.Faction}");
                    }
                }
            }
        }

        if (!hasPlayerFaction)
        {
            Debug.LogError("[ShipManagerConfig] No player faction configured!");
        }
    }

    public FactionShipData GetPlayerFactionData()
    {
        return factionShipData?.Find(data => data.IsPlayerFaction);
    }

    public FactionShipData GetFactionData(FactionType faction)
    {
        return factionShipData?.Find(data => data.Faction == faction);
    }

    public bool ValidateConfiguration()
    {
        if (factionShipData == null || factionShipData.Count == 0)
        {
            Debug.LogError("[ShipManagerConfig] No faction data configured!");
            return false;
        }

        if (GetPlayerFactionData() == null)
        {
            Debug.LogError("[ShipManagerConfig] No player faction configured!");
            return false;
        }

        return factionShipData.TrueForAll(data => data != null && data.Validate());
    }
}