using UnityEngine;

namespace CSM.Base
{
    public abstract class SeaEntityBase : MonoBehaviour
    {
        #region Fields and Properties

        [SerializeField] protected string entityName;
        [SerializeField] protected float maxHealth = 100f;

        private float _currentHealth;
        private FactionType _faction;
        private bool _isInitialized;

        // Modified EntityName to be settable
        public virtual string EntityName
        {
            get => entityName;
            protected set => entityName = value;
        }

        public float MaxHealth => maxHealth;
        public float CurrentHealth => _currentHealth;

        public FactionType Faction
        {
            get => _faction;
            protected set
            {
                if (_faction != value)
                {
                    var oldFaction = _faction;
                    _faction = value;
                    OnFactionChanged(oldFaction, _faction);
                }
            }
        }

        public bool IsAlive => _currentHealth > 0;
        public bool IsInitialized => _isInitialized;

        public virtual string Name { get; protected set; }


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

        public virtual void SetFaction(FactionType factionType)
        {
            Faction = factionType;
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

        protected virtual void OnInitialized()
        {
            Debug.Log($"[{GetType().Name}] {entityName} initialized");
        }

        protected virtual void OnCleanup()
        {
            Debug.Log($"[{GetType().Name}] {entityName} cleaned up");
        }

        protected virtual void OnFactionChanged(FactionType oldFaction, FactionType newFaction)
        {
            Debug.Log($"[{GetType().Name}] {entityName} faction changed from {oldFaction} to {newFaction}");
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