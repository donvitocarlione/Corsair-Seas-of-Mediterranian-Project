// WaterBody.cs
using UnityEngine;

public class WaterBody : MonoBehaviour
{
    [SerializeField] private float surfaceLevel = 0f;
    public static WaterBody Instance { get; private set; }

    private void Awake()
    {
         if (Instance == null)
        {
           Instance = this;
        }
       else
        {
           Debug.LogError("Multiple water bodies detected. Destroying the new one");
           Destroy(gameObject);
        }
    }

    public float GetWaterSurfaceHeight()
    {
        return transform.position.y + surfaceLevel;
    }
    
     public float GetYBound()
    {
       return transform.position.y + surfaceLevel;
    }

    public bool IsPositionInWater(Vector3 position)
    {
         float waterHeight = GetWaterSurfaceHeight();
        return position.y <= waterHeight;
    }
}