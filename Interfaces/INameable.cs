// INameable.cs
public interface INameable
{
    string Name { get; }
    bool SetName(string newName);
}