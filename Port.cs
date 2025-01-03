// Port.cs
using UnityEngine;
using CSM.Base;

public class Port : SeaEntityBase
{
    
    [SerializeField]
    private string portName;
    
    public string PortName => portName;
     protected override void OnDestroy()
    {
      
    }
}