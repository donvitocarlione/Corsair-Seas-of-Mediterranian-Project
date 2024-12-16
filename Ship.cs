using UnityEngine;

[RequireComponent(typeof(ShipSelectionHandler))]
public class Ship : MonoBehaviour
{
    [SerializeField]
    private string shipName = "Unnamed Ship";
    [SerializeField]
    private float maxHealth = 100f;
    [SerializeField]
    private float currentHealth;
    
    // Combat Properties
    [SerializeField]
    private float attackRange = 15f;
    [SerializeField]
    private float attackDamage = 10f;
    [SerializeField]
    private float reloadTime = 1f;
    [SerializeField]
    private float ammoCount = 10f;
    [SerializeField]
    private float currentAmmo;
    [SerializeField]
    private float firingArc = 60f;

    private float lastFireTime;
    private Vector3 firingDirection;
    
    private ShipSelectionHandler selectionHandler;
    private bool isSelected;
    private bool isSinking;
    private Pirate shipOwner;

    // Original Properties
    public string ShipName => shipName;
    public string Name => shipName;
    public Pirate ShipOwner => shipOwner;
    public bool IsSelected => isSelected;
    public bool IsSinking => isSinking;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    // Combat Properties
    public float AttackRange { get => attackRange; private set => attackRange = value; }
    public float AttackDamage { get => attackDamage; private set => attackDamage = value; }
    public float ReloadTime { get => reloadTime; private set => reloadTime = value; }
    public float AmmoCount { get => ammoCount; private set => ammoCount = value; }
    public float CurrentAmmo { get => currentAmmo; private set => currentAmmo = value; }
    public float FiringArc { get => firingArc; private set => firingArc = value; }
    public Vector3 FiringDirection { get => firingDirection; private set => firingDirection = value; }
    public bool CanFire => Time.time - lastFireTime > reloadTime && currentAmmo > 0;

    private void Awake()
    {
        Debug.Log($"[Ship] Initializing {shipName}");
        currentHealth = maxHealth;
        currentAmmo = ammoCount;
        selectionHandler = GetComponent<ShipSelectionHandler>();
        
        if (selectionHandler == null)
        {
            Debug.LogError($"[Ship] No ShipSelectionHandler found on {shipName}");
        }
    }

    public void Initialize(FactionType newFaction, string newName)
    {
        Debug.Log($"[Ship] Initializing {shipName} with faction {newFaction} and name {newName}");
        SetName(newName);
    }

    public void SetName(string newName)
    {
        shipName = newName;
    }

    public void SetOwner(Pirate owner)
    {
        Debug.Log($"[Ship] Setting owner for {shipName} to {(owner != null ? owner.GetType().Name : "null")}");
        shipOwner = owner;
    }

    public void ClearOwner()
    {
        Debug.Log($"[Ship] Clearing owner for {shipName}");
        shipOwner = null;
    }

    // Combat Methods
    public void SetFiringArc(float arc)
    {
        Debug.Log($"[Ship] Setting firing arc for {shipName} to {arc}");
        firingArc = arc;
    }

    public void SetAttackRange(float range)
    {
        Debug.Log($"[Ship] Setting attack range for {shipName} to {range}");
        attackRange = range;
    }

    public void SetAttackDamage(float damage)
    {
        Debug.Log($"[Ship] Setting attack damage for {shipName} to {damage}");
        attackDamage = damage;
    }

    public void SetReloadTime(float time)
    {
        Debug.Log($"[Ship] Setting reload time for {shipName} to {time}");
        reloadTime = time;
    }

    public void SetAmmoCount(float ammo)
    {
        Debug.Log($"[Ship] Setting ammo count for {shipName} to {ammo}");
        ammoCount = ammo;
        currentAmmo = ammo;
    }

    public void SetCurrentAmmo(float ammo)
    {
        Debug.Log($"[Ship] Setting current ammo for {shipName} to {ammo}");
        currentAmmo = Mathf.Min(ammo, ammoCount);
    }

    public void SetFiringDirection(Vector3 direction)
    {
        firingDirection = direction.normalized;
    }

    public void Fire(Ship target)
    {
        if (target == null || !CanFire)
        {
            Debug.LogWarning($"[Ship] Cannot fire: target null={target == null}, canFire={CanFire}");
            return;
        }

        if (FiringSystem.Instance != null)
        {
            FiringSystem.Instance.FireProjectile(this, target);
            currentAmmo--;
            lastFireTime = Time.time;
            Debug.Log($"[Ship] {shipName} fired at {target.ShipName}. Ammo remaining: {currentAmmo}");
        }
        else
        {
            Debug.LogError("[Ship] No FiringSystem found, can't fire!");
        }
    }

    public void Reload()
    {
        Debug.Log($"[Ship] Reloading {shipName}");
        currentAmmo = ammoCount;
    }

    public bool Select()
    {
        Debug.Log($"[Ship] Attempting to select {shipName}");

        if (isSinking)
        {
            Debug.LogWarning($"[Ship] Cannot select {shipName} - ship is sinking");
            return false;
        }

        if (selectionHandler != null && selectionHandler.Select())
        {
            isSelected = true;
            Debug.Log($"[Ship] Successfully selected {shipName}");
            return true;
        }

        Debug.LogWarning($"[Ship] Failed to select {shipName}");
        return false;
    }

    public void Deselect()
    {
        Debug.Log($"[Ship] Deselecting {shipName}");
        if (selectionHandler != null)
        {
            selectionHandler.Deselect();
        }
        isSelected = false;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"[Ship] {shipName} taking {damage} damage");
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        Debug.Log($"[Ship] {shipName} health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0 && !isSinking)
        {
            StartSinking();
        }
    }

    private void StartSinking()
    {
        Debug.Log($"[Ship] {shipName} starting to sink");
        isSinking = true;
        
        if (isSelected)
        {
            Deselect();
        }

        if (shipOwner != null)
        {
            Debug.Log($"[Ship] Removing {shipName} from owner's fleet");
            shipOwner.RemoveShip(this);
            ClearOwner();
        }

        if (selectionHandler != null)
        {
            selectionHandler.enabled = false;
        }

        if (ShipManager.Instance != null)
        {
            ShipManager.Instance.OnShipDestroyed(this);
        }
    }

    public void Repair(float amount)
    {
        if (isSinking)
        {
            Debug.LogWarning($"[Ship] Cannot repair {shipName} - ship is sinking");
            return;
        }

        Debug.Log($"[Ship] Repairing {shipName} for {amount}");
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"[Ship] {shipName} health after repair: {currentHealth}/{maxHealth}");
    }

    private void OnValidate()
    {
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        if (currentAmmo > ammoCount)
        {
            currentAmmo = ammoCount;
        }
    }
}