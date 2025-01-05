using System;
using CSM.Base;

public static class GameEvents
{
    public static event Action<Ship> OnShipSpawned;
    public static event Action<Ship> OnShipDestroyed;
    public static event Action<Ship, IEntityOwner> OnShipOwnerChanged;

    public static void ShipSpawned(Ship ship) => OnShipSpawned?.Invoke(ship);
    public static void ShipDestroyed(Ship ship) => OnShipDestroyed?.Invoke(ship);
    public static void ShipOwnerChanged(Ship ship, IEntityOwner newOwner) =>
        OnShipOwnerChanged?.Invoke(ship, newOwner);
}