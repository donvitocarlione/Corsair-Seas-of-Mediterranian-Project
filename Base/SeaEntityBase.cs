using UnityEngine;

namespace CSM.Base {
  public class SeaEntityBase : MonoBehaviour
  {
      // Add common fields and methods used by your ships and other entities.
      public FactionType Faction { get; protected set; }
      public string Name { get; protected set; }

      protected virtual void Start()
      {

      }
      protected virtual void OnDestroy() {

      }

      public virtual void SetFaction(FactionType newFaction)
      {
          Faction = newFaction;
      }
      public virtual void SetName(string newName) {
          Name = newName;
      }
  }
}