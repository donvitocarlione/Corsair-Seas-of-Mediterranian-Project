using UnityEngine;
using CSM.Base;

public class Port : MonoBehaviour
{
    [SerializeField]
    private string portName;
    
    public string PortName => portName;
    
    private void Start()
    {
        if (string.IsNullOrEmpty(portName))
        {
            portName = $"Port_{gameObject.name}";
            Debug.LogWarning($"Port name was empty, assigned default name: {portName}");
        }
    }
}