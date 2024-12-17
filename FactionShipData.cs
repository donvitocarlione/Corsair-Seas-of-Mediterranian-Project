using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FactionShipData
{
    [Header("Faction Settings")]
    public FactionType Faction;
    public bool IsPlayerFaction;
    
    [Header("Ship Settings")]
    public List<GameObject> ShipPrefabs = new List<GameObject>();
    
    [Header("Spawn Settings")]
    public float SpawnHeightAboveWater = 5f;
    public float SpawnRadius = 100f;
    public Vector2 SpawnArea = new Vector2(100f, 100f);
    
    [Header("Spawn Area Settings")]
    public float SpawnAreaRadius = 200f;

    public bool Validate()
    {
        if (ShipPrefabs == null || ShipPrefabs.Count == 0)
        {
            Debug.LogError($"[FactionShipData] No ship prefabs configured for faction {Faction}");
            return false;
        }

        if (SpawnHeightAboveWater < 1f)
        {
            Debug.LogWarning($"[FactionShipData] Spawn height for faction {Faction} is too low");
            SpawnHeightAboveWater = 5f;
        }

        foreach (var prefab in ShipPrefabs)
        {
            if (prefab == null)
            {
                Debug.LogError($"[FactionShipData] Null ship prefab in faction {Faction}");
                return false;
            }

            if (prefab.GetComponent<Ship>() == null)
            {
                Debug.LogError($"[FactionShipData] Ship prefab missing Ship component in faction {Faction}");
                return false;
            }
        }

        return true;
    }
}
