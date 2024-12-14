using UnityEngine;

public interface IFactionMember
{
    /// <summary>
    /// The current faction this entity belongs to
    /// </summary>
    FactionType Faction { get; }

    /// <summary>
    /// Change the faction of this entity
    /// </summary>
    /// <param name="newFaction">The new faction to assign</param>
    /// <returns>True if faction change was successful</returns>
    bool SetFaction(FactionType newFaction);

    /// <summary>
    /// Gets the color associated with this entity's faction
    /// </summary>
    Color FactionColor { get; }

    /// <summary>
    /// Event triggered when this entity's faction changes
    /// </summary>
    event System.Action<FactionType> OnFactionChanged;
}
