using UnityEngine;
using CSM.Base;

namespace CorsairGame
{
    public class Ship : MonoBehaviour
    {
        [SerializeField] private Pirate owner;
        [SerializeField] private string shipName;
        [SerializeField] private float maxHealth = 100f;
        
        private float currentHealth;
        
        public Pirate Owner => owner;
        public string ShipName => shipName;
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        
        private void Start()
        {
            currentHealth = maxHealth;
        }
        
        public bool SetOwner(Pirate newOwner)
        {
            if (owner == newOwner) return false;
            
            owner = newOwner;
            return true;
        }
        
        public void TakeDamage(float damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        private void Die()
        {
            // Implement ship destruction logic
            gameObject.SetActive(false);
        }
    }
}