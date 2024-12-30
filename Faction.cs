using UnityEngine;
using System.Collections.Generic;
using CSM.Base; // Added namespace

[AddComponentMenu("Game/Faction")]
public class Faction : MonoBehaviour, IShipOwner
{
    [SerializeField] private FactionType factionType;
    public FactionType Type => factionType;
    
    protected List<Ship> ownedShips = new List<Ship>();

    public virtual void AddShip(Ship ship)
    {
        if (!ownedShips.Contains(ship))
            ownedShips.Add(ship);
    }

    public virtual void RemoveShip(Ship ship)
    {
        ownedShips.Remove(ship);
    }

    public virtual void SelectShip(Ship ship)
    {
        // Implement selection logic
    }

    public virtual List<Ship> GetOwnedShips()
    {
        return ownedShips;
    }

    private void Start()
    {
        if (FactionManager.Instance != null)
        {
            // Initialization
        }
    }
}