using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[DefaultExecutionOrder(-1)]
[AddComponentMenu("Game/Faction Manager")]
public class FactionManager : MonoBehaviour
{
    [SerializeField] private FactionDefinitionAsset[] factionDefinitions;
    private Dictionary<FactionType, FactionDefinition> factionData;
    private Dictionary<FactionType, List<Ship>> factionShips;
    private Dictionary<FactionType, List<Pirate>> factionPirates;
    private Dictionary<FactionType, Pirate> factionLeaders = new();


    public static FactionManager Instance { get; private set; }
    public FactionEventSystem EventSystem { get; protected set; }
    [SerializeField] protected FactionConfiguration _configuration;
    public FactionConfiguration configuration => _configuration;


    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            EventSystem = new FactionEventSystem();
            InitializeManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeManager()
    {
        // Initialize collections
        factionData = new Dictionary<FactionType, FactionDefinition>();
        factionShips = new Dictionary<FactionType, List<Ship>>();
        factionPirates = new Dictionary<FactionType, List<Pirate>>();
         // Initialize faction data
        InitializeFactionData();
       

        // Initialize empty lists for each faction
        foreach (FactionType faction in Enum.GetValues(typeof(FactionType)))
        {
             if (!factionShips.ContainsKey(faction))
            {
              factionShips[faction] = new List<Ship>();
            }
               if (!factionPirates.ContainsKey(faction))
            {
                factionPirates[faction] = new List<Pirate>();
            }
        }
    }

    private void InitializeFactionData()
    {
        if (factionDefinitions == null || factionDefinitions.Length == 0)
        {
            Debug.LogError("[FactionManager] No faction definitions assigned!");
            return;
        }

        foreach (var asset in factionDefinitions)
        {
            if (asset == null || asset.factionDefinition == null)
            {
                Debug.LogError("[FactionManager] Null faction definition found!");
                continue;
            }

            var definition = asset.factionDefinition;
            if (!factionData.ContainsKey(definition.FactionType))
            {
                factionData.Add(definition.FactionType, definition);
                Debug.Log($"[FactionManager] Initialized faction: {definition.FactionType}");
            }
            else
            {
                Debug.LogWarning($"[FactionManager] Duplicate faction type found: {definition.FactionType}");
            }
        }

        // Log initialized factions
        var initializedFactions = string.Join(", ", factionData.Keys);
        Debug.Log($"[FactionManager] Initialized factions: {initializedFactions}");
    }

    public bool IsFactionInitialized(FactionType faction)
    {
        return factionData != null && factionData.ContainsKey(faction);
    }

    public FactionDefinition GetFactionData(FactionType faction)
    {
        if (!IsFactionInitialized(faction))
        {
            Debug.LogError($"[FactionManager] Attempting to get data for uninitialized faction: {faction}");
            return null;
        }
        return factionData[faction];
    }


   public void SetFactionLeader(FactionType faction, Pirate pirate)
    {
        if (faction == FactionType.None || faction == FactionType.Independent)
            return;

        if (factionLeaders.TryGetValue(faction, out Pirate currentLeader))
        {
            currentLeader.SetRank(PirateRank.Captain);
        }

        factionLeaders[faction] = pirate;
        pirate.SetRank(PirateRank.FactionLeader);
        Debug.Log($"Set pirate {pirate.name} as the faction leader of {faction}");
    }

      public void RegisterPirate(FactionType faction, Pirate pirate)
    {
        if (pirate == null)
            throw new ArgumentNullException(nameof(pirate));

        if (!factionData.ContainsKey(faction))
        {
             Debug.LogWarning($"Faction {faction} not initialized.");
            return;
        }

        if (!factionPirates.ContainsKey(faction))
            factionPirates[faction] = new List<Pirate>();

        if (!factionPirates[faction].Contains(pirate))
        {
            factionPirates[faction].Add(pirate);
            foreach (var ship in pirate.GetOwnedShips())
            {
                RegisterShip(faction, ship);
            }
            EventSystem.Publish(faction, pirate, FactionChangeType.PirateRegistered);
            Debug.Log($"Registered pirate {pirate.name} to faction {faction}");
        }
        else
        {
            Debug.LogWarning($"Attempting to register an already registered pirate {pirate.name} to faction {faction}");
        }
    }


    public void UnregisterPirate(FactionType faction, Pirate pirate)
    {
        if (pirate == null)
        {
            throw new ArgumentNullException(nameof(pirate));
        }

        if (factionPirates.TryGetValue(faction, out List<Pirate> pirates))
        {
            if (pirates.Remove(pirate))
            {
                // Unregister all ships owned by the pirate
                foreach (var ship in pirate.GetOwnedShips())
                {
                    UnregisterShip(faction, ship);
                }
                EventSystem.Publish(faction, pirate, FactionChangeType.PirateUnregistered);
                Debug.Log($"Unregistered pirate {pirate.name} from faction {faction}");
            }
            else
            {
                Debug.LogWarning($"Attempting to unregister a pirate {pirate.name} that does not exist on faction {faction}");
            }
        }
        else
        {
            Debug.LogWarning($"Attempting to unregister pirate from unknown faction: {faction}");
        }
    }


