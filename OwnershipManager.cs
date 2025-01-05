using UnityEngine;
using System.Collections.Generic;
using CSM.Base;

public class OwnershipManager : MonoBehaviour
{
    public static OwnershipManager Instance { get; private set; }

    private Dictionary<Ship, IEntityOwner> shipOwnership = new Dictionary<Ship, IEntityOwner>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool TransferOwnership(Ship ship, IEntityOwner newOwner)
    {
        if (ship == null)
        {
            Debug.LogError("[OwnershipManager] Cannot transfer ownership: Ship is null.");
            return false;
        }
         if (newOwner == null)
        {
             Debug.LogError("[OwnershipManager] Cannot transfer ownership: New owner is null.");
            return false;
        }
       
        if(shipOwnership.TryGetValue(ship, out var currentOwner) && currentOwner != null)
        {
            if(currentOwner is Pirate pirate)
            {
                pirate.RemoveShip(ship);
            }
        }
           
        shipOwnership[ship] = newOwner;

        if (newOwner is Pirate newPirate)
        {
            newPirate.AddShip(ship);
        }

       ship.SetOwner(newOwner);
       GameEvents.ShipOwnerChanged(ship, newOwner);

       Debug.Log($"[OwnershipManager] Ship '{ship.Name}' ownership transferred to {newOwner.GetType().Name}.");
       return true;
    }
     public IEntityOwner GetOwner(Ship ship)
    {
        if (ship == null) return null;
       if (shipOwnership.TryGetValue(ship, out var owner))
        {
            return owner;
        }
        return null;
    }
   public bool UnregisterShip(Ship ship)
    {
       if (ship == null) return false;
        return shipOwnership.Remove(ship);
    }
}