using UnityEngine;
using System.Collections.Generic;

public class ShipRegistry : MonoBehaviour
{
    [SerializeField] private bool debugPositions = false;
    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();

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
            Debug.Log($"[ShipRegistry] Ship {ship.ShipName} destroyed, removing from registry");
            UnregisterShipPosition(ship.transform.position);
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

    public void Clear()
    {
        occupiedPositions.Clear();
        if (debugPositions)
        {
            Debug.Log("[ShipRegistry] Cleared all registered positions");
        }
    }
}