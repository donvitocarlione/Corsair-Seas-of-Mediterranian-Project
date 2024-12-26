using UnityEngine;
using System.Collections.Generic;

public class FactionDefinition
{
    public FactionType Type { get; private set; }
    public string Name { get; private set; }
    public int Influence { get; set; }
    public int ResourceLevel { get; set; }
    public Color Color { get; set; }
    public string BaseLocation { get; set; }

    private Dictionary<FactionType, float> relations = new Dictionary<FactionType, float>();
    private List<Ship> ships = new List<Ship>();
    private List<Port> ports = new List<Port>();
    public List<Pirate> pirates = new List<Pirate>();

    public IReadOnlyList<Ship> Ships => ships.AsReadOnly();
    public IReadOnlyList<Port> Ports => ports.AsReadOnly();

    public FactionDefinition(FactionType type, string name)
    {
        Type = type;
        Name = name;
    }

    public void SetRelation(FactionType otherFaction, float value)
    {
        relations[otherFaction] = value;
    }

    public float GetRelation(FactionType otherFaction)
    {
        return relations.TryGetValue(otherFaction, out float value) ? value : 50f;
    }

    public void AddShip(Ship ship)
    {
        if (!ships.Contains(ship))
        {
            ships.Add(ship);
        }
    }

    public void RemoveShip(Ship ship)
    {
        ships.Remove(ship);
    }

    public void AddPort(Port port)
    {
        if (!ports.Contains(port))
        {
            ports.Add(port);
        }
    }

    public void RemovePort(Port port)
    {
        ports.Remove(port);
    }
}