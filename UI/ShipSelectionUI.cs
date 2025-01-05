// ShipSelectionUI.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShipSelectionUI : MonoBehaviour
{
    [SerializeField] private Transform shipListContainer;
    [SerializeField] private Button shipButtonPrefab;
    
    private Player _player;
    private List<Button> _shipButtons = new List<Button>();

    private void Awake()
    {
       ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (shipListContainer == null)
        {
           Debug.LogError($"Ship List Container is not assigned on {gameObject.name}! Assign it in the inspector.");
        }

        if (shipButtonPrefab == null)
        {
            Debug.LogError($"Ship Button Prefab is not assigned on {gameObject.name}! Assign it in the inspector.");
        }
        else if (shipButtonPrefab.GetComponentInChildren<Text>() == null)
        {
            Debug.LogError($"Ship Button Prefab on {gameObject.name} is missing a Text component in its children!");
        }

        if(_player == null)
        {
             _player = GameManager.Instance.GetDependency<Player>();
            if (_player == null)
            {
               Debug.LogError($"No Player found in scene! Ship selection will not work on {gameObject.name}.");
            }
            else
           {
                Debug.Log($"Player automatically found for {gameObject.name}. Consider assigning it directly in the inspector.");
           }
        }
    }
    public void UpdateShipList(List<Ship> ships)
    {
        if (!ValidateComponents()) return;

       ClearShipButtons();
       if (ships == null)
        {
            Debug.LogError("Received null ships list in UpdateShipList!");
            return;
       }

        foreach (var ship in ships)
       {
            if (ship != null)
            {
                CreateShipButton(ship);
            }
           else
           {
                Debug.LogWarning("Null ship detected in ships list!");
            }
        }
    }

    private bool ValidateComponents()
    {
         if (shipButtonPrefab == null)
        {
           Debug.LogError($"Cannot update ship list: Ship Button Prefab is not assigned on {gameObject.name}!");
            return false;
        }
        if (shipListContainer == null)
        {
           Debug.LogError($"Cannot update ship list: Ship List Container is not assigned on {gameObject.name}!");
           return false;
       }
        return true;
    }
    private void CreateShipButton(Ship ship)
    {
         if (ship == null)
       {
           Debug.LogError("Attempting to create button for null ship!");
            return;
        }

        var button = Instantiate(shipButtonPrefab, shipListContainer);
        var text = button.GetComponentInChildren<Text>();
        if (text != null)
       {
            text.text = ship.Name;
       }
       else
        {
            Debug.LogError($"No Text component found in instantiated button for ship {ship.Name}!");
       }
        button.onClick.AddListener(() => OnShipButtonClicked(ship));
       _shipButtons.Add(button);
    }

    private void ClearShipButtons()
    {
       foreach (var button in _shipButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                Destroy(button.gameObject);
            }
       }
        _shipButtons.Clear();
    }

    private void OnShipButtonClicked(Ship ship)
    {
       if (_player != null)
        {
           _player.SelectShip(ship);
        }
       else
        {
           Debug.LogWarning("Cannot select ship: Player reference is missing!");
        }
    }

    public void UpdateSelection(Ship selectedShip)
    {
       if (selectedShip == null)
        {
            Debug.LogWarning("UpdateSelection called with null ship!");
           return;
        }

       foreach (var button in _shipButtons)
        {
           if (button == null) continue;

           var text = button.GetComponentInChildren<Text>();
           if (text != null && text.text == selectedShip.Name)
            {
               button.interactable = false;
            }
            else
           {
               button.interactable = true;
           }
        }
    }

    private void OnDestroy()
    {
       ClearShipButtons();
    }
}