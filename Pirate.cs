using UnityEngine;

public class Pirate : MonoBehaviour
{
    public FactionType Faction { get; protected set; }
    
    public virtual void SetFaction(FactionType faction)
    {
        Faction = faction;
    }

    // Add other pirate-specific functionality here
}
