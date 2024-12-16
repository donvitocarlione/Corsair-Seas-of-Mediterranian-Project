using UnityEngine;

public class ShipMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float stoppingDistance = 1f;
    
    [Header("Movement Smoothing")]
    [SerializeField] private float rotationSmoothTime = 0.3f;
    [SerializeField] private float velocitySmoothTime = 0.3f;
    [SerializeField] private float heightSmoothTime = 0.5f;
    
    [Header("Water Interaction")]
    [SerializeField] private float waterLevel = 0f;
    [SerializeField] private float buoyancyOffset = 0.5f;
    [SerializeField] private float waterDrag = 0.95f;
    [SerializeField] private float stoppingThreshold = 0.1f;

    [Header("Combat Movement")]
    [SerializeField] private float combatStoppingDistance = 10f;
    [SerializeField] private float optimalCombatDistance = 12f;
    [SerializeField] private float repositionThreshold = 5f;
    
    private Vector3 targetPosition;
    private float currentSpeed;
    private bool isMoving;
    private Rigidbody rb;
    private Buoyancy buoyancy;
    private Ship targetShip;
    private Ship ownShip;
    
    // Smoothing variables
    private Vector3 currentVelocity;
    private float rotationVelocity;
    private float heightVelocity;
    private float lastRepositionTime;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        buoyancy = GetComponent<Buoyancy>();
        ownShip = GetComponent<Ship>();
        
        if (rb == null)
        {
            Debug.LogError($"[ShipMovement] No Rigidbody found on {gameObject.name}");
            enabled = false;
            return;
        }
        
        Vector3 startPos = transform.position;
        startPos.y = waterLevel + buoyancyOffset;
        transform.position = startPos;
        targetPosition = startPos;
        
        rb.drag = waterDrag;
        rb.angularDrag = waterDrag;
        rb.useGravity = false;
        
        Debug.Log($"[ShipMovement] Initialized on {gameObject.name}");
    }
    
    private void FixedUpdate()
    {
        if (isMoving)
        {
            if (targetShip != null && !targetShip.IsSinking)
            {
                UpdateCombatMovement();
            }
            else
            {
                MoveTowardsTarget();
            }
        }
        else
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 5f);
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 5f);
        }
        
        MaintainWaterLevel();
    }
    
    private void MaintainWaterLevel()
    {
        float targetHeight = waterLevel + buoyancyOffset;
        Vector3 newPosition = rb.position;
        newPosition.y = Mathf.SmoothDamp(rb.position.y, targetHeight, ref heightVelocity, heightSmoothTime);
        rb.MovePosition(newPosition);
    }
    
    public void SetTargetPosition(Vector3 position)
    {
        position.y = waterLevel + buoyancyOffset;
        targetPosition = position;
        targetShip = null;
        isMoving = true;
        currentSpeed = rb.linearVelocity.magnitude;
        Debug.Log($"[ShipMovement] Set target position for {gameObject.name} to {position}");
    }

    public void SetTargetPosition(Vector3 position, Ship target)
    {
        position.y = waterLevel + buoyancyOffset;
        targetPosition = position;
        targetShip = target;
        isMoving = true;
        currentSpeed = rb.linearVelocity.magnitude;
        Debug.Log($"[ShipMovement] Set combat target for {gameObject.name} to {target.ShipName}");

        // Set combat target in the combat system
        if (CombatSystem.Instance != null)
        {
            CombatSystem.Instance.SetCombatTarget(ownShip, target);
        }
    }
