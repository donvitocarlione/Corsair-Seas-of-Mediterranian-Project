using UnityEngine;
using System;

public class Pirate : MonoBehaviour
{
    public FactionType CurrentFaction { get; private set; } = FactionType.None;
    
    public event Action<FactionType, FactionType> OnFactionChanged;
    
    private void SetFaction(FactionType newFaction)
    {
        var oldFaction = CurrentFaction;
        CurrentFaction = newFaction;
        OnFactionChanged?.Invoke(oldFaction, newFaction);
    }
    
    // Internal method to be called only by FactionManager
    internal void InternalSetFaction(FactionType faction)
    {
        SetFaction(faction);
    }
}