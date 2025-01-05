// IOwnable.cs
using CSM.Base;

public interface IOwnable
{
    IEntityOwner Owner { get; }
    bool SetOwner(IEntityOwner newOwner);
    void ClearOwner();
    event System.Action<IEntityOwner> OnOwnerChanged;
}