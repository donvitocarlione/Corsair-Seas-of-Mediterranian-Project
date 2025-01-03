// Pirate.cs
using UnityEngine;
using System.Collections.Generic;
using CSM.Base;
using static ShipExtensions;

[AddComponentMenu("Game/Pirate")]
public class Pirate : SeaEntityBase, IEntityOwner
{
    [Header("Pirate Identity")]
    [SerializeField] private string pirateName;
    [SerializeField] private PirateRank rank = PirateRank.Regular;

    [SerializeField, Range(0f, 100f), Tooltip("Pirate's reputation affects trading and diplomacy")]
    protected float reputation = 50f;
    [SerializeField, Min(0f), Tooltip("Current wealth in gold coins")]
    protected float wealth = 1000f;

    protected List<Ship> ownedShips;
    private bool isInitialized;
    private const float MIN_REPUTATION = 0f;
    private const float MAX_REPUTATION = 100f;

    //Implement IEntityOwner
    public string OwnerName => EntityName;
    public override FactionType Faction { get; protected set; } // Changed set to protected

    protected override void Awake()
    {
        base.Awake();
        ownedShips = new List<Ship>();
    }

    protected override void Start()
    {
        base.Start();
     }


    protected override void OnDestroy()
    {
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

        base.OnDestroy();
    }

    public void SetRank(PirateRank newRank)
    {
        rank = newRank;
        Debug.Log($"{pirateName}'s rank has been changed to {rank}");
        // Notify any listeners about rank change, if needed.
    }

    public void ModifyReputation(float amount)
    {
        reputation = Mathf.Clamp(reputation + amount, MIN_REPUTATION, MAX_REPUTATION);
    }

    public void ModifyWealth(float amount)
    {
        wealth = Mathf.Max(0f, wealth + amount);
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
            // Re-initialize ship with pirate as the owner, because this ship was probably spawned before this pirate
             ship.Initialize(ship.Name, Faction, this);
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
            if (ReferenceEquals(ship.Owner, this))
            {
                ship.SetOwner(null);
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
}