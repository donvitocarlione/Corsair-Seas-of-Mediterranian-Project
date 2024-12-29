using UnityEngine;

[CreateAssetMenu(fileName = "FactionConfiguration", menuName = "Game/Faction Configuration")]
public class FactionConfiguration : ScriptableObject
{
    [Header("Relation Settings")]
    public float defaultInfluence = 50f;
    public float defaultResourceLevel = 50f;
    public float neutralRelation = 50f;
    public float warThreshold = 25f;
    public float allyThreshold = 75f;
    public float minRelation = 0f;
    public float maxRelation = 100f;
    
    [Header("Trade Settings")]
    public float tradeRelationMultiplier = 0.1f;
    
    [Header("Capture Settings")]
    public float captureRelationPenalty = 20f;
    public int captureInfluenceChange = 10;
}