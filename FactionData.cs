using UnityEngine;
using System;
using System.Collections.Generic;

public class FactionData
{
    // Core properties
    public FactionType Type { get; private set; }
    public string Name { get; private set; }
    public Color Color { get; set; }
    public float Influence { get; private set; }
    
    // Relationships
    private Dictionary<FactionType, float> relationships;
    
    // Collections
    private List<Ship> ships;
    private List<Port> ports;
    private List<Pirate> members;
    
    // Read-only access to collections
    public IReadOnlyList<Ship> Ships => ships.AsReadOnly();
    public IReadOnlyList<Port> Ports => ports.AsReadOnly();
    public IReadOnlyList<Pirate> Members => members.AsReadOnly();
    
    public FactionData(FactionType type, string name)
    {
        if (string.IsNullOrEmpty(name) || 
            name.Length < FactionConstants.MIN_FACTION_NAME_LENGTH || 
            name.Length > FactionConstants.MAX_FACTION_NAME_LENGTH)
        {
            throw new ArgumentException(string.Format(
                FactionConstants.ERROR_INVALID_NAME_LENGTH,
                FactionConstants.MIN_FACTION_NAME_LENGTH,
                FactionConstants.MAX_FACTION_NAME_LENGTH));
        }

        Type = type;
        Name = name;
        Influence = FactionConstants.DEFAULT_INFLUENCE;
        
        // Initialize collections
        relationships = new Dictionary<FactionType, float>();
        ships = new List<Ship>();
        ports = new List<Port>();
        members = new List<Pirate>();
    }
    
    // Relationship methods
    public float GetRelationship(FactionType other)
    {
        if (relationships.TryGetValue(other, out float value))
        {
            return value;
        }
        return FactionConstants.DEFAULT_STARTING_RELATIONSHIP;
    }
    
    internal void SetRelationship(FactionType other, float value)
    {
        if (other == Type)
        {
            throw new ArgumentException(FactionConstants.ERROR_SAME_FACTION);
        }
        
        value = Mathf.Clamp(value, FactionConstants.MIN_RELATIONSHIP, FactionConstants.MAX_RELATIONSHIP);
        relationships[other] = value;
    }
    
    // Member management (internal use only - should be managed through FactionManager)
    internal void AddMember(Pirate pirate)
    {
        if (!members.Contains(pirate))
        {
            members.Add(pirate);
        }
    }
    
    internal void RemoveMember(Pirate pirate)
    {
        members.Remove(pirate);
    }
    
    internal void SetInfluence(float value)
    {
        Influence = Mathf.Clamp(value, 0f, FactionConstants.DEFAULT_INFLUENCE);
    }
}