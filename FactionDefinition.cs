using UnityEngine;
using System.Collections.Generic;
using CSM.Base;

// Since we don't want to create MonoBehaviours with 'new', make this a regular class
public class FactionDefinition
{
    // Basic properties - using read-only pattern for immutable data
    public FactionType Type { get; }
    public string Name { get; }
    
    // Properties that can be modified internally
    public int Influence { get; internal set; }
    public int ResourceLevel { get; internal set; }
    public Color Color { get; internal set; }
    public string BaseLocation { get; internal set; }

    // Collections for relationships and owned entities
    private readonly Dictionary<FactionType, float> relations = new();
    private readonly List<Port> ports = new();
    private readonly List<Pirate> pirates = new();
    private readonly List<Ship> ownedShips = new();

    // Public read-only access to collections
    public IReadOnlyDictionary<FactionType, float> Relations => relations;
    public IReadOnlyList<Port> Ports => ports.AsReadOnly();
    public IReadOnlyList<Pirate> Pirates => pirates.AsReadOnly();
    public IReadOnlyList<Ship> Ships => ownedShips.AsReadOnly();

    // Constructor that initializes immutable properties
    public FactionDefinition(FactionType type, string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new System.ArgumentException("Faction name cannot be null or empty", nameof(name));

        Type = type;
        Name = name;
        
        // Initialize default values
        Influence = 0;
        ResourceLevel = 0;
        Color = Color.white;
    }

    // Relationship management methods
    internal void SetRelation(FactionType otherFaction, float value)
    {
        if (otherFaction == Type)
            throw new System.ArgumentException("Cannot set relation with self", nameof(otherFaction));

        relations[otherFaction] = Mathf.Clamp(value, 0f, 100f);
    }

    internal float GetRelation(FactionType otherFaction)
    {
        if (otherFaction == Type)
            return 100f; // A faction always has perfect relations with itself

        return relations.TryGetValue(otherFaction, out float value) ? value : 50f;
    }

    // Ship management methods
    internal void AddShip(Ship ship)
    {
        if (ship == null) 
            throw new System.ArgumentNullException(nameof(ship));
        
        if (!ownedShips.Contains(ship))
        {
            ownedShips.Add(ship);
            Debug.Log($"[FactionDefinition] Added ship {ship.ShipName()} to faction {Name}");
        }
    }

    internal void RemoveShip(Ship ship)
    {
        if (ship == null) 
            throw new System.ArgumentNullException(nameof(ship));

        if (ownedShips.Remove(ship))
        {
            Debug.Log($"[FactionDefinition] Removed ship {ship.ShipName()} from faction {Name}");
        }
    }

    // Port management methods
    internal void AddPort(Port port)
    {
        if (port == null) 
            throw new System.ArgumentNullException(nameof(port));
        
        if (!ports.Contains(port))
        {
            ports.Add(port);
            Debug.Log($"[FactionDefinition] Added port {port.name} to faction {Name}");
        }
    }

    internal void RemovePort(Port port)
    {
        if (port == null) 
            throw new System.ArgumentNullException(nameof(port));

        if (ports.Remove(port))
        {
            Debug.Log($"[FactionDefinition] Removed port {port.name} from faction {Name}");
        }
    }
    
    // Pirate management methods
    internal void AddPirate(Pirate pirate)
    {
        if (pirate == null) 
            throw new System.ArgumentNullException(nameof(pirate));
        
        if (!pirates.Contains(pirate))
        {
            pirates.Add(pirate);
            Debug.Log($"[FactionDefinition] Added pirate {pirate.name} to faction {Name}");
        }
    }

    internal void RemovePirate(Pirate pirate)
    {
        if (pirate == null) 
            throw new System.ArgumentNullException(nameof(pirate));

        if (pirates.Remove(pirate))
        {
            Debug.Log($"[FactionDefinition] Removed pirate {pirate.name} from faction {Name}");
        }
    }
}