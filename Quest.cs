using UnityEngine;

public class Quest : MonoBehaviour
{
    public string questName;
    public string description;
    public bool isCompleted;
    public int experienceReward;
    public int goldReward;

    public virtual void CompleteQuest()
    {
        isCompleted = true;
        // Add reward distribution logic here
    }
}
