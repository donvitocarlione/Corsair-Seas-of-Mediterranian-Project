using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using CSM.Base;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(ShipSelectionHandler))]
[RequireComponent(typeof(Buoyancy))]
[RequireComponent(typeof(ShipMovement))]
public class Ship : SeaEntityBase, IOwnable
{
    [Header("Ship Properties")]
    [SerializeField] protected float sinkingThreshold = 20f;
    [SerializeField] protected GameObject waterSplashPrefab;
    [SerializeField] protected float waterFloodRate = 0.1f;

    protected float currentHealth;
    protected bool isSelected;
    protected bool isSinking;
    
     // Use IEntityOwner instead of IShipOwner
    private IEntityOwner _owner;

    protected Rigidbody shipRigidbody;
    protected Buoyancy buoyancyComponent;
    protected ShipMovement movementComponent;

    public event UnityAction OnShipDestroyed;

    public float Health => currentHealth;
    public bool IsSelected => isSelected;
    public bool IsSinking => isSinking;

    // Use 'new' keyword to acknowledge hiding and to use own implementation:
    public new IEntityOwner Owner => _owner;
    public new FactionType Faction { get; private set; }

     public new event System.Action<IEntityOwner> OnOwnerChanged;


    public override string Name
    {
        get => EntityName;
        protected set => EntityName = value;
    }

     private bool isInitialized = false;
     private bool startCalled = false;


    protected override void Awake()
    {
        base.Awake();
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
        startCalled = true;
        if (isInitialized)
        {
            OnInitializedStart();
        }
         Debug.Log($"[Ship] Start called on {gameObject.name}");
        base.Start();
    }
   
    public virtual void SetName(string newName)
    {
        EntityName = newName;
        Debug.Log($"[Ship] Name set to {newName} for {gameObject.name}");
    }
     public override void Initialize(string shipName, FactionType faction, IEntityOwner shipOwner)
    {
        if (isInitialized)
        {
             Debug.LogWarning($"[Ship] Ship {shipName} is already initialized");
            return;
        }
        if (shipOwner == null)
        {
             Debug.LogError($"[Ship] Cannot initialize {shipName} - no owner provided");
            return;
        }
        if (shipOwner.Faction != faction)
        {
            Debug.LogError($"[Ship] Owner faction {shipOwner.Faction} doesn't match ship faction {faction}");
             return;
        }
        SetName(shipName);
        SetFaction(faction);
        if (!SetOwner(shipOwner))
        {
            Debug.LogError($"[Ship] Failed to set owner for {shipName}");
            return;
        }
         if (FactionManager.Instance != null)
        {
            FactionManager.Instance.RegisterShip(faction, this);
        }

         isInitialized = true;
       Debug.Log($"[Ship] {shipName} initialized successfully with faction {faction} and owner {shipOwner.GetType().Name}");
          if (startCalled)
        {
            OnInitializedStart();
        }

    }
      protected virtual void OnInitializedStart()
    {
        // Put your Start logic here
           Debug.Log($"[Ship] Start called for {Name} after initialization");
    }

    //Use override instead of hiding
      public override void SetFaction(FactionType faction)
    {
        base.SetFaction(faction);
         // Additional faction-specific logic here
        Debug.Log($"[Ship] Faction set to {faction} for {gameObject.name}");
    }
    
    // Implement IOwnable
     public bool SetOwner(IEntityOwner newOwner)
    {
         if (newOwner == _owner) return true; // Already set
        
        // Validate new owner's faction matches ship's faction
        if (newOwner != null && newOwner.Faction != Faction)
        {
           Debug.LogError($"[Ship] Cannot set owner - faction mismatch. Ship:{Faction}, Owner:{newOwner.Faction}");
           return false;
        }

        // Handle old owner cleanup
        if (_owner != null)
        {
            if (_owner is IShipOwner oldShipOwner)
            {
                oldShipOwner.RemoveShip(this);
            }
        }

        _owner = newOwner;

        // Handle new owner setup
        if (_owner is IShipOwner shipOwner)
        {
            shipOwner.AddShip(this);
        }

         Debug.Log($"[Ship] {Name} owner changed to {(_owner != null ? _owner.GetType().Name : "none")}");
         // Use new event:
        OnOwnerChanged?.Invoke(_owner);
         return true;
    }

    public virtual void ClearOwner()
    {
        Debug.Log($"[Ship] Clearing owner for {gameObject.name}");
         if (_owner is IShipOwner shipOwner)
        {
           shipOwner.RemoveShip(this);
        }
        _owner = null;
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