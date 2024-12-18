using UnityEngine;
using System;
using System.Collections.Generic;

public class FactionManager : MonoBehaviour
{
    private static FactionManager instance;
    public static FactionManager Instance => instance;
    
    // Core data storage
    private Dictionary<FactionType, FactionData> factionData;
    
    // Events
    public event Action<FactionType, FactionType, float> OnRelationshipChanged;
    public event Action<Pirate, FactionType, FactionType> OnMemberFactionChanged;
    public event Action<Port, FactionType, FactionType> OnPortOwnershipChanged;
    public event Action<FactionType, float> OnInfluenceChanged;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFactionSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeFactionSystem()
    {
        factionData = new Dictionary<FactionType, FactionData>();
        
        // Initialize all factions except None
        foreach (FactionType factionType in Enum.GetValues(typeof(FactionType)))
        {
            if (factionType != FactionType.None)
            {
                CreateFaction(factionType, factionType.ToString());
            }
        }
    }
    
    private void CreateFaction(FactionType type, string name)
    {
        if (factionData.ContainsKey(type))
        {
            Debug.LogWarning($"Faction {type} already exists!");
            return;
        }
        
        try
        {
            var faction = new FactionData(type, name);
            factionData[type] = faction;
        }
        catch (ArgumentException e)
        {
            Debug.LogError($"Failed to create faction {type}: {e.Message}");
        }
    }
    
    // Relationship Management
    public float GetRelationship(FactionType a, FactionType b)
    {
        if (!factionData.ContainsKey(a) || !factionData.ContainsKey(b))
        {
            throw new ArgumentException(FactionConstants.ERROR_INVALID_FACTION);
        }
        
        return factionData[a].GetRelationship(b);
    }
    
    public void SetRelationship(FactionType a, FactionType b, float value)
    {
        if (!factionData.ContainsKey(a) || !factionData.ContainsKey(b))
        {
            throw new ArgumentException(FactionConstants.ERROR_INVALID_FACTION);
        }
        
        // Update both factions' relationship values
        factionData[a].SetRelationship(b, value);
        factionData[b].SetRelationship(a, value);
        
        OnRelationshipChanged?.Invoke(a, b, value);
    }
    
    // Member Management
    public void AddMemberToFaction(Pirate pirate, FactionType faction)
    {
        if (!factionData.ContainsKey(faction))
        {
            throw new ArgumentException(FactionConstants.ERROR_INVALID_FACTION);
        }
        
        var oldFaction = pirate.CurrentFaction;
        
        // Remove from old faction if necessary
        if (oldFaction != FactionType.None && factionData.ContainsKey(oldFaction))
        {
            factionData[oldFaction].RemoveMember(pirate);
        }
        
        // Add to new faction
        factionData[faction].AddMember(pirate);
        pirate.InternalSetFaction(faction);
        
        OnMemberFactionChanged?.Invoke(pirate, oldFaction, faction);
    }
    
    public void RemoveMemberFromFaction(Pirate pirate, FactionType faction)
    {
        if (!factionData.ContainsKey(faction))
        {
            throw new ArgumentException(FactionConstants.ERROR_INVALID_FACTION);
        }
        
        factionData[faction].RemoveMember(pirate);
        pirate.InternalSetFaction(FactionType.None);
        
        OnMemberFactionChanged?.Invoke(pirate, faction, FactionType.None);
    }
    
    // Influence Management
    public void SetFactionInfluence(FactionType faction, float value)
    {
        if (!factionData.ContainsKey(faction))
        {
            throw new ArgumentException(FactionConstants.ERROR_INVALID_FACTION);
        }
        
        factionData[faction].SetInfluence(value);
        OnInfluenceChanged?.Invoke(faction, value);
    }
}