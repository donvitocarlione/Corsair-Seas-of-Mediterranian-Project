using UnityEngine;
using System.Collections.Generic;
using CSM.Base;
using static ShipExtensions;

[AddComponentMenu("Game/Pirate")]
public class Pirate : SeaEntityBase, IEntityOwner, IShipOwner
{
    [Header("Pirate Identity")]
    [SerializeField] private string pirateName;
    [SerializeField] private PirateRank rank = PirateRank.Regular;

    [SerializeField, Range(0f, 100f), Tooltip("Pirate's reputation affects trading and diplomacy")]
    protected float reputation = 50f;
    [SerializeField, Min(0f), Tooltip("Current wealth in gold coins")]
    protected float wealth = 1000f;

    [SerializeField] protected FactionType initialFaction = FactionType.None; // New field: initial faction
    private bool hasInitializedFaction = false; // New field: initialization flag

    protected List<Ship> ownedShips;
    private bool isInitialized;
    private const float MIN_REPUTATION = 0f;
    private const float MAX_REPUTATION = 100f;
    // Add our new registration flag alongside other state flags
    private bool isFactionRegistered = false;

    public PirateRank Rank => rank;

     //Implement IEntityOwner
     public string OwnerName => EntityName;


    protected override void Awake()
    {
        base.Awake();
        ownedShips = new List<Ship>();
    }

    protected override void Start()
    {
        base.Start();

        // Only initialize faction if we haven't already
        if (!hasInitializedFaction)
        {
            // If we're the player, let the player handling take care of it
            if (!(this is Player))
            {
                InitializeWithFaction(initialFaction);
            }
        }
    }

    protected void InitializeWithFaction(FactionType faction)
    {
        if (hasInitializedFaction)
        {
            Debug.LogWarning($"[Pirate] Attempting to initialize faction for {name} when already initialized!");
            return;
        }

        // Set the faction directly without going through the None state
        base.SetFaction(faction);
        RegisterWithFaction();
        hasInitializedFaction = true;
        HandleFactionChanged(faction);

        Debug.Log($"[Pirate] {name} initialized directly with faction {faction}");
    }

     protected override void OnDestroy()
    {
        if (isFactionRegistered)
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

     public override void SetFaction(FactionType newFaction)
    {
        // If this is our first initialization, use the direct path
         if (!hasInitializedFaction)
        {
            InitializeWithFaction(newFaction);
            return;
        }
        
        // Otherwise handle normal faction changes
        if (!Equals(newFaction, Faction))
        {
            UnregisterFromFaction();
             base.SetFaction(newFaction);
             RegisterWithFaction();
            HandleFactionChanged(newFaction);
        }
    }

    // Keep the original method signature
    protected void RegisterWithFaction()
    {
        if (FactionManager.Instance == null)
        {
            Debug.LogError("FactionManager instance not found!");
            return;
        }

        // Only proceed if we haven't registered yet
        if (!isFactionRegistered)
        {
            try
            {
                FactionManager.Instance.RegisterPirate(Faction, this);
                isFactionRegistered = true;
                Debug.Log($"Registered pirate with faction {Faction}");
            }
            catch (System.ArgumentException e)
            {
                Debug.LogError($"Failed to register pirate: {e.Message}");
            }
        }
    }

    // Keep the original method signature
    protected void UnregisterFromFaction()
    {
        if (FactionManager.Instance == null) return;

        if (isFactionRegistered)
        {
            try
            {
                FactionManager.Instance.UnregisterPirate(Faction, this);
                isFactionRegistered = false;
                Debug.Log($"Unregistered pirate from faction {Faction}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error unregistering pirate: {e.Message}");
            }
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

    protected virtual void HandleFactionChanged(FactionType newFaction)
    {
        Debug.Log($"{GetType().Name}'s faction changed to {newFaction}");

        if (ownedShips == null) return;

        // Update faction for all owned ships
        foreach (var ship in ownedShips.ToArray())
        {
            if (ship != null)
            {
                ship.Initialize(ship.Name, newFaction, this);
            }
        }
    }
        // Add method to handle faction leadership setup
    public void SetupAsFactionLeader(FactionType faction)
    {
        SetFaction(faction);
       SetRank(PirateRank.FactionLeader);
        // Additional leader setup logic
        Debug.Log($"Pirate {name} set up as faction leader for {faction}");
    }
}