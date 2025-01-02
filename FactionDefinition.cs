using UnityEngine;
using System.Collections.Generic;

public class FactionDefinition
{
    // Immutable properties
    public FactionType Type { get; }
    public string Name { get; }
    public Color Color { get; set; }  // Changed to public set

    // Mutable properties
    public int Influence { get; internal set; } // Changed to internal set for FactionManager

    // Relationships
    private readonly Dictionary<FactionType, float> _relations = new();
    public IReadOnlyDictionary<FactionType, float> Relations => _relations;

    // Ports
    private readonly List<Port> _ports = new();
    public IReadOnlyList<Port> Ports => _ports.AsReadOnly();


    public FactionDefinition(FactionType type, string name, Color color)
    {
        Type = type;
        Name = name;
        Color = color;
        Influence = 50; // Default influence
    }


    internal void SetRelation(FactionType otherFaction, float value)
    {
       if (otherFaction == Type)
            throw new System.ArgumentException("Cannot set relation with self", nameof(otherFaction));
        _relations[otherFaction] = Mathf.Clamp(value, 0f, 100f);
    }

    public float GetRelation(FactionType otherFaction)
    {
         if (otherFaction == Type)
            return 100f;

        return _relations.TryGetValue(otherFaction, out float value) ? value : 50f;
    }

    internal void AddPort(Port port)
    {
          if (port == null) 
            throw new System.ArgumentNullException(nameof(port));
        
        if (!_ports.Contains(port))
        {
            _ports.Add(port);
             Debug.Log($"[FactionDefinition] Added port {port.name} to faction {Name}");
        }
    }

     internal void RemovePort(Port port)
    {
        if (port == null) 
            throw new System.ArgumentNullException(nameof(port));

        if (_ports.Remove(port))
        {
              Debug.Log($"[FactionDefinition] Removed port {port.name} from faction {Name}");
        }
    }


}