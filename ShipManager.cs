using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Game/Ship Manager")]
public class ShipManager : MonoBehaviour
{
    public static ShipManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private FactionManager factionManager;
    
    private Player playerInstance;
    private Dictionary<Ship, FactionType> registeredShips = new Dictionary<Ship, FactionType>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Multiple ShipManager instances found! Destroying duplicate on {gameObject.name}");
            Destroy(this);
            return;
        }
        Instance = this;

        // Find FactionManager if not assigned
        if (factionManager == null)
        {
            factionManager = FindObjectOfType<FactionManager>();
            if (factionManager == null)
            {
                Debug.LogError("FactionManager not found! Ship faction management will be disabled.");
            }
        }
    }

    public void RegisterPlayer(Player player)
    {
        if (player == null)
            throw new System.ArgumentNullException(nameof(player));

        if (playerInstance != null && playerInstance != player)
        {
            Debug.LogWarning("Attempting to register a new player while one is already registered. Unregistering previous player.");
            UnregisterPlayer();
        }

        playerInstance = player;
        player.RegisterPlayer(); // Explicitly register the player with faction system
        Debug.Log($"Player registered with ShipManager and faction {player.Faction}");
    }

    public void UnregisterPlayer()
    {
        if (playerInstance != null)
        {
            // Clean up any player-specific registrations if needed
            foreach (var ship in playerInstance.GetOwnedShips())
            {
                if (ship != null)
                {
                    UnregisterShip(ship);
                }
            }

            playerInstance = null;
            Debug.Log("Player unregistered from ShipManager");
        }
    }

    public void RegisterShip(Ship ship)
    {
        if (ship == null)
            throw new System.ArgumentNullException(nameof(ship));

        if (!registeredShips.ContainsKey(ship))
        {
            registeredShips.Add(ship, ship.Faction);
            factionManager?.RegisterShip(ship.Faction, ship);
            Debug.Log($"Ship {ship.ShipName} registered with faction {ship.Faction}");
        }
    }

    public void UnregisterShip(Ship ship)
    {
        if (ship == null)
            throw new System.ArgumentNullException(nameof(ship));

        if (registeredShips.TryGetValue(ship, out FactionType faction))
        {
            registeredShips.Remove(ship);
            factionManager?.UnregisterShip(faction, ship);
            Debug.Log($"Ship {ship.ShipName} unregistered from faction {faction}");
        }
    }

    public void OnShipDestroyed(Ship ship)
    {
        if (ship == null) return;

        UnregisterShip(ship);
        Debug.Log($"Ship {ship.ShipName} removed from registration due to destruction");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            UnregisterPlayer();
            registeredShips.Clear();
            Instance = null;
        }
    }
}
