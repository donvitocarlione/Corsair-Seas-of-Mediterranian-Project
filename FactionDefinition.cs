using UnityEngine;
using System.Collections.Generic;

namespace CorsairGame
{
    [System.Serializable]
    public class FactionDefinition
    {
        public FactionType factionType;
        public string displayName;
        public List<Pirate> startingPirates = new List<Pirate>();
        public bool isPlayerFaction;
        
        public float startingInfluence = 100f;
        
        public Dictionary<FactionType, float> initialRelationships = new Dictionary<FactionType, float>();
        
        public void Validate()
        {
            if (string.IsNullOrEmpty(displayName))
            {
                Debug.LogError($"Faction {factionType} has no display name");
            }
            
            if (displayName.Length < FactionConstants.MIN_FACTION_NAME_LENGTH || 
                displayName.Length > FactionConstants.MAX_FACTION_NAME_LENGTH)
            {
                Debug.LogError(string.Format(FactionConstants.ERROR_INVALID_NAME_LENGTH, 
                    FactionConstants.MIN_FACTION_NAME_LENGTH, 
                    FactionConstants.MAX_FACTION_NAME_LENGTH));
            }
            
            if (startingInfluence <= 0)
            {
                Debug.LogWarning($"Faction {factionType} has invalid starting influence. Setting to default.");
                startingInfluence = FactionConstants.DEFAULT_INFLUENCE;
            }
        }
    }
}