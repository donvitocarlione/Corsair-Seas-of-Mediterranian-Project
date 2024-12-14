using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FactionData
{
    public Faction faction;
    public string name;
    public Color color = Color.white;
    public float reputation = 50f;
    public List<Pirate> pirates = new List<Pirate>();
    public List<Ship> ships = new List<Ship>();
    public List<Port> ports = new List<Port>();
    
    // Relations with other factions (0-100)
    public Dictionary<Faction, float> relations = new Dictionary<Faction, float>();

    public FactionData(Faction faction, string name)
    {
        this.faction = faction;
        this.name = name;
    }

    public void Initialize()
    {
        pirates = new List<Pirate>();
        ships = new List<Ship>();
        ports = new List<Port>();
        relations = new Dictionary<Faction, float>();
    }

    public void SetRelation(Faction otherFaction, float value)
    {
        relations[otherFaction] = Mathf.Clamp(value, 0f, 100f);
    }

    public float GetRelation(Faction otherFaction)
    {
        if (relations.TryGetValue(otherFaction, out float value))
        {
            return value;
        }
        return 50f; // Default neutral relation
    }
}
