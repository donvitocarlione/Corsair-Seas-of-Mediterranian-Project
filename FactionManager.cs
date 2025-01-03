// FactionManager.cs
using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-1)]
public class FactionManager : MonoBehaviour
{
    public static FactionManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Color GetFactionColor(FactionType faction)
    {
        return Color.gray;
    }

    public IReadOnlyList<Port> GetFactionPorts(FactionType faction)
    {
        return new List<Port>().AsReadOnly();
    }

    public void HandlePortCapture(FactionType capturingFaction, Port capturedPort)
    {
        if (capturedPort == null) return;
        capturedPort.SetFaction(capturingFaction);
    }
}