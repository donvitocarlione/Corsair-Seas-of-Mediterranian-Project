using UnityEngine;

public class Ship : MonoBehaviour
{
    [SerializeField] protected string shipName;
    public string Name => shipName;
    public string ShipName => shipName; // Additional property for backward compatibility

    [SerializeField] protected float attackRange = 20f;
    public float AttackRange => attackRange;

    [SerializeField] protected float firingArc = 60f;
    public float FiringArc => firingArc;

    [SerializeField] protected float attackDamage = 10f;
    public float AttackDamage => attackDamage;

    [SerializeField] protected int maxAmmo = 10;
    [SerializeField] protected float reloadTime = 3f;
    protected float reloadTimer;
    protected int currentAmmo;
    public int CurrentAmmo => currentAmmo;

    [SerializeField] protected Transform[] firingPoints;
    
    public bool IsSinking { get; protected set; } = false;
    public bool IsSelected { get; protected set; } = false;

    protected ShipMovement movement;
    protected FiringSystem firingSystem;
    protected Pirate shipOwner;

    public ShipMovement Movement => movement;
    public FiringSystem FiringSystem => firingSystem;
    public Pirate ShipOwner => shipOwner;

    protected virtual void Awake()
    {
        movement = GetComponent<ShipMovement>();
        firingSystem = GetComponent<FiringSystem>();
        shipOwner = GetComponent<Pirate>();
        currentAmmo = maxAmmo;
    }

    protected virtual void Start()
    {
        if (string.IsNullOrEmpty(shipName))
        {
            shipName = gameObject.name;
        }
    }

    protected virtual void Update()
    {
        if (currentAmmo < maxAmmo)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0)
            {
                currentAmmo++;
                reloadTimer = reloadTime;
            }
        }
    }

    public virtual void TakeDamage(float amount)
    {
        // Implement damage logic in derived classes
        Debug.Log($"[Ship] {shipName} took {amount} damage!");
    }

    public virtual Transform[] GetFiringPoints()
    {
        return firingPoints;
    }

    public bool CanFire => CurrentAmmo > 0;

    public virtual void SetSelected(bool selected)
    {
        IsSelected = selected;
    }

    public virtual void StartSinking()
    {
        IsSinking = true;
        // Derived classes should implement specific sinking behavior
    }

    public virtual void ConsumeAmmo()
    {
        if (currentAmmo > 0)
        {
            currentAmmo--;
            reloadTimer = reloadTime;
        }
    }
}