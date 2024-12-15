using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    private List<Ship> ownedShips = new List<Ship>();
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

        if (selectedShip == null && ownedShips.Count == 1)
        {
            SelectShip(ship);
        }
    }

    public void SelectShip(Ship ship)
    {
        if (ownedShips.Contains(ship))
        {
            selectedShip = ship;
            Debug.Log($"Selected ship: {ship.ShipName}");
        }
    }

    public void SelectNextShip()
    {
        if (ownedShips.Count == 0) return;

        int currentIndex = selectedShip != null ? ownedShips.IndexOf(selectedShip) : -1;
        int nextIndex = (currentIndex + 1) % ownedShips.Count;
        SelectShip(ownedShips[nextIndex]);
    }

    public Ship GetSelectedShip()
    {
        return selectedShip;
    }

    public List<Ship> GetOwnedShips()
    {
        return new List<Ship>(ownedShips);
    }
}