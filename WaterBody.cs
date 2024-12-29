using UnityEngine;

public class WaterBody : MonoBehaviour
{
    [SerializeField] private float surfaceLevel = 0f;

    // Get the exact water surface height
    public float GetWaterSurfaceHeight()
    {
        return transform.position.y + surfaceLevel;
    }
    
    public float GetYBound()
    {
        return transform.position.y + surfaceLevel;
    }
}