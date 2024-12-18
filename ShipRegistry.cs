using UnityEngine;
using System.Collections.Generic;

public class ShipRegistry : MonoBehaviour
{
    [SerializeField] private bool debugPositions = false;
    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
    private HashSet<Ship> registeredShips = new HashSet<Ship>();
    private Dictionary<Faction, HashSet<Ship>> factionShips = new Dictionary<Faction, HashSet<Ship>>();

    private void Start()
    {
        if (FactionManager.Instance != null)
        {
            foreach (var faction in FactionManager.Instance.GetAllFactions())
            {
                factionShips[faction] = new HashSet<Ship>();
            }
        }
    }

    public void RegisterShip(Ship ship)
    {
        if (ship == null)
        {
            Debug.LogWarning("[ShipRegistry] Attempted to register null ship");
            return;
        }

        registeredShips.Add(ship);
        RegisterShipPosition(ship.transform.position);

        var shipFaction = ship.GetComponent<FactionMember>()?.Faction;
        if (shipFaction != null)
        {
            if (!factionShips.ContainsKey(shipFaction))
            {
                factionShips[shipFaction] = new HashSet<Ship>();
            }
            factionShips[shipFaction].Add(ship);

            // Notify FactionManager
            FactionManager.Instance?.OnShipRegistered(ship, shipFaction);
        }

        Debug.Log($"[ShipRegistry] Registered ship {ship.ShipName} with faction {(shipFaction?.FactionName ?? "None")}");
    }

    public void RegisterShipPosition(Vector3 position)
    {
        if (debugPositions)
        {
            Debug.Log($"[ShipRegistry] Registering position: {position}");
        }
        occupiedPositions.Add(position);
    }

    public void UnregisterShipPosition(Vector3 position)
    {
        if (debugPositions)
        {
            Debug.Log($"[ShipRegistry] Unregistering position: {position}");
        }
        occupiedPositions.Remove(position);
    }

    public void OnShipDestroyed(Ship ship)
    {
        if (ship != null)
        {
            var shipFaction = ship.GetComponent<FactionMember>()?.Faction;
            Debug.Log($"[ShipRegistry] Ship {ship.ShipName} destroyed, removing from registry");
            
            UnregisterShipPosition(ship.transform.position);
            registeredShips.Remove(ship);

            if (shipFaction != null && factionShips.ContainsKey(shipFaction))
            {
                factionShips[shipFaction].Remove(ship);
                // Notify FactionManager
                FactionManager.Instance?.OnShipDestroyed(ship, shipFaction);
            }

            Destroy(ship.gameObject, 2f); // Delayed destruction for effects
        }
    }

    public bool IsSafePosition(Vector3 position, float minDistance)
    {
        foreach (Vector3 occupied in occupiedPositions)
        {
            float distance = Vector3.Distance(
                new Vector3(position.x, 0f, position.z),
                new Vector3(occupied.x, 0f, occupied.z)
            );

            if (distance < minDistance)
            {
                if (debugPositions)
                {
                    Debug.Log($"[ShipRegistry] Position {position} too close to occupied position {occupied} (Distance: {distance})");
                }
                return false;
            }
        }

        if (debugPositions)
        {
            Debug.Log($"[ShipRegistry] Position {position} is safe");
        }
        return true;
    }

    public IReadOnlyCollection<Ship> GetShipsForFaction(Faction faction)
    {
        if (faction != null && factionShips.TryGetValue(faction, out var ships))
        {
            return ships;
        }
        return new HashSet<Ship>();
    }

    public IReadOnlyCollection<Ship> GetAllShips()
    {
        return registeredShips;
    }

    public void Clear()
    {
        occupiedPositions.Clear();
        registeredShips.Clear();
        factionShips.Clear();
        if (debugPositions)
        {
            Debug.Log("[ShipRegistry] Cleared all registered positions and ships");
        }
    }
}