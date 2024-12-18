using UnityEngine;

[CreateAssetMenu(fileName = "New Faction", menuName = "Game/Faction")]
public class Faction : ScriptableObject
{
    [SerializeField] private string factionName;
    [SerializeField] private Sprite factionFlag;
    [SerializeField] private Color factionColor = Color.white;
    [SerializeField] private bool isPlayableFaction;

    public string FactionName => factionName;
    public Sprite FactionFlag => factionFlag;
    public Color FactionColor => factionColor;
    public bool IsPlayableFaction => isPlayableFaction;

    public bool IsFriendlyWith(Faction otherFaction)
    {
        if (otherFaction == null || otherFaction == this) return true;
        float relationship = FactionManager.Instance.GetRelationship(this, otherFaction);
        return relationship >= FactionConstants.FRIENDLY_THRESHOLD;
    }

    public bool IsHostileWith(Faction otherFaction)
    {
        if (otherFaction == null || otherFaction == this) return false;
        float relationship = FactionManager.Instance.GetRelationship(this, otherFaction);
        return relationship <= FactionConstants.HOSTILE_THRESHOLD;
    }

    public bool IsNeutralWith(Faction otherFaction)
    {
        if (otherFaction == null || otherFaction == this) return false;
        float relationship = FactionManager.Instance.GetRelationship(this, otherFaction);
        return relationship > FactionConstants.HOSTILE_THRESHOLD && 
               relationship < FactionConstants.FRIENDLY_THRESHOLD;
    }

    public float GetCurrentInfluence()
    {
        return FactionManager.Instance.GetInfluence(this);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(factionName))
        {
            factionName = name;
        }
    }
#endif
}
