using UnityEngine;

[CreateAssetMenu(fileName = "New Faction Definition", menuName = "Game/Faction Definition")]
public class FactionDefinitionAsset : ScriptableObject
{
    public FactionType factionType;
    public string factionName;
    public Color factionColor;

    public FactionDefinition GetFactionData()
    {
        // Use the constructor instead of object initialization
        var definition = new FactionDefinition(factionType, factionName);
        // Use internal setters
        definition.Color = factionColor;
        return definition;
    }
}