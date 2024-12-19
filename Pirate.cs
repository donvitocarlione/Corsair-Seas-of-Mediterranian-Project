using UnityEngine;
using System.Collections.Generic;
using CSM.Base;

namespace CorsairGame
{
    public class Pirate : SeaEntityBase, IShipOwner
    {
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
        
        protected override void Start()
        {
            base.Start();
            health = maxHealth;
        }
        
        public virtual void TakeDamage(float damage)
        {
            float actualDamage = Mathf.Max(0, damage - defense);
            health = Mathf.Max(0, health - actualDamage);
            
            if (health <= 0)
            {
                Die();
            }
        }
        
        protected virtual void Die()
        {
            // Implement death logic here
            gameObject.SetActive(false);
        }
        
        public virtual void GainExperience(int amount)
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public virtual void AddShip(Ship ship)
        {
            throw new System.NotImplementedException();
        }

        public virtual void RemoveShip(Ship ship)
        {
           throw new System.NotImplementedException();
        }

        public virtual void SelectShip(Ship ship)
        {
           throw new System.NotImplementedException();
        }

        public virtual List<Ship> GetOwnedShips()
        {
          throw new System.NotImplementedException();
        }
    }
}