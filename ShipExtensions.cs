using UnityEngine;

public static class ShipExtensions
{
    public static string ShipName(this Ship ship)
    {
        return ship?.Name ?? "Unknown Ship";
    }
}