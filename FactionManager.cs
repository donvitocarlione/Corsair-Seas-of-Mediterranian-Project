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
            throw new ArgumentNullException(nameof(entity));

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
            throw new ArgumentNullException(nameof(entity));

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

   protected void InitializeDefaultFaction(FactionType faction)
    {
        if (!factions.ContainsKey(faction))
        {
            var newFaction = new FactionDefinition(
                faction,
                faction.ToString()
            )
            {
                Influence = (int)configuration.defaultInfluence,
                ResourceLevel = (int)configuration.defaultResourceLevel,
                Color = GetDefaultFactionColor(faction),
                BaseLocation = "Unknown"
            };

            factions[faction] = newFaction;
            InitializeFactionRelations(newFaction);
            Debug.Log($"Initialized default faction: {faction}");
        }
    }


    protected void InitializeFactionFromAsset(FactionDefinitionAsset asset)
    {
        if (factions.ContainsKey(asset.type))
        {
            Debug.LogWarning($"Faction {asset.type} already exists! Skipping initialization.");
            return;
        }

        var newFaction = new FactionDefinition(
            asset.type,
            asset.displayName
        )
        {
            Influence = asset.initialInfluence,
            ResourceLevel = asset.initialResourceLevel,
            Color = asset.color,
            BaseLocation = asset.baseLocation
        };

        factions[asset.type] = newFaction;
        InitializeFactionRelations(newFaction);
        Debug.Log($"Initialized faction from asset: {asset.displayName} ({asset.type})");
    }

    protected Color GetDefaultFactionColor(FactionType faction)
    {
        return faction switch
        {
            FactionType.Pirates => Color.red,
            FactionType.Merchants => Color.green,
            FactionType.RoyalNavy => Color.blue,
            FactionType.Ottomans => Color.yellow,
            FactionType.Venetians => Color.magenta,
            _ => Color.gray
        };
    }

    protected void InitializeFactionRelations(FactionDefinition faction)
    {
        foreach (FactionType otherFaction in Enum.GetValues(typeof(FactionType)))
        {
            if (faction.Type != otherFaction)
            {
                faction.SetRelation(otherFaction, configuration.neutralRelation);
            }
        }
    }

    public void RegisterShip(FactionType faction, Ship ship)
    {
        if (ship == null)
            throw new ArgumentNullException(nameof(ship));

        if (!factions.ContainsKey(faction))
        {
            Debug.LogWarning($"Faction {faction} not initialized. Initializing with default values...");
            InitializeDefaultFaction(faction);
        }
        
        if (ship.Faction != faction)
        {
            Debug.LogError($"Ship {ship.ShipName} faction mismatch during registration. Expected {faction}, but ship has {ship.Faction}. Ensure ship's faction is set correctly before registration.");
            return;
        }
        
        if (factions.TryGetValue(faction, out FactionDefinition factionData))
        {
             factionData.AddShip(ship);
            
            // Register ship as faction entity
            RegisterFactionEntity(ship);
             EventSystem.Publish(faction, ship, FactionChangeType.ShipRegistered);
            Debug.Log($"Ship {ship.ShipName} registered with faction {faction}");
        }
        else
        {
            throw new ArgumentException($"Unknown faction: {faction}", nameof(faction));
        }
    }


    public void UnregisterShip(FactionType faction, Ship ship)
    {
        if (ship == null)
        {
            throw new ArgumentNullException(nameof(ship));
        }

        if (factions.TryGetValue(faction, out FactionDefinition factionData))
        {
            factionData.RemoveShip(ship);
            UnregisterFactionEntity(ship);
            EventSystem.Publish(faction, ship, FactionChangeType.ShipUnregistered);
            Debug.Log($"Unregistered ship {ship.ShipName} from faction {faction}");
        }
        else
        {
            Debug.LogWarning($"Attempting to unregister ship from unknown faction: {faction}");
        }
    }

    public void UpdateFactionRelation(FactionType faction1, FactionType faction2, float newValue)
    {
        if (faction1 == faction2)
        {
            throw new ArgumentException("Cannot update relation between a faction and itself!");
        }

        var faction1Data = GetFactionData(faction1);
        var faction2Data = GetFactionData(faction2);

        if (faction1Data != null && faction2Data != null)
        {
            float clampedValue = Mathf.Clamp(newValue, configuration.minRelation, configuration.maxRelation);
            
            faction1Data.SetRelation(faction2, clampedValue);
            faction2Data.SetRelation(faction1, clampedValue);
            
            EventSystem.Publish(faction1, clampedValue, FactionChangeType.RelationChanged);
            
            // Log significant relation changes
            if (clampedValue <= configuration.warThreshold)
            {
                Debug.Log($"War conditions between {faction1} and {faction2} (Relation: {clampedValue})");
            }
            else if (clampedValue >= configuration.allyThreshold)
            {
                Debug.Log($"Alliance formed between {faction1} and {faction2} (Relation: {clampedValue})");
            }
        }
    }

    public void ModifyFactionInfluence(FactionType faction, int change)
    {
        if (change == 0) return;

        if (factions.TryGetValue(faction, out FactionDefinition factionData))
        {
            int oldInfluence = factionData.Influence;
            factionData.Influence = Mathf.Clamp(factionData.Influence + change, 0, 100);
            
            if (oldInfluence != factionData.Influence)
            {
                EventSystem.Publish(faction, factionData.Influence, FactionChangeType.InfluenceChanged);
                Debug.Log($"Updated {faction} influence from {oldInfluence} to {factionData.Influence}");
            }
        }
        else
        {
            Debug.LogWarning($"Attempting to modify influence of unknown faction: {faction}");
        }
    }

    public void HandlePortCapture(FactionType capturingFaction, Port capturedPort)
    {
        if (capturedPort == null)
        {
            throw new ArgumentNullException(nameof(capturedPort));
        }

        var oldFaction = capturedPort.OwningFaction;
        var oldFactionData = GetFactionData(oldFaction);
        var newFactionData = GetFactionData(capturingFaction);

        if (oldFactionData != null)
        {
            oldFactionData.RemovePort(capturedPort);
        }

        if (newFactionData != null)
        {
            newFactionData.AddPort(capturedPort);
            capturedPort.SetFaction(capturingFaction);
            EventSystem.Publish(capturingFaction, capturedPort, FactionChangeType.PortCaptured);
            
            // Update relations and influence
            if (oldFaction != FactionType.None)
            {
                float currentRelation = GetRelationBetweenFactions(oldFaction, capturingFaction);
                UpdateFactionRelation(oldFaction, capturingFaction, currentRelation - configuration.captureRelationPenalty);
                ModifyFactionInfluence(oldFaction, -configuration.captureInfluenceChange);
                ModifyFactionInfluence(capturingFaction, configuration.captureInfluenceChange);
            }
        }
    }

    public void RecordTradeBetweenFactions(FactionType faction1, FactionType faction2, float value)
    {
        if (faction1 == faction2 || value <= 0)
        {
            return;
        }

        var faction1Data = GetFactionData(faction1);
        var faction2Data = GetFactionData(faction2);

        if (faction1Data != null && faction2Data != null)
        {
            float relationBonus = value * configuration.tradeRelationMultiplier;
            float currentRelation = faction1Data.GetRelation(faction2);
            float newRelation = Mathf.Min(currentRelation + relationBonus, configuration.maxRelation);

            UpdateFactionRelation(faction1, faction2, newRelation);
            Debug.Log($"Trade between {faction1} and {faction2} improved relations by {relationBonus:F1} points");
        }
    }

    public FactionDefinition GetFactionData(FactionType faction)
    {
        if (factions.TryGetValue(faction, out FactionDefinition factionData))
        {
            return factionData;
        }
        throw new ArgumentException($"Unknown faction: {faction}", nameof(faction));
    }

    public bool AreFactionsAtWar(FactionType faction1, FactionType faction2)
    {
        if (faction1 == faction2) return false;

        var faction1Data = GetFactionData(faction1);
        if (faction1Data == null) return false;

        return faction1Data.GetRelation(faction2) < configuration.warThreshold;
    }

    public bool AreFactionsAllied(FactionType faction1, FactionType faction2)
    {
        if (faction1 == faction2) return true;

        var faction1Data = GetFactionData(faction1);
        if (faction1Data == null) return false;

        return faction1Data.GetRelation(faction2) >= configuration.allyThreshold;
    }

    public float GetRelationBetweenFactions(FactionType faction1, FactionType faction2)
    {
        if (faction1 == faction2) return configuration.maxRelation;

        var faction1Data = GetFactionData(faction1);
        return faction1Data?.GetRelation(faction2) ?? configuration.neutralRelation;
    }

    public IReadOnlyList<Ship> GetFactionShips(FactionType faction)
    {
        var factionData = GetFactionData(faction);
        return factionData?.Ships ?? new List<Ship>().AsReadOnly();
    }

    public IReadOnlyList<Port> GetFactionPorts(FactionType faction)
    {
        var factionData = GetFactionData(faction);
        return factionData?.Ports ?? new List<Port>().AsReadOnly();
    }

    public Color GetFactionColor(FactionType faction)
    {
        var factionData = GetFactionData(faction);
        return factionData?.Color ?? Color.gray;
    }

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