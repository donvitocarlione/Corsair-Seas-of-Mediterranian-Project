using UnityEngine;

namespace CorsairGame
{
    public class Pirate : MonoBehaviour
    {
        public FactionType faction;
        public string pirateName;
        public int level = 1;
        public float health = 100f;
        public float maxHealth = 100f;
        
        // Combat stats
        public float attackPower = 10f;
        public float defense = 5f;
        public float accuracy = 0.7f;
        public float criticalChance = 0.1f;
        
        // Skills and progression
        public int experience = 0;
        public int skillPoints = 0;
        
        // Equipment slots
        public string weaponEquipped;
        public string armorEquipped;
        public string accessoryEquipped;
        
        private void Start()
        {
            health = maxHealth;
        }
        
        public void TakeDamage(float damage)
        {
            float actualDamage = Mathf.Max(0, damage - defense);
            health = Mathf.Max(0, health - actualDamage);
            
            if (health <= 0)
            {
                Die();
            }
        }
        
        private void Die()
        {
            // Implement death logic here
            gameObject.SetActive(false);
        }
        
        public void GainExperience(int amount)
        {
            experience += amount;
            CheckLevelUp();
        }
        
        private void CheckLevelUp()
        {
            int experienceNeeded = level * 100; // Simple level-up formula
            if (experience >= experienceNeeded)
            {
                LevelUp();
            }
        }
        
        private void LevelUp()
        {
            level++;
            skillPoints++;
            maxHealth += 10;
            health = maxHealth;
            attackPower += 2;
            defense += 1;
            
            // Reset experience for next level
            experience = 0;
        }
    }
}