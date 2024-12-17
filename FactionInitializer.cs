using UnityEngine;
using System.Collections.Generic;

public class FactionInitializer : MonoBehaviour
{
    private ShipSpawner shipSpawner;
    private FactionManager factionManager;
    private Player playerInstance;
    private List<FactionShipData> factionData;

    public void Initialize(ShipSpawner spawner, FactionManager manager, Player player, List<FactionShipData> data)
    {
        shipSpawner = spawner;
        factionManager = manager;
        playerInstance = player;
        factionData = data;
        
        Debug.Log("[FactionInitializer] Components initialized");
    }

    public void InitializePlayerFaction(FactionType faction)
    {
        if (factionManager != null)
        {
            // Set initial player faction relations
            foreach (FactionType otherFaction in System.Enum.GetValues(typeof(FactionType)))
            {
                if (otherFaction != faction && otherFaction != FactionType.None)
                {
                    // Default to neutral relations (50)
                    factionManager.UpdateFactionRelation(faction, otherFaction, 50f);
                }
            }
            Debug.Log($"[FactionInitializer] Player faction {faction} initialized");
        }
    }

    public void InitializeAllFactions()
    {
        if (factionManager == null || shipSpawner == null)
        {
            Debug.LogError("[FactionInitializer] Cannot initialize factions - missing components");
            return;
        }

        // Set up initial faction relations
        SetupInitialRelations();

        // Spawn initial ships for each faction
        foreach (var data in factionData)
        {
            if (data.Faction != FactionType.None)
            {
                SpawnInitialShips(data);
            }
        }

        Debug.Log("[FactionInitializer] All factions initialized");
    }

    private void SetupInitialRelations()
    {
        // Set up historical rivalries and alliances
        
        // Ottoman-Venetian War
        factionManager.UpdateFactionRelation(FactionType.Ottomans, FactionType.Venetians, 10f);
        
        // Pirates are generally hostile
        factionManager.UpdateFactionRelation(FactionType.Pirates, FactionType.Merchants, 20f);
        factionManager.UpdateFactionRelation(FactionType.Pirates, FactionType.RoyalNavy, 15f);
        
        // Merchants are protected by Royal Navy
        factionManager.UpdateFactionRelation(FactionType.Merchants, FactionType.RoyalNavy, 80f);

        Debug.Log("[FactionInitializer] Initial faction relations set");
    }

    private void SpawnInitialShips(FactionShipData data)
    {
        if (shipSpawner != null && data != null)
        {
            for (int i = 0; i < data.InitialShipCount; i++)
            {
                shipSpawner.SpawnShip(data.Faction, data.ShipPrefab);
            }
            Debug.Log($"[FactionInitializer] Spawned {data.InitialShipCount} ships for {data.Faction}");
        }
    }
}
