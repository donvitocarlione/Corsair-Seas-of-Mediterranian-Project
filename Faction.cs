using UnityEngine;

[AddComponentMenu("Game/Faction")]
public class Faction : MonoBehaviour
{
    [SerializeField] private FactionType factionType;
    public FactionType Type => factionType;

    private void Start()
    {
        // Register with FactionManager if needed
        if (FactionManager.Instance != null)
        {
            // Any initialization with FactionManager can be done here
        }
    }

    private void OnDestroy()
    {
        // Cleanup if needed
    }
}