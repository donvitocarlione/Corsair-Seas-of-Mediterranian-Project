using UnityEngine;
using System.Collections.Generic;

public class Player : Pirate
{
    private Ship selectedShip;
    private ShipSelectionUI shipSelectionUI;


   public override void AddShip(Ship ship)
    {
        if (ship == null)
        {
            Debug.LogError("Attempting to add a null ship!");
            return;
        }

        base.AddShip(ship);
        if (ship.GetComponent<AIShipController>() != null)
        {
            Destroy(ship.GetComponent<AIShipController>());
            Debug.Log($"Removed AI controller from ship {ship.ShipName} in player fleet");
        }
        
        shipSelectionUI?.UpdateShipList(GetOwnedShips());

        if (selectedShip == null && GetOwnedShips().Count == 1)
        {
            SelectShip(ship);
        }
    }

    public override void SelectShip(Ship ship)
    {
        if (GetOwnedShips().Contains(ship))
        {
            base.SelectShip(ship);
            selectedShip = ship;
             Debug.Log($"Selected ship: {ship.ShipName}");
        }
    }

    public void SelectNextShip()
    {
        if (GetOwnedShips().Count == 0) return;

        int currentIndex = selectedShip != null ? GetOwnedShips().IndexOf(selectedShip) : -1;
        int nextIndex = (currentIndex + 1) % GetOwnedShips().Count;
        SelectShip(GetOwnedShips()[nextIndex]);
    }

    public Ship GetSelectedShip()
    {
        return selectedShip;
    }
}