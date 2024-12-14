using UnityEngine;

public enum ShipState
{
    Idle,
    Moving,
    Turning,
    Stopping
}

public class ShipMovement : MonoBehaviour
{
    [Header("Ship Characteristics")]
    public float mass = 1000f;
    public float windResistance = 1f;
    public float waterResistance = 2f;
    public float minSpeedForTurning = 0.1f;

    [Header("Movement Settings")]
    public float baseSpeed = 5f;
    public float baseTurnSpeed = 90f;
    public float acceleration = 1f;
    public float deceleration = 0.5f;
    public float stoppingDistance = 1f;

    [Header("Movement Modifiers")]
    public float speedMultiplier = 1f;
    public float turnSpeedMultiplier = 1f;
    public bool isMoving { get; private set; }

    private Rigidbody rb;
    private Vector3 targetPosition;
    private Vector3 currentVelocity;
    private Quaternion targetRotation;
    private ShipState currentState = ShipState.Idle;
    private float currentSpeed = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Ship needs a Rigidbody!");
            enabled = false;
            return;
        }

        InitializePhysics();
    }

    private void InitializePhysics()
    {
        rb.useGravity = true;
        rb.mass = mass;
        rb.linearDamping = waterResistance;
        rb.angularDamping = windResistance;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | 
                        RigidbodyConstraints.FreezeRotationZ;
    }

    public void ApplyNavigationBonus(float bonus)
    {
        speedMultiplier = bonus;
        turnSpeedMultiplier = Mathf.Lerp(1f, bonus, 0.5f);
    }

    public void ResetNavigationBonus()
    {
        speedMultiplier = 1f;
        turnSpeedMultiplier = 1f;
    }

    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
        targetPosition.y = transform.position.y;
        isMoving = true;
        currentState = ShipState.Moving;

        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        if (directionToTarget != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
            targetRotation = Quaternion.Euler(0, targetAngle, 0);
            currentState = ShipState.Turning;
        }
    }

    public void StopMovement()
    {
        targetPosition = Vector3.zero;
        isMoving = false;
        currentVelocity = Vector3.zero;
        currentState = ShipState.Stopping;
        currentSpeed = 0f;
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case ShipState.Moving:
            case ShipState.Turning:
                RotateTowardsTarget();
                MoveTowardsTarget();
                break;
                
            case ShipState.Stopping:
                ApplyBraking();
                break;
        }
        
        ApplyWaterPhysics();
        ApplyWindPhysics();
    }

    private void RotateTowardsTarget()
    {
        if (currentSpeed < minSpeedForTurning) return;
        
        float currentTurnSpeed = baseTurnSpeed * turnSpeedMultiplier;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 
            currentTurnSpeed * Time.fixedDeltaTime);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
        {
            currentState = ShipState.Moving;
        }
    }

    private void MoveTowardsTarget()
    {
        if (!isMoving) return;
        
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        
        if (distanceToTarget <= stoppingDistance)
        {
            StopMovement();
            return;
        }
        
        // Calculate desired speed based on distance
        float desiredSpeed = Mathf.Min(
            baseSpeed * speedMultiplier,
            Mathf.Sqrt(2f * acceleration * distanceToTarget)
        );
        
        // Apply smooth acceleration/deceleration
        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            desiredSpeed,
            (desiredSpeed > currentSpeed ? acceleration : deceleration) * Time.fixedDeltaTime
        );
        
        // Calculate movement direction
        Vector3 moveDirection = transform.forward;
        Vector3 targetVelocity = moveDirection * currentSpeed;
        
        // Apply movement
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime);
    }

    private void ApplyWaterPhysics()
    {
        // Apply water resistance based on speed
        float resistance = waterResistance * rb.linearVelocity.magnitude * rb.linearVelocity.magnitude;
        rb.AddForce(-rb.linearVelocity.normalized * resistance);
        
        // Apply wave effects
        float waveHeight = Mathf.Sin(Time.time * 0.5f) * 0.1f;
        rb.AddForce(Vector3.up * waveHeight, ForceMode.Acceleration);
    }
    
    private void ApplyWindPhysics()
    {
        // Simplified wind effect (you can integrate with a weather system later)
        Vector3 windDirection = new Vector3(Mathf.Sin(Time.time * 0.1f), 0, Mathf.Cos(Time.time * 0.1f));
        float windStrength = 0.5f + Mathf.Sin(Time.time * 0.05f) * 0.5f;
        
        // Calculate wind effect based on ship's orientation
        float windEffect = Vector3.Dot(windDirection, transform.forward);
        rb.AddForce(windDirection * windStrength * windEffect * windResistance);
    }

    private void ApplyBraking()
    {
        if (rb.linearVelocity.magnitude < 0.01f)
        {
            rb.linearVelocity = Vector3.zero;
            currentState = ShipState.Idle;
            return;
        }
        
        rb.AddForce(-rb.linearVelocity * deceleration, ForceMode.Acceleration);
    }
}
