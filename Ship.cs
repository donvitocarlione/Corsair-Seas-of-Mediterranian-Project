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
    [SerializeField] private Transform[] firingPoints;

    private float lastFireTime;
    private ShipSelectionHandler selectionHandler;
    private bool isSelected;
    private bool isSinking;
    private Pirate shipOwner;

    public string ShipName => shipName;
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
        currentHealth = maxHealth;
        currentAmmo = ammoCount;
        selectionHandler = GetComponent<ShipSelectionHandler>();

        if (selectionHandler == null)
        {
            Debug.LogError($"[Ship] No ShipSelectionHandler found on {shipName}");
        }
        
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
                Debug.LogWarning($"[Ship] No firing points found on {shipName}. Creating default.");
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
        firingPoint.transform.localPosition = new Vector3(0, 1f, 2f);
        firingPoint.transform.localRotation = Quaternion.identity;

        firingPoints = new Transform[] { firingPoint.transform };
    }

    public Transform[] GetFiringPoints() => firingPoints;

    public void Initialize(string newName)
    {
        SetName(newName);
    }

    public void SetName(string newName)
    {
        shipName = newName;
    }

    public void SetOwner(Pirate owner)
    {
        shipOwner = owner;
    }

    public void ClearOwner()
    {
        shipOwner = null;
    }

    public bool Select()
    {
        if (isSinking || selectionHandler == null) return false;

        if (selectionHandler.Select())
        {
            isSelected = true;
            return true;
        }
        return false;
    }

    public void Deselect()
    {
        if (selectionHandler != null)
        {
            selectionHandler.Deselect();
        }
        isSelected = false;
    }

    public void Fire(Ship target)
    {
        if (target == null || !CanFire) return;

        if (FiringSystem.Instance != null)
        {
            FiringSystem.Instance.FireProjectile(this, target);
            currentAmmo--;
            lastFireTime = Time.time;
        }
    }

    public void Reload()
    {
        currentAmmo = ammoCount;
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (currentHealth <= 0 && !isSinking)
        {
            StartSinking();
        }
    }

    private void StartSinking()
    {
        isSinking = true;

        if (isSelected)
        {
            Deselect();
        }

        if (shipOwner != null)
        {
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
        if (isSinking) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    private void OnValidate()
    {
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        if (currentAmmo > ammoCount) currentAmmo = ammoCount;
    }

    private void OnDrawGizmosSelected()
    {
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

        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

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