// File: PirateData.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PirateData", menuName = "Corsair/Pirate Data")]
public class PirateData : ScriptableObject
{
    // Basic pirate information
    public string pirateName;
    public PirateRank rank;
    public int initialGold;
    public int initialCrew;
}