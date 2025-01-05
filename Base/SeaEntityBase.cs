// SeaEntityBase.cs
using UnityEngine;
using System;

namespace CSM.Base
{
    public class SeaEntityBase : MonoBehaviour, IOwnable
    {
        #region Fields and Properties

       [SerializeField] protected string _entityName;
       [SerializeField] protected float _maxHealth = 100f;

        private float _currentHealth;
        private bool _isInitialized;
        private IEntityOwner _owner;

        public bool IsAlive => _currentHealth > 0;
        public virtual string EntityName
        {
            get => _entityName;
            protected set => _entityName = value;
        }
        public float MaxHealth => _maxHealth;
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
                    HandleOwnerChanged(oldOwner, _owner);
                    OnOwnerChanged?.Invoke(_owner);
                }
            }
        }

        public event Action<IEntityOwner> OnOwnerChanged;

        #endregion

         #region Unity Lifecycle Methods

       protected virtual void Awake()
       {
           _currentHealth = _maxHealth;
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
               Debug.LogWarning($"[{GetType().Name}] Attempting to initialize {name} multiple times");
                return;
           }
           _entityName = name;
           Owner = owner;
           Initialize();
        }
        
        public virtual bool SetName(string newName)
        {
            _entityName = newName;
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
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);

            if (_currentHealth != oldHealth)
            {
                OnHeal(_currentHealth - oldHealth);
            }
        }

        public bool IsOwnedBy(IEntityOwner controller)
        {
            return Owner == controller;
        }

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
           _currentHealth = _maxHealth;
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
            Debug.Log($"[{GetType().Name}] {_entityName} owner changed from {oldOwner?.OwnerName ?? "none"} to {newOwner?.OwnerName ?? "none"}");
       }
        protected virtual void OnInitialized()
       {
           Debug.Log($"[{GetType().Name}] {_entityName} initialized");
       }

        protected virtual void OnCleanup()
       {
            Debug.Log($"[{GetType().Name}] {_entityName} cleaned up");
        }

        protected virtual void OnTakeDamage(float damage, SeaEntityBase attacker)
       {
           Debug.Log($"[{GetType().Name}] {_entityName} took {damage} damage from {attacker?.EntityName ?? "unknown"}");
        }

        protected virtual void OnHeal(float amount)
       {
          Debug.Log($"[{GetType().Name}] {_entityName} healed for {amount}");
       }

       protected virtual void OnDeath(SeaEntityBase killer)
       {
          Debug.Log($"[{GetType().Name}] {_entityName} was killed by {killer?.EntityName ?? "unknown"}");
        }

        #endregion
    }
}