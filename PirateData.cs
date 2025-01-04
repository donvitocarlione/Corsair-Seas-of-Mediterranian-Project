using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PirateData", menuName = "Corsair/Pirate Data")]
public class PirateData : ScriptableObject
{
    public string pirateName;
    public PirateRank rank;
    public bool isPlayer;

    // Add ship preferences
    public List<GameObject> preferredShipPrefabs;
    public int maxShips = 3;
    public float spawnRadius = 100f;
    public Vector3 spawnArea;
}