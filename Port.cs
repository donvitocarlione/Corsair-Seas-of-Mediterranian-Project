// Port.cs
using UnityEngine;
using CSM.Base;

public class Port : SeaEntityBase
{
    //Removed Faction
    //[SerializeField]
    //private FactionType owningFaction = FactionType.None;
    
    [SerializeField]
    private string portName;
    
    //Removed Faction
   // public FactionType OwningFaction => owningFaction;
    public string PortName => portName;
    //Removed SetFaction
    //public override void SetFaction(FactionType newFaction)
    //{
    //    if (owningFaction != newFaction)
    //    {
    //        var oldFaction = owningFaction;
    //        owningFaction = newFaction;
            
    //        // Notify FactionManager of the change
    //        if (FactionManager.Instance != null)
    //        {
    //            FactionManager.Instance.HandlePortCapture(newFaction, this);
    //        }
            
    //        Debug.Log($"Port {portName} changed ownership from {oldFaction} to {newFaction}");
    //    }
    //}

     protected override void OnDestroy()
    {
      
    }
}