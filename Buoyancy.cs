using UnityEngine;

public class Buoyancy : MonoBehaviour
{
    [Header("Buoyancy Settings")]
    public float waterDensity = 1000f;
    [SerializeField] private float waterLevelY = 0f;
    public float buoyancyForce = 15f;
    public float waterDrag = 2f;
    public float waterAngularDrag = 2f;
    
    [Header("Advanced Settings")]
    public float sideResistance = 3f;  // Resistance to sideways movement
    public float turningResistance = 2f;  // Resistance to rotation
    public bool useWaves = true;
    public float waveHeight = 0.5f;
    public float waveFrequency = 1f;
    
    [Header("Stabilization")]
    public float rollStability = 0.3f;  // How strongly the ship tries to stay upright
    public float pitchStability = 0.2f;  // How strongly the ship resists pitch changes
    
    [Header("Debug Info")]
    public float boatSubmergedPercentage = 0f;
    public bool isInWater;
    public Vector3 waterMovement;
    
    private Rigidbody rb;
    private Collider boatCollider;
    private float initialDrag;
    private float initialAngularDrag;
    private float timeOffset;

    // Public property to access water level
    public float WaterLevel => waterLevelY;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        boatCollider = GetComponent<Collider>();
        
        if (rb == null)
        {
            Debug.LogError("Ship needs a Rigidbody!");
            enabled = false;
            return;
        }
        
        if (boatCollider == null)
        {
            Debug.LogError("Ship needs a collider!");
            enabled = false;
            return;
        }
        
        // Store initial values
        initialDrag = rb.linearDamping;
        initialAngularDrag = rb.angularDamping;
        
        // Set recommended Rigidbody settings
        rb.mass = 1000f; // 1 ton
        rb.useGravity = true;
        
        // Allow some rotation for more realistic movement
        rb.constraints = RigidbodyConstraints.None;
        
        // Random offset for wave variation
        timeOffset = Random.Range(0f, 100f);
    }
    
    void FixedUpdate()
    {
        UpdateWaterLevel();
        
        // Calculate submerged percentage
        boatSubmergedPercentage = CalculateSubmergedPercentage();
        isInWater = boatSubmergedPercentage > 0;
        
        if (isInWater)
        {
            ApplyBuoyancyForce();
            ApplyWaterResistance();
            ApplyStabilizationForces();
        }
        else
        {
            // Reset drag when out of water
            rb.linearDamping = initialDrag;
            rb.angularDamping = initialAngularDrag;
        }
    }
    
    void UpdateWaterLevel()
    {
        if (useWaves)
        {
            float waveFactor = Mathf.Sin((Time.time + timeOffset) * waveFrequency);
            waterLevelY = waveFactor * waveHeight;
        }
    }
    
    void ApplyBuoyancyForce()
    {
        // Calculate volume-based buoyancy force
        float shipVolume = boatCollider.bounds.size.x * boatCollider.bounds.size.y * boatCollider.bounds.size.z;
        float submeredVolume = shipVolume * boatSubmergedPercentage;
        float forceMagnitude = buoyancyForce * submeredVolume * waterDensity * -Physics.gravity.y;
        
        // Calculate center of buoyancy
        Vector3 centerOfBuoyancy = CalculateCenterOfBuoyancy();
        
        // Apply buoyant force
        Vector3 buoyantForce = Vector3.up * forceMagnitude;
        rb.AddForceAtPosition(buoyantForce, centerOfBuoyancy, ForceMode.Force);
    }
    
    Vector3 CalculateCenterOfBuoyancy()
    {
        // Calculate a more accurate center of buoyancy based on orientation
        Vector3 center = boatCollider.bounds.center;
        float pitchFactor = Vector3.Dot(transform.forward, Vector3.up);
        float rollFactor = Vector3.Dot(transform.right, Vector3.up);
        
        // Adjust center based on orientation
        center += transform.right * (rollFactor * boatCollider.bounds.size.x * 0.15f);
        center += transform.forward * (pitchFactor * boatCollider.bounds.size.z * 0.15f);
        
        // Lower center of buoyancy for better stability
        center.y = Mathf.Lerp(boatCollider.bounds.min.y, boatCollider.bounds.center.y, 0.4f);
        
        return center;
    }
    
    void ApplyWaterResistance()
    {
        // Basic water resistance
        rb.linearDamping = waterDrag;
        rb.angularDamping = waterAngularDrag;
        
        // Calculate relative velocity
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        
        // Apply directional resistance
        Vector3 sideForce = -transform.right * localVelocity.x * sideResistance;
        rb.AddForce(sideForce, ForceMode.Force);
        
        // Apply turning resistance
        Vector3 turnForce = -transform.up * rb.angularVelocity.y * turningResistance;
        rb.AddTorque(turnForce, ForceMode.Force);
    }
    
    void ApplyStabilizationForces()
    {
        // Roll stabilization
        float currentRoll = Vector3.Dot(transform.right, Vector3.up);
        Vector3 rollCorrection = transform.forward * -currentRoll * rollStability;
        rb.AddTorque(rollCorrection, ForceMode.Force);
        
        // Pitch stabilization
        float currentPitch = Vector3.Dot(transform.forward, Vector3.up);
        Vector3 pitchCorrection = transform.right * -currentPitch * pitchStability;
        rb.AddTorque(pitchCorrection, ForceMode.Force);
    }
    
    float CalculateSubmergedPercentage()
    {
        float shipBottom = boatCollider.bounds.min.y;
        float shipHeight = boatCollider.bounds.size.y;
        float submergedHeight = Mathf.Max(0, waterLevelY - shipBottom);
        return Mathf.Clamp01(submergedHeight / shipHeight);
    }
    
    void OnDrawGizmosSelected()
    {
        if (boatCollider != null)
        {
            // Draw water level
            Gizmos.color = new Color(0f, 0.7f, 1f, 0.3f);
            Vector3 center = boatCollider.bounds.center;
            Vector3 size = boatCollider.bounds.size;
            Vector3 waterLineCenter = new Vector3(center.x, waterLevelY, center.z);
            Gizmos.DrawCube(waterLineCenter, new Vector3(size.x * 1.5f, 0.1f, size.z * 1.5f));
            
            // Draw center of buoyancy
            if (isInWater)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(CalculateCenterOfBuoyancy(), 0.3f);
            }
        }
    }
}
