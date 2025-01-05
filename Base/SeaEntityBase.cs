using UnityEngine;
using System;

namespace CSM.Base
{
    public class SeaEntityBase : MonoBehaviour, IOwnable
    {
        #region Fields and Properties

        [SerializeField] protected string entityName;
        [SerializeField] protected float maxHealth = 100f;

        private float _currentHealth;
        private bool _isInitialized;
        private IEntityOwner _owner;

        public bool IsAlive => CurrentHealth > 0;
        public virtual string EntityName
        {
            get => entityName;
            protected set => entityName = value;
        }

        public float MaxHealth => maxHealth;
        public float CurrentHealth => _currentHealth;

        public virtual string Name
        {
            get => EntityName;
            protected set => EntityName = value;
        }
        public IEntityOwner Owner
        {
            get => _owner;
            protected set
            {
                if (_owner != value)
                {
                    var oldOwner = _owner;
                    _owner = value;
                     HandleOwnerChanged(oldOwner, _owner); // Call method with two parameters
                    OnOwnerChanged?.Invoke(_owner);  // invoke with new owner
                }
            }
        }

        // Match the interface definition
        public event Action<IEntityOwner> OnOwnerChanged;

        #endregion

         #region Unity Lifecycle Methods

        protected virtual void Awake()
        {
            _currentHealth = maxHealth;
        }

        protected virtual void Start()
        {
            Initialize();
        }

        protected virtual void OnDestroy()
        {
            Cleanup();
        }

        #endregion

        #region Public Methods

        public virtual void Initialize(string name, IEntityOwner owner)
        {
          if (_isInitialized)
            {
                Debug.LogWarning($"[{GetType().Name}] Attempting to initialize {entityName} multiple times");
                return;
           }
           EntityName = name;
           Owner = owner;
           Initialize();
        }

        public virtual bool SetName(string newName)
        {
           EntityName = newName;
            Debug.Log($"[{GetType().Name}] Name set to {newName}");
             return true;
        }


        public virtual void TakeDamage(float damage, SeaEntityBase attacker)
        {
            if (!IsAlive) return;

            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            OnTakeDamage(damage, attacker);

            if (_currentHealth <= 0)
            {
                Die(attacker);
            }
        }

        public virtual void Heal(float amount)
        {
            if (!IsAlive) return;

            float oldHealth = _currentHealth;
            _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);

            if (_currentHealth != oldHealth)
            {
                OnHeal(_currentHealth - oldHealth);
            }
        }

        public bool IsOwnedBy(IEntityOwner controller)
        {
           return Owner == controller;
        }

        // Implement IOwnable's SetOwner method
        public virtual bool SetOwner(IEntityOwner newOwner)
        {
           Owner = newOwner;
            return true;
       }

         public virtual void ClearOwner()
       {
          Owner = null;
       }
        #endregion

        #region Protected Methods

        protected virtual void Initialize()
        {
            if (_isInitialized) return;

            _currentHealth = maxHealth;
            _isInitialized = true;

             OnInitialized();
        }

       protected virtual void Cleanup()
        {
             if (!_isInitialized) return;

            _isInitialized = false;
            OnCleanup();
        }

       protected virtual void Die(SeaEntityBase killer)
        {
          if (!IsAlive) return;

            _currentHealth = 0;
           OnDeath(killer);
        }

       #endregion

      #region Virtual Event Methods

        protected virtual void HandleOwnerChanged(IEntityOwner oldOwner, IEntityOwner newOwner)
        {
             Debug.Log($"[{GetType().Name}] {entityName} owner changed from {oldOwner?.OwnerName ?? "none"} to {newOwner?.OwnerName ?? "none"}");
        }
         protected virtual void OnInitialized()
        {
            Debug.Log($"[{GetType().Name}] {entityName} initialized");
        }

        protected virtual void OnCleanup()
        {
            Debug.Log($"[{GetType().Name}] {entityName} cleaned up");
        }

        protected virtual void OnTakeDamage(float damage, SeaEntityBase attacker)
       {
          Debug.Log($"[{GetType().Name}] {entityName} took {damage} damage from {attacker?.EntityName ?? "unknown"}");
       }

        protected virtual void OnHeal(float amount)
        {
           Debug.Log($"[{GetType().Name}] {entityName} healed for {amount}");
       }

        protected virtual void OnDeath(SeaEntityBase killer)
        {
           Debug.Log($"[{GetType().Name}] {entityName} was killed by {killer?.EntityName ?? "unknown"}");
       }

        #endregion
    }
}