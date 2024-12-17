using UnityEngine;
using System.Collections.Generic;

public class Player : Pirate
{
    private List<Ship> controlledShips = new List<Ship>();
    private Ship selectedShip;

    public Ship SelectedShip => selectedShip;
    public IReadOnlyList<Ship> ControlledShips => controlledShips;

    public virtual void AddShip(Ship ship)
    {
        if (ship == null)
        {
            Debug.LogWarning("[Player] Attempted to add null ship");
            return;
        }

        if (!controlledShips.Contains(ship))
        {
            controlledShips.Add(ship);
            Debug.Log($"[Player] Added ship: {ship.Name}");

            // If this is our first ship, automatically select it
            if (selectedShip == null)
            {
                SelectShip(ship);
            }
        }
    }

    public virtual void SelectShip(Ship ship)
    {
        if (ship == null)
        {
            Debug.LogWarning("[Player] Attempted to select null ship");
            return;
        }

        if (!controlledShips.Contains(ship))
        {
            Debug.LogWarning($"[Player] Attempted to select uncontrolled ship: {ship.Name}");
            return;
        }

        selectedShip = ship;
        Debug.Log($"[Player] Selected ship: {ship.Name}");
    }

    public virtual void RemoveShip(Ship ship)
    {
        if (ship == null)
        {
            Debug.LogWarning("[Player] Attempted to remove null ship");
            return;
        }

        if (controlledShips.Contains(ship))
        {
            controlledShips.Remove(ship);
            Debug.Log($"[Player] Removed ship: {ship.Name}");

            // If we removed the selected ship, select a new one if available
            if (ship == selectedShip)
            {
                selectedShip = controlledShips.Count > 0 ? controlledShips[0] : null;
                if (selectedShip != null)
                {
                    Debug.Log($"[Player] Auto-selected new ship: {selectedShip.Name}");
                }
            }
        }
    }

    public virtual void SelectNextShip()
    {
        if (controlledShips.Count == 0)
        {
            Debug.LogWarning("[Player] No controlled ships");
            return;
        }

        int currentIndex = controlledShips.IndexOf(selectedShip);
        int nextIndex = (currentIndex + 1) % controlledShips.Count;
        SelectShip(controlledShips[nextIndex]);
        Debug.Log($"[Player] Selected next ship: {selectedShip.Name}");
    }

    public virtual Ship GetSelectedShip()
    {
        return selectedShip;
    }
}