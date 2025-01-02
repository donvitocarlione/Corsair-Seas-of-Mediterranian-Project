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
     private Dictionary<FactionType, IEntityOwner> factionLeaders = new Dictionary<FactionType, IEntityOwner>();
    [SerializeField] private GameObject piratePrefab;



    public static FactionManager Instance { get; private set; }
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

         // Initialize faction data
        InitializeFactionData();
          CreateFactionLeaders();
          Debug.Log("[FactionManager] Faction data initialized.");


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
            if (!factionData.ContainsKey(definition.Type))
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


     private void CreateFactionLeaders()
    {
        foreach (FactionType faction in Enum.GetValues(typeof(FactionType)))
        {
            if (faction == FactionType.None) continue;

            var leader = CreateFactionLeader(faction);
             if (leader != null)
            {
                factionLeaders[faction] = leader;
                 Debug.Log($"[FactionManager] Created leader for {faction}");
            }
        }
    }


    private IEntityOwner CreateFactionLeader(FactionType faction)
    {
         if (factionLeaders.ContainsKey(faction))
            return factionLeaders[faction];

         if (piratePrefab == null)
         {
             Debug.LogError("[FactionManager] No pirate prefab assigned!");
             return null;
         }

        GameObject pirateObject = Instantiate(piratePrefab, Vector3.zero, Quaternion.identity);
        Pirate newPirate = pirateObject.GetComponent<Pirate>();

         // Initialize the pirate directly
         newPirate.SetFaction(faction);
           SetFactionLeader(faction, newPirate);

         Debug.Log($"[FactionManager] Created new leader for {faction}");
        return newPirate;

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

        if (factionLeaders.TryGetValue(faction, out IEntityOwner currentLeader))
        {
           if(currentLeader is Pirate oldPirate)
             {
                 Debug.Log($"[FactionManager] Previous leader found {oldPirate.name}, setting rank to Captain.");
               oldPirate.SetRank(PirateRank.Captain);
           }

        }
        Debug.Log($"[FactionManager] Setting {pirate.name} as leader of {faction}");
        factionLeaders[faction] = pirate;
        pirate.SetRank(PirateRank.FactionLeader);
        Debug.Log($"[FactionManager] Set pirate {pirate.name} as the faction leader of {faction}");
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
            // EventSystem.Publish(capturingFaction, capturedPort, FactionChangeType.PortCaptured);

            // Update relations and influence
            if (oldFaction != FactionType.None)
            {
                float currentRelation = GetRelationBetweenFactions(oldFaction, capturingFaction);
              //  UpdateFactionRelation(oldFaction, capturingFaction, currentRelation - _configuration.captureRelationPenalty);
              ModifyFactionInfluence(oldFaction, -_configuration.captureInfluenceChange);
              ModifyFactionInfluence(capturingFaction, _configuration.captureInfluenceChange);
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
               // EventSystem.Publish(faction, factionDefinition.Influence, FactionChangeType.InfluenceChanged);
                 Debug.Log($"Updated {faction} influence from {oldInfluence} to {factionDefinition.Influence}");
            }
        }
        else
        {
             Debug.LogWarning($"Attempting to modify influence of unknown faction: {faction}");
        }
    }
    

    public IEntityOwner GetFactionOwner(FactionType factionType)
    {
        Debug.Log($"[FactionManager] GetFactionOwner called for {factionType}.");
        // First try to get the faction leader
         if (factionLeaders.TryGetValue(factionType, out IEntityOwner leader))
        {
             Debug.Log($"[FactionManager] Found leader for {factionType}: {leader.GetType().Name}");
            return leader;
        }


        // If no leader exists, try to get the first pirate of that faction
       // if (factionPirates.TryGetValue(factionType, out var pirates) && pirates.Count > 0)
       // {
        //     Debug.Log($"[FactionManager] Found first pirate for {factionType}: {pirates[0].name}");
        //     return pirates[0];
       // }
        Debug.LogWarning($"No Pirate owner found for faction {factionType}. Creating a new one...");

        // Create a new pirate for the faction if none exists
         var owner =  CreateFactionLeader(factionType);
        factionLeaders[factionType] = owner;
           Debug.Log($"[FactionManager] New pirate created for {factionType}: {owner.GetType().Name}");
        return owner;
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