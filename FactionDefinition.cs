using UnityEngine;
using System.Collections.Generic;

public class FactionDefinition
{
    public FactionType Type { get; }
    public string Name { get; }
    
    // Controlled properties with internal setters
    public int Influence { get; internal set; }
    public int ResourceLevel { get; internal set; }
    public Color Color { get; internal set; }
    public string BaseLocation { get; internal set; }

    private Dictionary<FactionType, float> relations = new();
    private List<Ship> ships = new();
    private List<Port> ports = new();
    private List<Pirate> pirates = new();

    public IReadOnlyDictionary<FactionType, float> Relations => relations;
    public IReadOnlyList<Ship> Ships => ships.AsReadOnly();
    public IReadOnlyList<Port> Ports => ports.AsReadOnly();
    public IReadOnlyList<Pirate> Pirates => pirates.AsReadOnly();

    public FactionDefinition(FactionType type, string name)
    {
        Type = type;
        Name = name;
    }

    internal void SetRelation(FactionType otherFaction, float value)
    {
        relations[otherFaction] = value;
    }

    internal float GetRelation(FactionType otherFaction)
    {
        return relations.TryGetValue(otherFaction, out float value) ? value : 50f;
    }

    internal void AddShip(Ship ship)
    {
        if (ship == null) throw new System.ArgumentNullException(nameof(ship));
        
        if (!ships.Contains(ship))
        {
            ships.Add(ship);
        }
    }

    internal void RemoveShip(Ship ship)
    {
        if (ship == null) throw new System.ArgumentNullException(nameof(ship));
        ships.Remove(ship);
    }

    internal void AddPort(Port port)
    {
        if (port == null) throw new System.ArgumentNullException(nameof(port));
        
        if (!ports.Contains(port))
        {
            ports.Add(port);
        }
    }

    internal void RemovePort(Port port)
    {
        if (port == null) throw new System.ArgumentNullException(nameof(port));
        ports.Remove(port);
    }
    
    internal void AddPirate(Pirate pirate)
    {
        if (pirate == null) throw new System.ArgumentNullException(nameof(pirate));
        
        if (!pirates.Contains(pirate))
        {
            pirates.Add(pirate);
        }
    }

    internal void RemovePirate(Pirate pirate)
    {
        if (pirate == null) throw new System.ArgumentNullException(nameof(pirate));
        pirates.Remove(pirate);
    }
}