using UnityEngine;
using System.Collections.Generic;
using CSM.Base;

[AddComponentMenu("Game/Pirate")]
public class Pirate : SeaEntityBase, IShipOwner
{
    protected List<Ship> ownedShips;
    [SerializeField, Range(0f, 100f), Tooltip("Pirate's reputation affects trading and diplomacy")]
    protected float reputation = 50f;
    [SerializeField, Min(0f), Tooltip("Current wealth in gold coins")]
    protected float wealth = 1000f;

    private bool isInitialized;
    private const float MIN_REPUTATION = 0f;
    private const float MAX_REPUTATION = 100f;

    protected virtual void Awake()
    {
        ownedShips = new List<Ship>();
    }
    
    protected override void Start()
    {
        base.Start();
        // Only register if not the player (player will be registered by ShipManager)
        if (!(this is Player))
        {
            RegisterWithFaction();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (isInitialized)
        {
            UnregisterFromFaction();
        }

        // Clean up ships
        if (ownedShips != null)
        {
            foreach (var ship in ownedShips.ToArray())
            {
                if (ship != null)
                {
                    RemoveShip(ship);
                }
            }
            ownedShips.Clear();
        }
    }

    public void ModifyReputation(float amount)
    {
        reputation = Mathf.Clamp(reputation + amount, MIN_REPUTATION, MAX_REPUTATION);
    }

    public void ModifyWealth(float amount)
    {
        wealth = Mathf.Max(0f, wealth + amount);
    }

    public override void SetFaction(FactionType newFaction)
    {
        if (!isInitialized || !object.Equals(newFaction, Faction))
        {
            if (isInitialized)
            {
                UnregisterFromFaction();
            }
            
            base.SetFaction(newFaction);
            RegisterWithFaction();
            isInitialized = true;
            HandleFactionChanged(newFaction);
        }
    }

    private void RegisterWithFaction()
    {
        if (FactionManager.Instance == null)
        {
            Debug.LogError("FactionManager instance not found!");
            return;
        }

        var factionData = FactionManager.Instance.GetFactionData(Faction);
        if (factionData == null)
        {
            Debug.LogError($"No faction data found for faction {Faction}");
            return;
        }

        if (!factionData.pirates.Contains(this))
        {
            factionData.pirates.Add(this);
            Debug.Log($"Registered pirate with faction {Faction}");
        }
    }

    private void UnregisterFromFaction()
    {
        if (FactionManager.Instance == null) return;

        var factionData = FactionManager.Instance.GetFactionData(Faction);
        if (factionData != null && factionData.pirates.Contains(this))
        {
            factionData.pirates.Remove(this);
            Debug.Log($"Unregistered pirate from faction {Faction}");
        }
    }
    
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
            ship.Initialize(Faction, ship.ShipName);
            Debug.Log($"Added ship {ship.ShipName} to {GetType().Name}'s fleet");
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
            Debug.Log($"Removed ship {ship.ShipName} from {GetType().Name}'s fleet");
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
                if (ownedShip != null && !ReferenceEquals(ownedShip, ship) && ownedShip.IsSelected)
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

    protected virtual void HandleFactionChanged(FactionType newFaction)
    {
        Debug.Log($"{GetType().Name}'s faction changed to {newFaction}");

        if (ownedShips == null) return;
        
        // Update faction for all owned ships
        foreach (var ship in ownedShips.ToArray())
        {
            if (ship != null)
            {
                ship.Initialize(newFaction, ship.ShipName);
            }
        }
    }
}