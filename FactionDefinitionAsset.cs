using UnityEngine;

[CreateAssetMenu(fileName = "FactionDefinition", menuName = "Game/Faction Definition")]
public class FactionDefinitionAsset : ScriptableObject
{
    [Header("Core Settings")]
    public FactionType type;
    public string displayName;
    public Color color = Color.white;
    public string baseLocation = "Unknown";
    
    [Header("Initial Values")]
    public int initialInfluence = 50;
    public int initialResourceLevel = 50;
}