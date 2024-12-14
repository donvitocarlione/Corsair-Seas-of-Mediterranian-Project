using UnityEngine;
using System.Collections.Generic;
using CSM.Base;

public class FactionInitializer : MonoBehaviour
{
    private ShipSpawner shipSpawner;
    private FactionManager factionManager;
    private Player playerInstance;
    private List<FactionShipData> factionData;

    public void Initialize(ShipSpawner spawner, FactionManager manager, Player player, List<FactionShipData> data)
    {
        Debug.Log("[FactionInitializer] Initializing...");
        shipSpawner = spawner;
        factionManager = manager;
        playerInstance = player;
        factionData = data;

        if (!ValidateReferences())
        {
            Debug.LogError("[FactionInitializer] Failed to initialize - missing references");
            enabled = false;
            return;
        }
    }

    private bool ValidateReferences()
    {
        if (shipSpawner == null)
        {
            Debug.LogError("[FactionInitializer] ShipSpawner reference missing");
            return false;
        }

        if (factionManager == null)
        {
            Debug.LogError("[FactionInitializer] FactionManager reference missing");
            return false;
        }

        if (playerInstance == null)
        {
            Debug.LogError("[FactionInitializer] Player reference missing");
            return false;
        }

        if (factionData == null || factionData.Count == 0)
        {
            Debug.LogError("[FactionInitializer] No faction data provided");
            return false;
        }

        return true;
    }

    public void InitializeAllFactions()
    {
        Debug.Log("[FactionInitializer] Initializing all factions");
        foreach (var data in factionData)
        {
            Debug.Log($"[FactionInitializer] Initializing faction: {data.Faction} (IsPlayerFaction: {data.IsPlayerFaction})");
            if (data.IsPlayerFaction)
            {
                InitializePlayerShips(data);
            }
            else
            {
                InitializePiratesForFaction(data);
            }
        }
    }

    public void InitializePlayerFaction(FactionType playerFaction)
    {
        Debug.Log($"[FactionInitializer] Setting player faction to {playerFaction}");
        playerInstance.SetFaction(playerFaction);
    }

    private void InitializePlayerShips(FactionShipData data)
    {
        Debug.Log($"[FactionInitializer] Initializing player ships for faction {data.Faction}");
        if (playerInstance == null)
        {
            Debug.LogError("[FactionInitializer] Cannot initialize player ships - playerInstance is null");
            return;
        }

        for (int i = 0; i < data.InitialShipCount; i++)
        {
            Debug.Log($"[FactionInitializer] Spawning player ship {i + 1}/{data.InitialShipCount}");
            if (shipSpawner.SpawnShipForFaction(data.Faction, data) is Ship ship)
            {
                playerInstance.AddShip(ship);
                Debug.Log($"[FactionInitializer] Added ship {ship.ShipName} to player fleet");
            }
        }
    }

    private void InitializePiratesForFaction(FactionShipData data)
    {
        Debug.Log($"[FactionInitializer] Initializing pirates for faction {data.Faction}");
        for (int i = 0; i < data.InitialPirateCount; i++)
        {
            if (shipSpawner.SpawnPirateShip(data.Faction) is Pirate pirate)
            {
                int shipsPerPirate = data.InitialShipCount / data.InitialPirateCount;
                Debug.Log($"[FactionInitializer] Spawned pirate for faction {data.Faction}, assigning {shipsPerPirate} ships");
                
                for (int j = 0; j < shipsPerPirate; j++)
                {
                    if (shipSpawner.SpawnShipForFaction(data.Faction, data) is Ship ship)
                    {
                        pirate.AddShip(ship);
                        Debug.Log($"[FactionInitializer] Added ship {ship.ShipName} to pirate's fleet");
                    }
                }

                // Register pirate with FactionManager
                var factionData = factionManager.GetFactionData(data.Faction);
                if (factionData != null && !factionData.pirates.Contains(pirate))
                {
                    factionData.pirates.Add(pirate);
                    Debug.Log($"[FactionInitializer] Registered pirate with faction {data.Faction}");
                }
            }
        }
    }
}