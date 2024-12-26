using UnityEngine;
using System.Collections.Generic;
using System;

[AddComponentMenu("Game/Faction Manager")]
public class FactionManager : MonoBehaviour
{
    private static FactionManager instance;
    public static FactionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<FactionManager>();
                if (instance == null)
                {
                    Debug.LogError("No FactionManager found in scene!");
                }
            }
            return instance;
        }
    }

    private Dictionary<FactionType, FactionDefinition> factions = new();
    
    // Events
    public event Action<FactionType, Ship> OnShipRegistered;
    public event Action<FactionType, Ship> OnShipUnregistered;
    public event Action<FactionType, FactionType, float> OnRelationChanged;
    public event Action<FactionType, int> OnInfluenceChanged;
    public event Action<FactionType, Port> OnPortCaptured;

    // Constants
    private const float MIN_RELATION = 0f;
    private const float MAX_RELATION = 100f;
    private const float NEUTRAL_RELATION = 50f;
    private const float WAR_THRESHOLD = 25f;
    private const float ALLY_THRESHOLD = 75f;
    private const float TRADE_RELATION_MULTIPLIER = 0.1f;

    private void Awake()
    {
        ValidateSingleton();
        InitializeFactions();
    }

    private void ValidateSingleton()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"Multiple FactionManager instances found! Destroying duplicate on {gameObject.name}");
            Destroy(this);
            return;
        }
        instance = this;
    }

    private void InitializeFactions()
    {
        foreach (FactionType faction in Enum.GetValues(typeof(FactionType)))
        {
            if (!factions.ContainsKey(faction))
            {
                InitializeDefaultFaction(faction);
            }
        }
    }

    private void InitializeDefaultFaction(FactionType faction)
    {
        var newFaction = new FactionDefinition(
            faction,
            faction.ToString()
        )
        {
            Influence = 50,
            ResourceLevel = 50,
            Color = GetDefaultFactionColor(faction),
            BaseLocation = "Unknown"
        };

        factions[faction] = newFaction;
        InitializeFactionRelations(newFaction);
        Debug.Log($"Initialized default faction: {faction}");
    }

    private Color GetDefaultFactionColor(FactionType faction)
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

    private void InitializeHistoricalFaction(
        FactionType type,
        string name,
        string baseLocation,
        int influence,
        int resourceLevel,
        Color color)
    {
        if (factions.ContainsKey(type))
        {
            Debug.LogWarning($"Faction {type} already exists! Skipping initialization.");
            return;
        }

        var faction = new FactionDefinition(type, name)
        {
            BaseLocation = baseLocation,
            Influence = Mathf.Clamp(influence, 0, 100),
            ResourceLevel = Mathf.Clamp(resourceLevel, 0, 100),
            Color = color
        };

        factions[type] = faction;
        InitializeFactionRelations(faction);
        Debug.Log($"Initialized historical faction: {name} ({type})");
    }

    private void InitializeFactionRelations(FactionDefinition faction)
    {
        foreach (FactionType otherFaction in Enum.GetValues(typeof(FactionType)))
        {
            if (faction.Type != otherFaction)
            {
                faction.SetRelation(otherFaction, NEUTRAL_RELATION);
            }
        }
    }

    public void RegisterShip(FactionType faction, Ship ship)
    {
        if (ship == null)
        {
            Debug.LogError("Attempting to register null ship!");
            return;
        }

        if (factions.TryGetValue(faction, out FactionDefinition factionData))
        {
            factionData.AddShip(ship);
            OnShipRegistered?.Invoke(faction, ship);
            Debug.Log($"Registered ship {ship.ShipName} to faction {faction}");
        }
        else
        {
            Debug.LogError($"Attempting to register ship for unknown faction: {faction}");
        }
    }

    public void UnregisterShip(FactionType faction, Ship ship)
    {
        if (ship == null)
        {
            Debug.LogError("Attempting to unregister null ship!");
            return;
        }

        if (factions.TryGetValue(faction, out FactionDefinition factionData))
        {
            factionData.RemoveShip(ship);
            OnShipUnregistered?.Invoke(faction, ship);
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
            Debug.LogWarning("Cannot update relation between a faction and itself!");
            return;
        }

        var faction1Data = GetFactionData(faction1);
        var faction2Data = GetFactionData(faction2);

        if (faction1Data != null && faction2Data != null)
        {
            float clampedValue = Mathf.Clamp(newValue, MIN_RELATION, MAX_RELATION);
            
            faction1Data.SetRelation(faction2, clampedValue);
            faction2Data.SetRelation(faction1, clampedValue);
            
            OnRelationChanged?.Invoke(faction1, faction2, clampedValue);
            
            // Log significant relation changes
            if (clampedValue <= WAR_THRESHOLD)
            {
                Debug.Log($"War conditions between {faction1} and {faction2} (Relation: {clampedValue})");
            }
            else if (clampedValue >= ALLY_THRESHOLD)
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
                OnInfluenceChanged?.Invoke(faction, factionData.Influence);
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
        if (capturedPort == null) return;

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
            OnPortCaptured?.Invoke(capturingFaction, capturedPort);
            
            // Update relations and influence
            if (oldFaction != FactionType.None)
            {
                float currentRelation = GetRelationBetweenFactions(oldFaction, capturingFaction);
                UpdateFactionRelation(oldFaction, capturingFaction, currentRelation - 20f);
                ModifyFactionInfluence(oldFaction, -10);
                ModifyFactionInfluence(capturingFaction, 10);
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
            float relationBonus = value * TRADE_RELATION_MULTIPLIER;
            float currentRelation = faction1Data.GetRelation(faction2);
            float newRelation = Mathf.Min(currentRelation + relationBonus, MAX_RELATION);

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
        Debug.LogError($"Attempting to get data for unknown faction: {faction}");
        return null;
    }

    public bool AreFactionsAtWar(FactionType faction1, FactionType faction2)
    {
        if (faction1 == faction2) return false;

        var faction1Data = GetFactionData(faction1);
        if (faction1Data == null) return false;

        return faction1Data.GetRelation(faction2) < WAR_THRESHOLD;
    }

    public bool AreFactionsAllied(FactionType faction1, FactionType faction2)
    {
        if (faction1 == faction2) return true;

        var faction1Data = GetFactionData(faction1);
        if (faction1Data == null) return false;

        return faction1Data.GetRelation(faction2) >= ALLY_THRESHOLD;
    }

    public float GetRelationBetweenFactions(FactionType faction1, FactionType faction2)
    {
        if (faction1 == faction2) return MAX_RELATION;

        var faction1Data = GetFactionData(faction1);
        return faction1Data?.GetRelation(faction2) ?? NEUTRAL_RELATION;
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

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}