using UnityEngine;
using System.Collections.Generic;
using CSM.Base;

public class Pirate : MonoBehaviour, IShipOwner
{
    protected List<Ship> ownedShips = new List<Ship>();

    public virtual void AddShip(Ship ship)
    {
        if (ship == null)
        {
            Debug.LogError("Attempting to add a null ship!");
            return;
        }

        if (!ownedShips.Contains(ship))
        {
            ownedShips.Add(ship);
            ship.SetOwner(this);
            ship.Initialize(ship.ShipName);

            if (!(this is Player) && ship.GetComponent<AIShipController>() == null)
            {
                ship.gameObject.AddComponent<AIShipController>().Initialize(ship);
            }
        }
    }

    public virtual void RemoveShip(Ship ship)
    {
        if (ship == null)
        {
            Debug.LogError("Attempting to remove a null ship!");
            return;
        }

        if (ownedShips.Contains(ship))
        {
            ownedShips.Remove(ship);
            if (ship.ShipOwner == this)
            {
                ship.ClearOwner();
            }
            if (!(this is Player) && ship.TryGetComponent<AIShipController>(out var aiController))
            {
                Destroy(aiController);
            }
        }
    }

    public virtual void SelectShip(Ship ship)
    {
        if (ship == null)
        {
            Debug.LogError("Attempting to select a null ship!");
            return;
        }

        if (ownedShips.Contains(ship))
        {
            foreach (var ownedShip in ownedShips)
            {
                if (ownedShip != null && (object)ownedShip != (object)ship && ownedShip.IsSelected)
                {
                    ownedShip.Deselect();
                }
            }
            ship.Select();
        }
    }

    public List<Ship> GetOwnedShips()
    {
        return new List<Ship>(ownedShips);
    }
}