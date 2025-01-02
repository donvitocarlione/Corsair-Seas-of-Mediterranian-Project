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
            InitializeUI();
        }
        else
        {
            Debug.LogError("FactionManager not found! UI will not be initialized.");
        }
    }

     private void OnEnable()
    {
         if (FactionManager.Instance != null)
        {
            UpdateUI();
        }
       
    }
     private void Update()
    {
        if (FactionManager.Instance != null)
        {
            UpdateUI();
        }

    }


    private void InitializeUI()
    {
         Debug.Log("[FactionUIManager] InitializeUI called.");
         foreach (var ui in factionUIs)
        {
            try
            {
                var factionData = FactionManager.Instance.GetFactionData(ui.faction);
                 if (factionData != null)
                 {
                      UpdateUIElement(ui, factionData);
                      UpdateRelationshipUI(ui, FactionManager.Instance.GetRelationBetweenFactions(ui.faction, FactionType.Independent));

                 }
                else
                 {
                    Debug.LogError($"No faction data found for faction {ui.faction}");
                }

            }
            catch (System.ArgumentException e)
            {
                Debug.LogError($"Error initializing UI for faction {ui.faction}: {e.Message}");
            }
        }
          Debug.Log("[FactionUIManager] UI Initialized.");
    }

    private void UpdateUI()
    {
        Debug.Log("[FactionUIManager] UpdateUI called.");
        foreach (var ui in factionUIs)
        {
            if (FactionManager.Instance == null)
            {
                 Debug.LogError($"[FactionUIManager] FactionManager not initialized.");
                return;
            }
            var factionData = FactionManager.Instance.GetFactionData(ui.faction);
             if (factionData != null)
            {
                 UpdateUIElement(ui, factionData);
                UpdateRelationshipUI(ui, FactionManager.Instance.GetRelationBetweenFactions(ui.faction, FactionType.Independent));
            }
             else
             {
                Debug.LogError($"[FactionUIManager] No Faction data found for  {ui.faction}.");
             }

        }
           Debug.Log("[FactionUIManager] UI Updated.");
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

       
    }

    private void UpdateRelationshipUI(FactionUI ui, float newValue)
    {
        if (ui.relationshipSlider != null)
        {
            ui.relationshipSlider.value = newValue;
            UpdateSliderColor(ui.relationshipSlider, newValue);
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
        //No more event unsubscriptions
    }
}