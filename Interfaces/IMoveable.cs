// IMoveable.cs
using UnityEngine;

namespace CSM.Base
{
    public interface IMoveable
    {
        void SetDestination(Vector3 destination);
        bool IsMoving { get; }
        float Speed { get; set; }
    }
}