using UnityEngine;

[System.Serializable]
public class FactionShipData
{
    [Header("Faction Settings")]
    public FactionType Faction;
    
    [Header("Ship Settings")]
    public GameObject ShipPrefab;
    public int InitialShipCount = 3;
    
    [Header("Spawn Settings")]
    public float MinSpawnDistance = 100f;
    public float MaxSpawnDistance = 300f;

    [Header("Combat Settings")]
    public float BaseAttackRange = 50f;
    public float BaseFiringArc = 45f;
    public float BaseFireRate = 1f;

    public FactionShipData(FactionType faction)
    {
        Faction = faction;
    }
}
