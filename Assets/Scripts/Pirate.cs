public override void SetFaction(FactionType newFaction)
{
    if (!isInitialized || !object.Equals(newFaction, Faction))
    {
        if (isInitialized)
        {
            UnregisterFromFaction();
        }

        base.SetFaction(newFaction);
        RegisterWithFaction();
        isInitialized = true;
        HandleFactionChanged(newFaction);
    }
}