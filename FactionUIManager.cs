using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

    [SerializeField] private List<FactionUI> factionUIs = new List<FactionUI>();
    [SerializeField] private Color positiveRelationColor = Color.green;
    [SerializeField] private Color neutralRelationColor = Color.yellow;
    [SerializeField] private Color negativeRelationColor = Color.red;

    private void Start()
    {
        if (FactionManager.Instance != null)
        {
            FactionManager.Instance.OnRelationChanged += UpdateRelationshipUI;
            FactionManager.Instance.OnInfluenceChanged += UpdateInfluenceUI;
            InitializeUI();
        }
    }

    private void InitializeUI()
    {
        foreach (var ui in factionUIs)
        {
            var factionData = FactionManager.Instance.GetFactionData(ui.faction);
            if (factionData != null)
            {
                ui.nameText.text = factionData.Name;
                ui.influenceText.text = $"Influence: {factionData.Influence}%";
                ui.relationshipSlider.value = 50f; // Default neutral value
                UpdateSliderColor(ui.relationshipSlider, 50f);
            }
        }
    }

    private void UpdateRelationshipUI(FactionType faction1, FactionType faction2, float newValue)
    {
        foreach (var ui in factionUIs)
        {
            if (ui.faction == faction2) // Update UI for the other faction
            {
                ui.relationshipSlider.value = newValue;
                UpdateSliderColor(ui.relationshipSlider, newValue);
            }
        }
    }

    private void UpdateInfluenceUI(FactionType faction, int newInfluence)
    {
        var ui = factionUIs.Find(x => x.faction == faction);
        if (ui != null)
        {
            ui.influenceText.text = $"Influence: {newInfluence}%";
        }
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
            FactionManager.Instance.OnRelationChanged -= UpdateRelationshipUI;
            FactionManager.Instance.OnInfluenceChanged -= UpdateInfluenceUI;
        }
    }
}