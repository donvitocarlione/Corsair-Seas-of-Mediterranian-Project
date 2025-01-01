// Interfaces/IEntityOwner.cs
public interface IEntityOwner
{
    string OwnerName { get; }
    FactionType Faction { get; }
}