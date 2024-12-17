using UnityEngine;

public class FactionInitializer : MonoBehaviour
{
    private void Start()
    {
        if (FactionManager.Instance != null)
        {
            // Set Ottomans and Venetians at war (very low relations)
            FactionManager.Instance.UpdateFactionRelation(FactionType.Ottomans, FactionType.Venetians, 10f);
            
            // Log the status
            float relation = FactionManager.Instance.GetRelationBetweenFactions(FactionType.Ottomans, FactionType.Venetians);
            bool atWar = FactionManager.Instance.AreFactionsAtWar(FactionType.Ottomans, FactionType.Venetians);
            
            Debug.Log($"[FactionInitializer] Ottomans and Venetians relation set to {relation}. At War: {atWar}");
        }
    }
}
