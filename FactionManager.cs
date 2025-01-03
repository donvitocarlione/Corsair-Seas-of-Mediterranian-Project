using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[DefaultExecutionOrder(-1)]
public class FactionManager : MonoBehaviour
{
    [SerializeField] private FactionDefinitionAsset[] factionDefinitions;
    private Dictionary<FactionType, FactionDefinition> factionData;
    
    public static FactionManager Instance { get; private set; }
    [SerializeField] protected FactionConfiguration _configuration;
    public FactionConfiguration configuration => _configuration;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeManager()
    {
        factionData = new Dictionary<FactionType, FactionDefinition>();
        InitializeFactionData();
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
            if (asset == null) continue;
            
            var definition = asset.GetFactionData();
            if (!factionData.ContainsKey(definition.Type))
            {
                factionData.Add(definition.Type, definition);
            }
        }
    }

    public bool IsFactionInitialized(FactionType faction)
    {
        return factionData != null && factionData.ContainsKey(faction);
    }

    public FactionDefinition GetFactionData(FactionType faction)
    {
        return IsFactionInitialized(faction) ? factionData[faction] : null;
    }

    public void HandlePortCapture(FactionType capturingFaction, Port capturedPort)
    {
        if (capturedPort == null) return;

        var oldFaction = capturedPort.OwningFaction;
        var oldFactionData = GetFactionData(oldFaction);
        var newFactionData = GetFactionData(capturingFaction);

        oldFactionData?.RemovePort(capturedPort);
        
        if (newFactionData != null)
        {
            newFactionData.AddPort(capturedPort);
            capturedPort.SetFaction(capturingFaction);
        }
    }

    public Color GetFactionColor(FactionType faction)
    {
        var factionData = GetFactionData(faction);
        return factionData?.Color ?? Color.gray;
    }

    public IReadOnlyList<Port> GetFactionPorts(FactionType faction)
    {
        var factionData = GetFactionData(faction);
        return factionData?.Ports ?? new List<Port>().AsReadOnly();
    }
}