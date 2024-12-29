using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[AddComponentMenu("Game/Faction UI Manager")]
public class FactionUIManager : MonoBehaviour
{
    [System.Serializable]
    private class FactionUI
    {
        public FactionType faction;
        public Image flagImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI influenceText;
        public Slider relationshipSlider;
    }

    [Header("UI Elements")]
    [SerializeField] private List<FactionUI> factionUIs = new List<FactionUI>();

    [Header("Color Settings")]
    [SerializeField] private Color positiveRelationColor = Color.green;
    [SerializeField] private Color neutralRelationColor = Color.yellow;
    [SerializeField] private Color negativeRelationColor = Color.red;

    private void Start()
    {
        if (FactionManager.Instance != null)
        {
            FactionManager.Instance.EventSystem.OnFactionChanged += HandleFactionChange;
            InitializeUI();
        }
        else
        {
            Debug.LogError("FactionManager not found! UI will not be initialized.");
        }
    }

    private void InitializeUI()
    {
        foreach (var ui in factionUIs)
        {
            try
            {
                var factionData = FactionManager.Instance.GetFactionData(ui.faction);
                UpdateUIElement(ui, factionData);
            }
            catch (System.ArgumentException e)
            {
                Debug.LogError($"Error initializing UI for faction {ui.faction}: {e.Message}");
            }
        }
    }

    private void HandleFactionChange(FactionType faction, object data, FactionChangeType changeType)
    {
        var ui = factionUIs.Find(x => x.faction == faction);
        if (ui == null) return;

        switch (changeType)
        {
            case FactionChangeType.RelationChanged:
                if (data is float relationValue)
                {
                    UpdateRelationshipUI(ui, relationValue);
                }
                break;

            case FactionChangeType.InfluenceChanged:
                if (data is int influenceValue)
                {
                    UpdateInfluenceUI(ui, influenceValue);
                }
                break;
        }
    }

    private void UpdateUIElement(FactionUI ui, FactionDefinition factionData)
    {
        if (ui.nameText != null)
        {
            ui.nameText.text = factionData.Name;
        }

        if (ui.influenceText != null)
        {
            ui.influenceText.text = $"Influence: {factionData.Influence}%";
        }

        if (ui.flagImage != null)
        {
            ui.flagImage.color = factionData.Color;
        }

        if (ui.relationshipSlider != null)
        {
            ui.relationshipSlider.value = FactionManager.Instance.configuration.neutralRelation;
            UpdateSliderColor(ui.relationshipSlider, FactionManager.Instance.configuration.neutralRelation);
        }
    }

    private void UpdateRelationshipUI(FactionUI ui, float newValue)
    {
        if (ui.relationshipSlider != null)
        {
            ui.relationshipSlider.value = newValue;
            UpdateSliderColor(ui.relationshipSlider, newValue);
        }
    }

    private void UpdateInfluenceUI(FactionUI ui, int newInfluence)
    {
        if (ui.influenceText != null)
        {
            ui.influenceText.text = $"Influence: {newInfluence}%";
        }
    }

    private void UpdateSliderColor(Slider slider, float value)
    {
        Color targetColor;
        var config = FactionManager.Instance.configuration;

        if (value >= config.allyThreshold)
            targetColor = positiveRelationColor;
        else if (value >= config.warThreshold)
            targetColor = neutralRelationColor;
        else
            targetColor = negativeRelationColor;

        var fillImage = slider.fillRect.GetComponent<Image>();
        if (fillImage != null)
        {
            fillImage.color = targetColor;
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