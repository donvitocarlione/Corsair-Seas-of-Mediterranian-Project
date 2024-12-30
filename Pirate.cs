using UnityEngine;
using System.Collections.Generic;
using CSM.Base;
using static ShipExtensions;

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

    protected override void Awake()
    {
        base.Awake();
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
        if (!isInitialized || !Equals(newFaction, Faction))
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

        try
        {
            FactionManager.Instance.RegisterPirate(Faction, this);
            Debug.Log($"Registered pirate with faction {Faction}");
        }
        catch (System.ArgumentException e)
        {
            Debug.LogError($"Failed to register pirate: {e.Message}");
        }
    }

    private void UnregisterFromFaction()
    {
        if (FactionManager.Instance == null) return;

        try
        {
            FactionManager.Instance.UnregisterPirate(Faction, this);
            Debug.Log($"Unregistered pirate from faction {Faction}");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error unregistering pirate: {e.Message}");
        }
    }

    public virtual void AddShip(Ship ship)
    {
        if (ReferenceEquals(ship, null))
        {
            Debug.LogError("Attempting to add a null ship!");
            return;
        }

        if (!ownedShips.Contains(ship))
        {
            ownedShips.Add(ship);
            ship.SetOwner(this);
            ship.Initialize(Faction, ship.Name);
            Debug.Log($"Added ship {ship.ShipName()} to {GetType().Name}'s fleet");
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
            if (ReferenceEquals(ship.ShipOwner, this))
            {
                ship.ClearOwner();
            }
            Debug.Log($"Removed ship {ship.ShipName()} from {GetType().Name}'s fleet");
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
                if (ownedShip != null && ownedShip != ship && ownedShip.IsSelected)
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
                ship.Initialize(newFaction, ship.Name);
            }
        }
    }
}