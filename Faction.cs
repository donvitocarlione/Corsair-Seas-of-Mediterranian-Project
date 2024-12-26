using System.Collections.Generic;
using UnityEngine;

public class Faction : MonoBehaviour
{
    public FactionType factionType;
    public string factionName;
    public List<Pirate> members = new List<Pirate>();
    public List<Port> controlledPorts = new List<Port>();

    void Start()
    {
        if (string.IsNullOrEmpty(factionName))
        {
            factionName = factionType.ToString();
        }
    }

    public void AddMember(Pirate pirate)
    {
        if (!members.Contains(pirate))
        {
            members.Add(pirate);
            pirate.SetFaction(factionType);
        }
    }

    public void RemoveMember(Pirate pirate)
    {
        if (members.Contains(pirate))
        {
            members.Remove(pirate);
            pirate.SetFaction(FactionType.None); // Use None instead of null
        }
    }

    public bool IsFriendlyWith(Faction otherFaction)
    {
        return DiplomacySystem.Instance.AreFriendly(factionType, otherFaction.factionType);
    }

    public bool IsHostileWith(Faction otherFaction)
    {
        return DiplomacySystem.Instance.AreHostile(factionType, otherFaction.factionType);
    }
}
