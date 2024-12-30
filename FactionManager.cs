using UnityEngine;
using System.Collections.Generic;
using System;

[AddComponentMenu("Game/Faction Manager")]
public class FactionManager : MonoBehaviour
{
    public static FactionManager Instance { get; protected set; }

    [SerializeField] protected FactionConfiguration _configuration;
    public FactionConfiguration configuration => _configuration;
    
    [SerializeField] protected List<FactionDefinitionAsset> factionDefinitions;
    
    public FactionEventSystem EventSystem { get; protected set; }
    
    protected Dictionary<FactionType, FactionDefinition> factions = new();
    private Dictionary<FactionType, HashSet<Faction>> factionEntities = new();

    protected void Awake()
    {
        ValidateSingleton();
        EventSystem = new FactionEventSystem();
        InitializeFactions();
    }

    protected void ValidateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Multiple FactionManager instances found! Destroying duplicate on {gameObject.name}");
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void RegisterFactionEntity(Faction entity)
    {
        if (entity == null)
            throw new System.ArgumentNullException(nameof(entity));

        if (!factionEntities.ContainsKey(entity.Type))
        {
            factionEntities[entity.Type] = new HashSet<Faction>();
        }

        factionEntities[entity.Type].Add(entity);
        EventSystem.Publish(entity.Type, entity, FactionChangeType.EntityRegistered);
        Debug.Log($"Registered faction entity: {entity.name} with {entity.Type}");
    }

    public void UnregisterFactionEntity(Faction entity)
    {
        if (entity == null)
            throw new System.ArgumentNullException(nameof(entity));

        if (factionEntities.ContainsKey(entity.Type))
        {
            factionEntities[entity.Type].Remove(entity);
            EventSystem.Publish(entity.Type, entity, FactionChangeType.EntityUnregistered);
            Debug.Log($"Unregistered faction entity: {entity.name} from {entity.Type}");
        }
    }

    public IReadOnlyCollection<Faction> GetFactionEntities(FactionType type)
    {
        if (factionEntities.TryGetValue(type, out var entities))
        {
            return entities;
        }
        return new HashSet<Faction>();
    }

    protected void InitializeFactions()
    {
        if (factionDefinitions == null || factionDefinitions.Count == 0)
        {
            Debug.LogWarning("No Faction definitions found! Initializing default factions");
            foreach (FactionType faction in Enum.GetValues(typeof(FactionType)))
            {
                if (!factions.ContainsKey(faction))
                {
                    InitializeDefaultFaction(faction);
                }
            }
        }
        else
        {
            foreach (var definition in factionDefinitions)
            {
                InitializeFactionFromAsset(definition);
            }
        }
    }

    public void RegisterPirate(FactionType faction, Pirate pirate)
    {
        if (pirate == null)
        {
            throw new ArgumentNullException(nameof(pirate));
        }

        if (factions.TryGetValue(faction, out FactionDefinition factionData))
        {
            // Register all ships owned by the pirate
            foreach (var ship in pirate.GetOwnedShips())
            {
                RegisterShip(faction, ship);
            }
            EventSystem.Publish(faction, pirate, FactionChangeType.PirateRegistered);
            Debug.Log($"Registered pirate to faction {faction}");
        }
        else
        {
            throw new ArgumentException($"Unknown faction: {faction}", nameof(faction));
        }
    }

    public void UnregisterPirate(FactionType faction, Pirate pirate)
    {
        if (pirate == null)
        {
            throw new ArgumentNullException(nameof(pirate));
        }

        if (factions.TryGetValue(faction, out FactionDefinition factionData))
        {
            // Unregister all ships owned by the pirate
            foreach (var ship in pirate.GetOwnedShips())
            {
                UnregisterShip(faction, ship);
            }
            EventSystem.Publish(faction, pirate, FactionChangeType.PirateUnregistered);
            Debug.Log($"Unregistered pirate from faction {faction}");
        }
        else
        {
            Debug.LogWarning($"Attempting to unregister pirate from unknown faction: {faction}");
        }
    }

    [Rest of the existing FactionManager.cs code remains the same...]

    protected void OnDestroy()
    {
        if (Instance == this)
        {
            // Clear all registered entities
            foreach (var entitySet in factionEntities.Values)
            {
                entitySet.Clear();
            }
            factionEntities.Clear();
            
            Instance = null;
        }
    }
}
