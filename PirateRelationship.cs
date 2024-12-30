using UnityEngine;
using System.Collections.Generic;

public class PirateRelationship : MonoBehaviour
{
    private Dictionary<string, float> relationships = new Dictionary<string, float>();
    
    public float GetRelationship(string pirateId)
    {
        if (relationships.TryGetValue(pirateId, out float value))
            return value;
        return 0f; // Neutral by default
    }

    public void ModifyRelationship(string pirateId, float amount)
    {
        if (!relationships.ContainsKey(pirateId))
            relationships[pirateId] = 0f;
        
        relationships[pirateId] = Mathf.Clamp(relationships[pirateId] + amount, -100f, 100f);
    }
}