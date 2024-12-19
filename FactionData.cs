using UnityEngine;
using System.Collections.Generic;

namespace CorsairGame
{
    [System.Serializable]
    public class FactionData
    {
        public FactionType faction;
        public string factionName;
        public float influence;
        public List<Pirate> members = new List<Pirate>();
        public Dictionary<FactionType, float> relationships = new Dictionary<FactionType, float>();

        public void Initialize(FactionType type, string name)
        {
            faction = type;
            factionName = name;
            influence = FactionConstants.DEFAULT_INFLUENCE;
        }

        public void AddMember(Pirate pirate)
        {
            if (!members.Contains(pirate))
            {
                members.Add(pirate);
            }
        }

        public void RemoveMember(Pirate pirate)
        {
            members.Remove(pirate);
        }

        public void SetRelationship(FactionType otherFaction, float value)
        {
            if (otherFaction == faction)
            {
                Debug.LogWarning(FactionConstants.ERROR_SAME_FACTION);
                return;
            }

            value = Mathf.Clamp(value, FactionConstants.MIN_RELATIONSHIP, FactionConstants.MAX_RELATIONSHIP);
            relationships[otherFaction] = value;
        }

        public float GetRelationship(FactionType otherFaction)
        {
            if (relationships.TryGetValue(otherFaction, out float value))
            {
                return value;
            }
            return FactionConstants.DEFAULT_STARTING_RELATIONSHIP;
        }

        public bool IsHostileTo(FactionType otherFaction)
        {
            return GetRelationship(otherFaction) <= FactionConstants.HOSTILE_THRESHOLD;
        }

        public bool IsFriendlyTo(FactionType otherFaction)
        {
            return GetRelationship(otherFaction) >= FactionConstants.FRIENDLY_THRESHOLD;
        }
    }
}