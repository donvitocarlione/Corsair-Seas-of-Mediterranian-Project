using UnityEngine;
using System.Collections.Generic;

public class PirateManager : MonoBehaviour
{
    public static PirateManager Instance { get; private set; }

    private Dictionary<string, Pirate> pirates = new Dictionary<string, Pirate>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Pirate CreatePirate(PirateData pirateData)
    {
        // Instantiate pirate prefab and set data
        GameObject pirateGameObject = new GameObject("Pirate_" + pirateData.pirateName);
        Pirate pirateComponent = pirateGameObject.AddComponent<Pirate>();
        pirateComponent.SetName(pirateData.pirateName);
        pirateComponent.SetRank(pirateData.rank);

        //Store the pirate and return.
        pirates.Add(pirateComponent.EntityName, pirateComponent);
        return pirateComponent;
    }

    public Pirate GetPirate(string pirateId)
    {
         pirates.TryGetValue(pirateId, out Pirate pirate);
         return pirate;
    }

    public void RemovePirate(string pirateId)
    {
        pirates.Remove(pirateId);
    }
}