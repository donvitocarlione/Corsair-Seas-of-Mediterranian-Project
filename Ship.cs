using UnityEngine;
using System.Collections.Generic;

namespace CorsairGame
{
    public class Ship : MonoBehaviour
    {
        [SerializeField] private string shipName;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private Pirate currentOwner;

        private float currentHealth;

        public string ShipName => shipName;
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public Pirate Owner => currentOwner;

        protected virtual void Start()
        {
            currentHealth = maxHealth;
            InitializeShip();
        }

        protected virtual void InitializeShip()
        {
            if (string.IsNullOrEmpty(shipName))
            {
                shipName = $"Ship_{gameObject.GetInstanceID()}";
            }
        }

        public virtual bool SetOwner(Pirate newOwner)
        {
            if (currentOwner == newOwner) return false;

            var oldOwner = currentOwner;
            currentOwner = newOwner;

            return true;
        }

        public virtual void TakeDamage(float damage)
        {
            if (damage <= 0) return;

            currentHealth = Mathf.Max(0, currentHealth - damage);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            gameObject.SetActive(false);
        }

        public virtual void Repair(float amount)
        {
            if (amount <= 0) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }
    }
}