     public void RegisterShip(FactionType faction, Ship ship)
    {
         if (ship == null)
            throw new ArgumentNullException(nameof(ship));


        if (!factionData.ContainsKey(faction))
        {
             Debug.LogWarning($"Faction {faction} not initialized, cannot register ship.");
            return;
        }


        if (ship.Faction != faction)
        {
            Debug.LogError($"Ship {ship.ShipName()} faction mismatch during registration. Expected {faction}, but ship has {ship.Faction}");
            return;
        }

         if (!factionShips.ContainsKey(faction))
         {
             Debug.LogError($"Faction {faction} does not have a valid list of ships");
            return;
         }
        factionShips[faction].Add(ship);
         EventSystem.Publish(faction, ship, FactionChangeType.ShipRegistered);
       Debug.Log($"Ship {ship.ShipName()} registered with faction {faction}");
    }

    public void UnregisterShip(FactionType faction, Ship ship)
    {
        if (ship == null)
        {
            throw new ArgumentNullException(nameof(ship));
        }

        if (factionShips.TryGetValue(faction, out List<Ship> ships))
        {
            if(ships.Remove(ship))
            {
                EventSystem.Publish(faction, ship, FactionChangeType.ShipUnregistered);
                Debug.Log($"Unregistered ship {ship.ShipName()} from faction {faction}");
            }
           else
           {
                Debug.LogWarning($"Attempting to unregister ship {ship.ShipName()} that does not exist in faction {faction}");
           }
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
            float clampedValue = Mathf.Clamp(newValue, _configuration.minRelation, _configuration.maxRelation);

            faction1Data.SetRelation(faction2, clampedValue);
            faction2Data.SetRelation(faction1, clampedValue);

            EventSystem.Publish(faction1, clampedValue, FactionChangeType.RelationChanged);

            // Log significant relation changes
            if (clampedValue <= _configuration.warThreshold)
            {
                Debug.Log($"War conditions between {faction1} and {faction2} (Relation: {clampedValue})");
            }
            else if (clampedValue >= _configuration.allyThreshold)
            {
                Debug.Log($"Alliance formed between {faction1} and {faction2} (Relation: {clampedValue})");
            }
        }
    }


    public void ModifyFactionInfluence(FactionType faction, int change)
    {
        if (change == 0) return;

        if (factionData.TryGetValue(faction, out FactionDefinition factionDefinition))
        {
            int oldInfluence = factionDefinition.Influence;
            factionDefinition.Influence = Mathf.Clamp(factionDefinition.Influence + change, 0, 100);

            if (oldInfluence != factionDefinition.Influence)
            {
                EventSystem.Publish(faction, factionDefinition.Influence, FactionChangeType.InfluenceChanged);
                Debug.Log($"Updated {faction} influence from {oldInfluence} to {factionDefinition.Influence}");
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
                UpdateFactionRelation(oldFaction, capturingFaction, currentRelation - _configuration.captureRelationPenalty);
                ModifyFactionInfluence(oldFaction, -_configuration.captureInfluenceChange);
                ModifyFactionInfluence(capturingFaction, _configuration.captureInfluenceChange);
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
            float relationBonus = value * _configuration.tradeRelationMultiplier;
            float currentRelation = faction1Data.GetRelation(faction2);
            float newRelation = Mathf.Min(currentRelation + relationBonus, _configuration.maxRelation);

            UpdateFactionRelation(faction1, faction2, newRelation);
            Debug.Log($"Trade between {faction1} and {faction2} improved relations by {relationBonus:F1} points");
        }
    }

      public Pirate GetFactionOwner(FactionType factionType)
    {
        // First try to get the faction leader
        if (factionLeaders.TryGetValue(factionType, out Pirate leader))
        {
            return leader;
        }

        // If no leader exists, try to get the first pirate of that faction
        if (factionPirates.TryGetValue(factionType, out var pirates) && pirates.Count > 0)
        {
            return pirates[0];
        }

        Debug.LogWarning($"No Pirate owner found for faction {factionType}. Creating a new one...");

        // Create a new pirate for the faction if none exists
        GameObject pirateObject = new GameObject($"{factionType}_Leader");
        pirateObject.transform.parent = transform; // Parent the leader
        Pirate newPirate = pirateObject.AddComponent<Pirate>();
        newPirate.SetFaction(factionType);
        SetFactionLeader(factionType, newPirate);
        factionPirates[factionType].Add(newPirate); // ensure new pirate is added to pirates
        return newPirate;
    }

    public bool AreFactionsAtWar(FactionType faction1, FactionType faction2)
    {
        if (faction1 == faction2) return false;

        var faction1Data = GetFactionData(faction1);
        if (faction1Data == null) return false;

        return faction1Data.GetRelation(faction2) < _configuration.warThreshold;
    }

    public bool AreFactionsAllied(FactionType faction1, FactionType faction2)
    {
          if (faction1 == faction2) return true;

        var faction1Data = GetFactionData(faction1);
        if (faction1Data == null) return false;


        return faction1Data.GetRelation(faction2) >= _configuration.allyThreshold;
    }

      public float GetRelationBetweenFactions(FactionType faction1, FactionType faction2)
    {
          if (faction1 == faction2) return _configuration.maxRelation;

        var faction1Data = GetFactionData(faction1);
        return faction1Data?.GetRelation(faction2) ?? _configuration.neutralRelation;
    }


     public IReadOnlyList<Ship> GetFactionShips(FactionType faction)
    {
           if (factionShips.TryGetValue(faction, out List<Ship> ships))
          {
            return ships.AsReadOnly();
           }
            return new List<Ship>().AsReadOnly();
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
            Instance = null;
        }
    }
}