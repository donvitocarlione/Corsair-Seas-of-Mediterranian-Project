// Interfaces/IOwnable.cs
public interface IOwnable
{
    /// <summary>
    /// The current owner of this entity
    /// </summary>
    IEntityOwner Owner { get; }

    /// <summary>
    /// Change the owner of this entity
    /// </summary>
    /// <param name="newOwner">The new owner to assign</param>
    /// <returns>True if ownership change was successful</returns>
    bool SetOwner(IEntityOwner newOwner);

    /// <summary>
    /// Event triggered when this entity's owner changes
    /// </summary>
    event System.Action<IEntityOwner> OnOwnerChanged;
}