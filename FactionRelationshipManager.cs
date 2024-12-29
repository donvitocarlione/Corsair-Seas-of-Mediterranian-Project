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
            FactionManager.Instance.EventSystem.OnFactionChanged += HandleFactionChange;
        }
    }

    private void HandleFactionChange(FactionType faction, object data, FactionChangeType changeType)
    {
        if (changeType != FactionChangeType.RelationChanged) return;
        
        if (data is float newValue)
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
    }

    private void OnDestroy()
    {
        if (FactionManager.Instance != null)
        {
            FactionManager.Instance.EventSystem.OnFactionChanged -= HandleFactionChange;
        }
    }
}