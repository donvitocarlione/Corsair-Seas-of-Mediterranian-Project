using UnityEngine;
using System.Collections.Generic;

public class DiplomacySystem : MonoBehaviour
{
    private static DiplomacySystem instance;
    public static DiplomacySystem Instance => instance;

    public enum Relationship
    {
        Hostile = -1,
        Neutral = 0,
        Friendly = 1
    }

    [System.Serializable]
    public class FactionRelationship
    {
        public FactionType factionA;
        public FactionType factionB;
        public Relationship initialRelationship;
    }

    public FactionRelationship[] defaultRelationships;
    private Dictionary<(FactionType, FactionType), Relationship> relationships = new Dictionary<(FactionType, FactionType), Relationship>();

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        InitializeRelationships();
    }

    private void InitializeRelationships()
    {
        foreach (var rel in defaultRelationships)
        {
            SetRelationship(rel.factionA, rel.factionB, rel.initialRelationship);
        }
    }

    public void SetRelationship(FactionType factionA, FactionType factionB, Relationship relationship)
    {
        relationships[(factionA, factionB)] = relationship;
        relationships[(factionB, factionA)] = relationship;
    }

    public Relationship GetRelationship(FactionType factionA, FactionType factionB)
    {
        if (relationships.TryGetValue((factionA, factionB), out Relationship relationship))
        {
            return relationship;
        }
        return Relationship.Neutral;
    }

    public bool AreFriendly(FactionType factionA, FactionType factionB)
    {
        return GetRelationship(factionA, factionB) == Relationship.Friendly;
    }

    public bool AreHostile(FactionType factionA, FactionType factionB)
    {
        return GetRelationship(factionA, factionB) == Relationship.Hostile;
    }
}
