using UnityEngine;

[RequireComponent(typeof(ShipSelectionHandler))]
public class Ship : MonoBehaviour
{
    [Header("Ship Properties")]
    [SerializeField] private string shipName = "Unnamed Ship";
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Combat Properties")]
    [SerializeField] private float attackRange = 15f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float reloadTime = 1f;
    [SerializeField] private float ammoCount = 10f;
    [SerializeField] private float currentAmmo;
    [SerializeField] private float firingArc = 60f;
    [SerializeField] private Transform[] firingPoints;  // Array of firing points

    private float lastFireTime;
    private ShipSelectionHandler selectionHandler;
    private bool isSelected;
    private bool isSinking;
    private Pirate shipOwner;

    public string ShipName => shipName;
    public string Name => shipName;
    public Pirate ShipOwner => shipOwner;
    public bool IsSelected => isSelected;
    public bool IsSinking => isSinking;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float AttackRange => attackRange;
    public float AttackDamage => attackDamage;
    public float ReloadTime => reloadTime;
    public float AmmoCount => ammoCount;
    public float CurrentAmmo => currentAmmo;
    public float FiringArc => firingArc;
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

        // Auto-find firing points if none are assigned
        if (firingPoints == null || firingPoints.Length == 0)
        {
            Transform firingPointsParent = transform.Find("FiringPoints");
            if (firingPointsParent != null)
            {
                firingPoints = new Transform[firingPointsParent.childCount];
                for (int i = 0; i < firingPointsParent.childCount; i++)
                {
                    firingPoints[i] = firingPointsParent.GetChild(i);
                }
            }
            else
            {
                Debug.LogWarning($"[Ship] No firing points found on {shipName}. Creating default firing point.");
                CreateDefaultFiringPoint();
            }
        }
    }

    private void CreateDefaultFiringPoint()
    {
        GameObject firingPointsParent = new GameObject("FiringPoints");
        firingPointsParent.transform.SetParent(transform);
        firingPointsParent.transform.localPosition = Vector3.zero;

        GameObject firingPoint = new GameObject("FiringPoint");
        firingPoint.transform.SetParent(firingPointsParent.transform);
        firingPoint.transform.localPosition = new Vector3(0, 1f, 2f); // Adjust position as needed
        firingPoint.transform.localRotation = Quaternion.identity;

        firingPoints = new Transform[] { firingPoint.transform };
    }

    public Transform[] GetFiringPoints()
    {
        return firingPoints;
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

    private void OnDrawGizmosSelected()
    {
        // Draw firing points
        if (firingPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform firingPoint in firingPoints)
            {
                if (firingPoint != null)
                {
                    Gizmos.DrawWireSphere(firingPoint.position, 0.3f);
                    Gizmos.DrawLine(firingPoint.position, firingPoint.position + firingPoint.forward * 2f);
                }
            }
        }

        // Draw attack range
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw firing arc
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Vector3 forward = transform.forward;
        float radius = attackRange;
        float angleStep = 5f;

        for (float angle = -firingArc * 0.5f; angle <= firingArc * 0.5f; angle += angleStep)
        {
            Vector3 direction = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 nextDirection = Quaternion.Euler(0, angle + angleStep, 0) * forward;

            Vector3 point1 = transform.position;
            Vector3 point2 = transform.position + direction * radius;
            Vector3 point3 = transform.position + nextDirection * radius;

            Gizmos.DrawLine(point1, point2);
            Gizmos.DrawLine(point2, point3);
        }
    }
}