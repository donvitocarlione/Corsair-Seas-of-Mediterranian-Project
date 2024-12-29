using System;

public class FactionEventSystem
{
    public delegate void FactionChangeHandler(FactionType faction, object data, FactionChangeType changeType);
    public event FactionChangeHandler OnFactionChanged;

    public void Publish(FactionType faction, object data, FactionChangeType changeType)
    {
        OnFactionChanged?.Invoke(faction, data, changeType);
    }
}