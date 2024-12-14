using UnityEngine;
using System.Collections.Generic;

namespace CSM.Base
{
    [System.Serializable]
    public class FactionShipData
    {
        [SerializeField]
        private FactionType _faction;
        [SerializeField]
        private List<GameObject> _shipPrefabs;
        [SerializeField]
        private Vector3 _spawnArea;
        [SerializeField]
        private float _spawnRadius = 100f;
        [SerializeField]
        private int _initialShipCount = 3;
        [SerializeField]
        private bool _isPlayerFaction;
        [SerializeField]
        private int _initialPirateCount = 2;
        [SerializeField]
        private float _spawnHeightAboveWater = 5f;

        public FactionType Faction => _faction;
        public List<GameObject> ShipPrefabs => _shipPrefabs;
        public Vector3 SpawnArea => _spawnArea;
        public float SpawnRadius => _spawnRadius;
        public int InitialShipCount => _initialShipCount;
        public bool IsPlayerFaction => _isPlayerFaction;
        public int InitialPirateCount => _initialPirateCount;
        public float SpawnHeightAboveWater => _spawnHeightAboveWater;

        public bool Validate()
        {
            if (_shipPrefabs == null || _shipPrefabs.Count == 0)
            {
                Debug.LogError($"Missing ship prefabs for faction {_faction}");
                return false;
            }

            foreach (var prefab in _shipPrefabs)
            {
                if (prefab == null || prefab.GetComponent<Ship>() == null)
                {
                    Debug.LogError($"Invalid ship prefab configuration for faction {_faction}");
                    return false;
                }
            }

            return _spawnRadius > 0;
        }
    }
}