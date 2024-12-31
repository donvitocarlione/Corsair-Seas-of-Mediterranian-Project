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
        Debug.Log("[FactionManager] Awake called.");
        // Singleton setup
        if (Instance == null)
        {
            Debug.Log("[FactionManager] Instance is null, setting up singleton.");
            Instance = this;
            EventSystem = new FactionEventSystem();
            Debug.Log("[FactionManager] Event system created.");
            InitializeManager();
             Debug.Log("[FactionManager] Manager initialized.");

        }
        else
        {
             Debug.LogWarning("[FactionManager] Another instance detected, destroying this game object.");
            Destroy(gameObject);
        }
    }

    private void InitializeManager()
    {
         Debug.Log("[FactionManager] InitializeManager called.");
        // Initialize collections
        factionData = new Dictionary<FactionType, FactionDefinition>();
        Debug.Log("[FactionManager] factionData dictionary initialized.");
        factionShips = new Dictionary<FactionType, List<Ship>>();
         Debug.Log("[FactionManager] factionShips dictionary initialized.");
        factionPirates = new Dictionary<FactionType, List<Pirate>>();
         Debug.Log("[FactionManager] factionPirates dictionary initialized.");

         // Initialize faction data
        InitializeFactionData();
          Debug.Log("[FactionManager] Faction data initialized.");

        // Initialize empty lists for each faction
        foreach (FactionType faction in Enum.GetValues(typeof(FactionType)))
        {
            Debug.Log($"[FactionManager] Initializing lists for faction: {faction}");
             if (!factionShips.ContainsKey(faction))
            {
              factionShips[faction] = new List<Ship>();
               Debug.Log($"[FactionManager] Initialized factionShips list for: {faction}");
            }
               if (!factionPirates.ContainsKey(faction))
            {
                factionPirates[faction] = new List<Pirate>();
                 Debug.Log($"[FactionManager] Initialized factionPirates list for: {faction}");
            }
        }

         ValidateInitializedFactions();
        Debug.Log("[FactionManager] Faction manager initialization complete.");
    }

    private void InitializeFactionData()
    {
          Debug.Log("[FactionManager] InitializeFactionData called.");
        if (factionDefinitions == null || factionDefinitions.Length == 0)
        {
            Debug.LogError("[FactionManager] No faction definitions assigned!");
            return;
        }

        foreach (var asset in factionDefinitions)
        {
            if (asset == null)
            {
                Debug.LogError("[FactionManager] Null faction definition found!");
                continue;
            }

            var definition = asset.GetFactionData();
            if (!factionData.ContainsKey(definition.Type))  // Note: Using Type instead of FactionType
            {
                factionData.Add(definition.Type, definition);
                Debug.Log($"[FactionManager] Initialized faction: {definition.Type}");
            }
            else
            {
                Debug.LogWarning($"[FactionManager] Duplicate faction type found: {definition.Type}");
            }
        }
    }

    private void ValidateInitializedFactions()
    {
        Debug.Log("[FactionManager] ValidateInitializedFactions called.");
        var allFactionTypes = System.Enum.GetValues(typeof(FactionType));
        foreach (FactionType factionType in allFactionTypes)
        {
            if (!factionData.ContainsKey(factionType))
            {
                Debug.LogError($"[FactionManager] Missing definition for faction: {factionType}");
            }
        }

        Debug.Log($"[FactionManager] Initialized factions: {string.Join(", ", factionData.Keys)}");
    }

    public bool IsFactionInitialized(FactionType faction)
    {
        bool isInitialized = factionData != null && factionData.ContainsKey(faction);
        Debug.Log($"[FactionManager] IsFactionInitialized for {faction}: {isInitialized}");
        return isInitialized;
    }

    public FactionDefinition GetFactionData(FactionType faction)
    {
        Debug.Log($"[FactionManager] GetFactionData for {faction} called.");
        if (!IsFactionInitialized(faction))
        {
            Debug.LogError($"[FactionManager] Attempting to get data for uninitialized faction: {faction}");
            return null;
        }
          Debug.Log($"[FactionManager] Returning data for faction {faction}.");
        return factionData[faction];
    }


   public void SetFactionLeader(FactionType faction, Pirate pirate)
    {
         Debug.Log($"[FactionManager] SetFactionLeader called for faction {faction}, pirate: {pirate?.name}");
        if (faction == FactionType.None || faction == FactionType.Independent)
        {
             Debug.LogWarning($"[FactionManager] Cannot set leader for faction {faction}");
            return;
        }

        if (factionLeaders.TryGetValue(faction, out Pirate currentLeader))
        {
             Debug.Log($"[FactionManager] Previous leader found {currentLeader.name}, setting rank to Captain.");
            currentLeader.SetRank(PirateRank.Captain);
        }
        Debug.Log($"[FactionManager] Setting {pirate.name} as leader of {faction}");
        factionLeaders[faction] = pirate;
        pirate.SetRank(PirateRank.FactionLeader);
        Debug.Log($"[FactionManager] Set pirate {pirate.name} as the faction leader of {faction}");
    }

      public void RegisterPirate(FactionType faction, Pirate pirate)
    {
        Debug.Log($"[FactionManager] RegisterPirate called for faction {faction}, pirate: {pirate?.name}");
        if (pirate == null)
            throw new ArgumentNullException(nameof(pirate));

        if (!factionData.ContainsKey(faction))
        {
             Debug.LogWarning($"Faction {faction} not initialized.");
            return;
        }
         Debug.Log($"[FactionManager] RegisterPirate. checking for pirates of {faction}.");
        if (!factionPirates.ContainsKey(faction))
            factionPirates[faction] = new List<Pirate>();
        Debug.Log($"[FactionManager] RegisterPirate. checking if {pirate.name} exist in faction {faction}.");
        if (!factionPirates[faction].Contains(pirate))
        {
            factionPirates[faction].Add(pirate);
             Debug.Log($"[FactionManager] RegisterPirate. {pirate.name} added. Checking ships.");
            foreach (var ship in pirate.GetOwnedShips())
            {
                RegisterShip(faction, ship);
            }
            EventSystem.Publish(faction, pirate, FactionChangeType.PirateRegistered);
             Debug.Log($"[FactionManager] Registered pirate {pirate.name} to faction {faction}");
        }
        else
        {
             Debug.LogWarning($"Attempting to register an already registered pirate {pirate.name} to faction {faction}");
        }
    }


    public void UnregisterPirate(FactionType faction, Pirate pirate)
    {
          Debug.Log($"[FactionManager] UnregisterPirate called for faction {faction}, pirate: {pirate?.name}");
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
                Debug.Log($"[FactionManager] Unregistered pirate {pirate.name} from faction {faction}");
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
           Debug.Log($"[FactionManager] RegisterShip called for faction {faction}, ship: {ship?.ShipName()}");
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
          Debug.Log($"[FactionManager] UnregisterShip called for faction {faction}, ship: {ship?.ShipName()}");
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
         Debug.Log($"[FactionManager] UpdateFactionRelation called for {faction1}, {faction2}, value: {newValue}");
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
         Debug.Log($"[FactionManager] ModifyFactionInfluence called for {faction}, change: {change}");
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
           Debug.Log($"[FactionManager] HandlePortCapture called. Capturing faction: {capturingFaction}, captured port: {capturedPort?.name}");
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
          Debug.Log($"[FactionManager] RecordTradeBetweenFactions called. {faction1}, {faction2}, value: {value}");
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
        Debug.Log($"[FactionManager] GetFactionOwner called for {factionType}.");
        // First try to get the faction leader
        if (factionLeaders.TryGetValue(factionType, out Pirate leader))
        {
            Debug.Log($"[FactionManager] Found leader for {factionType}: {leader.name}");
            return leader;
        }

        // If no leader exists, try to get the first pirate of that faction
        if (factionPirates.TryGetValue(factionType, out var pirates) && pirates.Count > 0)
        {
             Debug.Log($"[FactionManager] Found first pirate for {factionType}: {pirates[0].name}");
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
         Debug.Log($"[FactionManager] New pirate created for {factionType}: {newPirate.name}");
        return newPirate;
    }

    public bool AreFactionsAtWar(FactionType faction1, FactionType faction2)
    {
        Debug.Log($"[FactionManager] AreFactionsAtWar called for {faction1}, {faction2}.");
        if (faction1 == faction2) {
             Debug.Log($"[FactionManager] {faction1} and {faction2} are same. Returning false");
            return false;
        }

        var faction1Data = GetFactionData(faction1);
        if (faction1Data == null) {
           Debug.Log($"[FactionManager] {faction1} data not found. Returning false.");
          return false;
        }

        bool atWar = faction1Data.GetRelation(faction2) < _configuration.warThreshold;
        Debug.Log($"[FactionManager] Relation between {faction1} and {faction2} war status: {atWar}");
        return atWar;
    }

    public bool AreFactionsAllied(FactionType faction1, FactionType faction2)
    {
         Debug.Log($"[FactionManager] AreFactionsAllied called for {faction1}, {faction2}.");
          if (faction1 == faction2) {
               Debug.Log($"[FactionManager] {faction1} and {faction2} are same. Returning true");
            return true;
          }


        var faction1Data = GetFactionData(faction1);
        if (faction1Data == null)
        {
            Debug.Log($"[FactionManager] {faction1} data not found. Returning false.");
            return false;
        }


        bool isAllied = faction1Data.GetRelation(faction2) >= _configuration.allyThreshold;
          Debug.Log($"[FactionManager] Relation between {faction1} and {faction2} ally status: {isAllied}");
        return isAllied;
    }

      public float GetRelationBetweenFactions(FactionType faction1, FactionType faction2)
    {
         Debug.Log($"[FactionManager] GetRelationBetweenFactions called for {faction1}, {faction2}.");
          if (faction1 == faction2)
        {
             Debug.Log($"[FactionManager] {faction1} and {faction2} are same, returning {_configuration.maxRelation}.");
            return _configuration.maxRelation;
        }

        var faction1Data = GetFactionData(faction1);
         float relation = faction1Data?.GetRelation(faction2) ?? _configuration.neutralRelation;
          Debug.Log($"[FactionManager] Relation between {faction1} and {faction2} is {relation}.");
        return relation;
    }


     public IReadOnlyList<Ship> GetFactionShips(FactionType faction)
    {
        Debug.Log($"[FactionManager] GetFactionShips called for {faction}.");
           if (factionShips.TryGetValue(faction, out List<Ship> ships))
          {
                Debug.Log($"[FactionManager] Returning {ships.Count} ships for faction {faction}.");
            return ships.AsReadOnly();
           }
             Debug.Log($"[FactionManager] No ships found for faction {faction}. Returning empty list.");
            return new List<Ship>().AsReadOnly();
    }

     public IReadOnlyList<Port> GetFactionPorts(FactionType faction)
    {
        Debug.Log($"[FactionManager] GetFactionPorts called for {faction}.");
        var factionData = GetFactionData(faction);
          if(factionData == null)
           {
              Debug.Log($"[FactionManager] No faction data found for {faction}. Returning empty port list.");
             return new List<Port>().AsReadOnly();
           }

           Debug.Log($"[FactionManager] Returning {factionData.Ports.Count} ports for faction {faction}.");
         return factionData?.Ports ?? new List<Port>().AsReadOnly();
    }


    public Color GetFactionColor(FactionType faction)
    {
        Debug.Log($"[FactionManager] GetFactionColor called for {faction}.");
        var factionData = GetFactionData(faction);
        var color = factionData?.Color ?? Color.gray;
        Debug.Log($"[FactionManager] Color for {faction} is: {color}.");
        return color;
    }



    protected void OnDestroy()
    {
        Debug.Log("[FactionManager] OnDestroy called.");
        if (Instance == this)
        {
            Debug.Log("[FactionManager] Instance is being cleared.");
            Instance = null;
        }
    }
}