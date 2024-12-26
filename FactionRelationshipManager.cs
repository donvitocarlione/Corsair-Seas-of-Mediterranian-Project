using UnityEngine;
using System.Collections.Generic;

public class FactionRelationshipManager : MonoBehaviour
{
    [System.Serializable]
    private class RelationshipTrigger
    {
        public float relationshipThreshold;
        public string eventDescription;
        public UnityEngine.Events.UnityEvent onTrigger;
    }

    [SerializeField] private List<RelationshipTrigger> relationshipTriggers = new List<RelationshipTrigger>();

    private void Start()
    {
        if (FactionManager.Instance != null)
        {
            FactionManager.Instance.OnRelationChanged += HandleRelationshipChange;
        }
    }

    private void HandleRelationshipChange(FactionType faction1, FactionType faction2, float newValue)
    {
        foreach (var trigger in relationshipTriggers)
        {
            if (Mathf.Approximately(newValue, trigger.relationshipThreshold))
            {
                Debug.Log($"Relationship trigger activated: {trigger.eventDescription}");
                trigger.onTrigger?.Invoke();
            }
        }
    }

    private void OnDestroy()
    {
        if (FactionManager.Instance != null)
        {
            FactionManager.Instance.OnRelationChanged -= HandleRelationshipChange;
        }
    }
}