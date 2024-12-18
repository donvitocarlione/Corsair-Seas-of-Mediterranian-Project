using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FactionUIManager : MonoBehaviour
{
    [System.Serializable]
    private class FactionUI
    {
        public Faction faction;
        public Image flagImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI influenceText;
        public Slider relationshipSlider;
    }

    [SerializeField] private List<FactionUI> factionUIs = new List<FactionUI>();
    [SerializeField] private Color positiveRelationColor = Color.green;
    [SerializeField] private Color neutralRelationColor = Color.yellow;
    [SerializeField] private Color negativeRelationColor = Color.red;

    private void Start()
    {
        if (FactionManager.Instance != null)
        {
            FactionManager.Instance.OnRelationshipChanged += UpdateRelationshipUI;
            FactionManager.Instance.OnInfluenceUpdated += UpdateInfluenceUI;
            InitializeUI();
        }
    }

    private void InitializeUI()
    {
        foreach (var ui in factionUIs)
        {
            if (ui.faction != null)
            {
                ui.nameText.text = ui.faction.FactionName;
                UpdateInfluenceText(ui, ui.faction.CurrentInfluence);
                
                // Get initial relationship values
                float relationshipValue = FactionManager.Instance.GetRelationship(ui.faction);
                ui.relationshipSlider.value = relationshipValue;
                UpdateSliderColor(ui.relationshipSlider, relationshipValue);

                // Set faction flag if available
                if (ui.faction.FactionFlag != null && ui.flagImage != null)
                {
                    ui.flagImage.sprite = ui.faction.FactionFlag;
                }
            }
        }
    }

    private void UpdateRelationshipUI(Faction faction1, Faction faction2, float newValue)
    {
        foreach (var ui in factionUIs)
        {
            // Update UI for both factions involved
            if (ui.faction == faction1 || ui.faction == faction2)
            {
                float relationshipValue = FactionManager.Instance.GetRelationship(ui.faction);
                ui.relationshipSlider.value = relationshipValue;
                UpdateSliderColor(ui.relationshipSlider, relationshipValue);
            }
        }
    }

    private void UpdateInfluenceUI(Faction faction, float newInfluence)
    {
        var ui = factionUIs.Find(x => x.faction == faction);
        if (ui != null)
        {
            UpdateInfluenceText(ui, newInfluence);
        }
    }

    private void UpdateInfluenceText(FactionUI ui, float influence)
    {
        ui.influenceText.text = $"Influence: {influence:F1}%";
    }

    private void UpdateSliderColor(Slider slider, float value)
    {
        Color targetColor;
        if (value >= 75f)
            targetColor = positiveRelationColor;
        else if (value >= 25f)
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
            FactionManager.Instance.OnRelationshipChanged -= UpdateRelationshipUI;
            FactionManager.Instance.OnInfluenceUpdated -= UpdateInfluenceUI;
        }
    }
}
