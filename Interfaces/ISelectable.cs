public interface ISelectable
{
    bool Select();
    void Deselect();
    bool IsSelected { get; }
}