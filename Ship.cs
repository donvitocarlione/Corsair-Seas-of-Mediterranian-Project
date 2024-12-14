using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using CSM.Base;

public class Ship : SeaEntityBase
{
    [Header("Ship Properties")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float sinkingThreshold = 20f;
    [SerializeField] protected GameObject waterSplashPrefab;
    [SerializeField] protected float waterFloodRate = 0.1f;

    protected float currentHealth;
    protected bool isSelected;
    protected bool isSinking;
    protected IShipOwner owner;

    protected Rigidbody shipRigidbody;
    protected Buoyancy buoyancyComponent;
    protected ShipMovement movementComponent;

    public event UnityAction OnShipDestroyed;

    public float Health => currentHealth;
    public bool IsSelected => isSelected;
    public bool IsSinking => isSinking;
    public IShipOwner ShipOwner => owner;
    public string ShipName => Name;

    protected virtual void Awake()
    {
        Debug.Log($"[Ship] Awake called on {gameObject.name}");
        shipRigidbody = GetComponent<Rigidbody>();
        buoyancyComponent = GetComponent<Buoyancy>();
        movementComponent = GetComponent<ShipMovement>();
        currentHealth = maxHealth;

        var collider = GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogError($"[Ship] No Collider found on {gameObject.name}");
        }

        var selectionHandler = GetComponent<ShipSelectionHandler>();
        if (selectionHandler == null)
        {
            Debug.LogError($"[Ship] No ShipSelectionHandler found on {gameObject.name}");
        }

        Debug.Log($"[Ship] Components check for {gameObject.name}:\n" +
                  $"- Rigidbody: {shipRigidbody != null}\n" +
                  $"- Buoyancy: {buoyancyComponent != null}\n" +
                  $"- Movement: {movementComponent != null}\n" +
                  $"- Collider: {collider != null}\n" +
                  $"- SelectionHandler: {selectionHandler != null}");
    }

    protected override void Start()
    {
        Debug.Log($"[Ship] Start called on {gameObject.name}");
        base.Start();
    }

    public virtual void Initialize(FactionType newFaction, string newName)
    {
        Debug.Log($"[Ship] Initializing {gameObject.name} with faction {newFaction} and name {newName}");
        SetFaction(newFaction);
        SetName(newName);
    }

    public virtual void SetOwner(IShipOwner newOwner)
    {
        Debug.Log($"[Ship] Setting owner for {gameObject.name} to {(newOwner != null ? newOwner.GetType().Name : "null")}");
        if (owner != null && !ReferenceEquals(owner, newOwner))
        {
            owner.RemoveShip(this);
        }

        owner = newOwner;
    }

    public virtual void ClearOwner()
    {
        Debug.Log($"[Ship] Clearing owner for {gameObject.name}");
        owner = null;
    }

    public virtual void Select()
    {
        Debug.Log($"[Ship] Selecting {gameObject.name}");
        isSelected = true;
    }

    public virtual void Deselect()
    {
        Debug.Log($"[Ship] Deselecting {gameObject.name}");
        isSelected = false;
    }

    public virtual void TakeDamage(float damage)
    {
        if (isSinking) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (currentHealth <= sinkingThreshold && !isSinking)
        {
            StartSinking();
        }
    }

    protected virtual void StartSinking()
    {
        isSinking = true;
        StartCoroutine(SinkingRoutine());
    }

    protected virtual IEnumerator SinkingRoutine()
    {
        WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

        while (currentHealth > 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - Time.deltaTime);

            if (waterSplashPrefab != null && 
                Random.value < waterFloodRate * Time.deltaTime && 
                buoyancyComponent != null)
            {
                Vector3 splashPosition = transform.position + Random.insideUnitSphere * 2f;
                splashPosition.y = buoyancyComponent.WaterLevel;
                Instantiate(waterSplashPrefab, splashPosition, Quaternion.identity);
            }

            yield return waitForEndOfFrame;
        }

        HandleShipDestroyed();
    }

    protected virtual void HandleShipDestroyed()
    {
        if (shipRigidbody != null) shipRigidbody.isKinematic = true;
        
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
        
        if (isSelected) Deselect();
        ClearOwner();
        
        OnShipDestroyed?.Invoke();
        Debug.Log($"Ship {Name} has been destroyed!");
        
        ShipManager.Instance?.OnShipDestroyed(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnShipDestroyed = null;
    }

    protected virtual void OnValidate()
    {
        if (sinkingThreshold > maxHealth)
        {
            sinkingThreshold = maxHealth * 0.2f;
            Debug.LogWarning($"Adjusted sinking threshold to {sinkingThreshold}");
        }

        if (maxHealth <= 0)
        {
            maxHealth = 100f;
            Debug.LogWarning("Adjusted max health to default value (100)");
        }
    }
}