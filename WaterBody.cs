using UnityEngine;

public class WaterBody : MonoBehaviour
{
    [SerializeField] private float surfaceLevel = 0f;

    public float GetYBound()
    {
        return transform.position.y + surfaceLevel;
    }
}
