using UnityEngine;

namespace CorsairGame
{
    [System.Serializable]
    public class FactionShipData
    {
        public string shipName;
        public FactionType factionType;
        public GameObject shipPrefab;
        public int maxHealth = 100;
        public float speed = 10f;
        public float turnSpeed = 30f;
        public float accelerationRate = 5f;
        public float brakingRate = 3f;
        
        // Combat stats
        public int cannonDamage = 10;
        public float reloadTime = 2f;
        public float cannonRange = 50f;
        public int maxCrewSize = 20;
        
        // Economic stats
        public int cargoCapacity = 100;
        public int maintenanceCost = 10;
        public int hirePrice = 1000;
    }
}