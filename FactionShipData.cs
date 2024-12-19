using UnityEngine;
using System.Collections.Generic;
using CSM.Base;

namespace CorsairGame
{
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

        [SerializeField] private float spawnHeightAboveWater;
        public float SpawnHeightAboveWater => spawnHeightAboveWater;
        public List<GameObject> ShipPrefabs => shipPrefabs;
        public FactionType Faction => faction;
        public bool IsPlayerFaction => isPlayerFaction;

        public bool Validate()
        {
            if (shipPrefabs == null || shipPrefabs.Count == 0)
            {
                Debug.LogError($"Missing ship prefabs for faction {faction}");
                return false;
            }

            foreach (var prefab in shipPrefabs)
            {
                if (prefab == null || prefab.GetComponent<Ship>() == null)
                {
                    Debug.LogError($"Invalid ship prefab configuration for faction {faction}");
                    return false;
                }
            }

            return spawnRadius > 0;
        }
    }
}