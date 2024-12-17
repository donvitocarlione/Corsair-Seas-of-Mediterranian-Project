using UnityEngine;
using System.Collections.Generic;

public class Player : Pirate
{
    private Ship selectedShip;
    private ShipSelectionUI shipSelectionUI;

    private void Start()
    {
        Debug.Log($"[Player] Initialized Player - Ships owned: {GetOwnedShips().Count}");
    }

    public override void AddShip(Ship ship)
    {
        if (ship == null)
        {
            Debug.LogError("[Player] Attempting to add a null ship!");
            return;
        }

        Debug.Log($"[Player] Adding ship {ship.ShipName} to player's fleet");
        
        base.AddShip(ship);
        
        // Remove AI controller if present
        AIShipController aiController = ship.GetComponent<AIShipController>();
        if (aiController != null)
        {
            Debug.Log($"[Player] Removing AI controller from ship {ship.ShipName}");
            Destroy(aiController);
        }
        
        // Update UI
        if (shipSelectionUI != null)
        {
            Debug.Log($"[Player] Updating ship selection UI with {GetOwnedShips().Count} ships");
            shipSelectionUI?.UpdateShipList(GetOwnedShips());
        }

        // Auto-select if this is the first ship
        if (selectedShip == null && GetOwnedShips().Count == 1)
        {
            Debug.Log($"[Player] Auto-selecting first ship {ship.ShipName}");
            SelectShip(ship);
        }
    }

    public override void SelectShip(Ship ship)
    {
        if (ship == null)
        {
            Debug.LogWarning("[Player] Attempted to select null ship");
            return;
        }

        if (!GetOwnedShips().Contains(ship))
        {
            Debug.LogWarning($"[Player] Attempted to select ship {ship.ShipName} not owned by player");
            return;
        }

        Debug.Log($"[Player] Selecting ship: {ship.ShipName}");
        
        // Deselect current ship if any
        if (selectedShip != null)
        {
            Debug.Log($"[Player] Deselecting previous ship: {selectedShip.ShipName}");
        }

        base.SelectShip(ship);
        selectedShip = ship;
    }

    public void SelectNextShip()
    {
        List<Ship> ownedShips = GetOwnedShips();
        if (ownedShips.Count == 0)
        {
            Debug.LogWarning("[Player] Cannot select next ship - no ships owned");
            return;
        }

        int currentIndex = selectedShip != null ? ownedShips.IndexOf(selectedShip) : -1;
        int nextIndex = (currentIndex + 1) % ownedShips.Count;
        
        Debug.Log($"[Player] Cycling ship selection from index {currentIndex} to {nextIndex}");
        SelectShip(ownedShips[nextIndex]);
    }

    public Ship GetSelectedShip()
    {
        if (selectedShip == null)
        {
            Debug.LogWarning("[Player] GetSelectedShip called but no ship is selected");
        }
        else
        {
            Debug.Log($"[Player] GetSelectedShip returning: {selectedShip.ShipName}");
        }
        return selectedShip;
    }

    public override void RemoveShip(Ship ship)
    {
        if (ship == null)
        {
            Debug.LogError("[Player] Attempting to remove null ship");
            return;
        }

        Debug.Log($"[Player] Removing ship {ship.ShipName} from player's fleet");

        // If removing currently selected ship, clear selection
        if (ship == selectedShip)
        {
            Debug.Log($"[Player] Removed ship was selected - clearing selection");
            selectedShip = null;
        }

        base.RemoveShip(ship);

        // Update UI
        if (shipSelectionUI != null)
        {
            Debug.Log($"[Player] Updating ship selection UI after removal - {GetOwnedShips().Count} ships remaining");
            shipSelectionUI.UpdateShipList(GetOwnedShips());
        }
    }
}