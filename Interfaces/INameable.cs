public interface INameable
{
    /// <summary>
    /// The name of this entity
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Set a new name for this entity
    /// </summary>
    /// <param name="newName">The new name to assign</param>
    /// <returns>True if name change was successful</returns>
    bool SetName(string newName);
}
