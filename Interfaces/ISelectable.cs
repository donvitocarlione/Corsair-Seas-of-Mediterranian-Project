// ISelectable.cs
using UnityEngine;

namespace CSM.Base
{
    public interface ISelectable
    {
        bool IsSelected { get; }
        void Select();
        void Deselect();
        Transform GetTransform();
    }
}