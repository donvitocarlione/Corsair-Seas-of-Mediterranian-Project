using UnityEngine;

public class Buoyancy : MonoBehaviour
{
    [Header("Buoyancy Settings")]
    public float waterDensity = 1000f;
    [SerializeField] private float waterLevelY = 0f;
    public float buoyancyForce = 15f;
    public float waterDrag = 2f;
    public float waterAngularDrag = 2f;
    
    [Header("Wave Settings")]
    public bool useWaves = true;
    public float waveHeight = 0.5f;
    public float waveFrequency = 1f;
    public float waveSpeed = 1f;
    public Vector2 waveDirection = new Vector2(1f, 1f);
    
    [Header("Advanced Settings")]
    public float sideResistance = 3f;  // Resistance to sideways movement
    public float turningResistance = 2f;  // Resistance to rotation
    public int buoyancyPoints = 4; // Number of points to apply buoyancy force
    
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
    private Vector3[] buoyancyPointsPositions;

    // Public property to access water level
    public float WaterLevel => waterLevelY;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        boatCollider = GetComponent<Collider>();
        
        if (rb == null || boatCollider == null)
        {
            Debug.LogError("Ship needs both Rigidbody and Collider!");
            enabled = false;
            return;
        }
        
        // Store initial values
        initialDrag = rb.drag;
        initialAngularDrag = rb.angularDrag;
        
        // Set recommended Rigidbody settings
        rb.mass = 1000f; // 1 ton
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        
        // Random offset for wave variation
        timeOffset = Random.Range(0f, 100f);
        
        // Initialize buoyancy points
        InitializeBuoyancyPoints();
    }
    
    void InitializeBuoyancyPoints()
    {
        buoyancyPointsPositions = new Vector3[buoyancyPoints];
        Bounds bounds = boatCollider.bounds;
        float length = bounds.size.z;
        float width = bounds.size.x;
        
        // Create points along the bottom of the ship
        for (int i = 0; i < buoyancyPoints; i++)
        {
            float xPos = (i % 2 == 0) ? -width/4 : width/4;
            float zPos = (i < buoyancyPoints/2) ? -length/4 : length/4;
            buoyancyPointsPositions[i] = new Vector3(xPos, -bounds.size.y/2, zPos);
        }
    }
    
    void FixedUpdate()
    {
        UpdateWaterLevel();
        
        // Apply buoyancy at each point
        float totalSubmerged = 0;
        foreach (Vector3 point in buoyancyPointsPositions)
        {
            Vector3 worldPoint = transform.TransformPoint(point);
            float waveHeight = CalculateWaveHeight(worldPoint);
            
            if (worldPoint.y < waveHeight)
            {
                float submersion = Mathf.Clamp01((waveHeight - worldPoint.y) / 1f);
                totalSubmerged += submersion;
                
                // Calculate buoyancy force
                float displacementMultiplier = Mathf.Clamp01(1f - (worldPoint.y - (waveHeight - 1f)));
                Vector3 buoyancyForceAtPoint = Vector3.up * buoyancyForce * displacementMultiplier;
                
                // Apply force
                rb.AddForceAtPosition(buoyancyForceAtPoint, worldPoint, ForceMode.Force);
            }
        }
        
        // Update submerged percentage
        boatSubmergedPercentage = totalSubmerged / buoyancyPoints;
        isInWater = boatSubmergedPercentage > 0;
        
        if (isInWater)
        {
            ApplyWaterResistance();
            ApplyStabilizationForces();
        }
        else
        {
            rb.drag = initialDrag;
            rb.angularDrag = initialAngularDrag;
        }
    }
    
    float CalculateWaveHeight(Vector3 worldPosition)
    {
        if (!useWaves)
            return waterLevelY;
            
        float x = worldPosition.x * waveDirection.x;
        float z = worldPosition.z * waveDirection.y;
        
        // Primary wave
        float wave1 = Mathf.Sin(x * waveFrequency + Time.time * waveSpeed + timeOffset);
        float wave2 = Mathf.Sin(z * waveFrequency * 0.8f + Time.time * waveSpeed * 0.8f + timeOffset);
        
        // Secondary wave
        float wave3 = Mathf.Sin((x + z) * waveFrequency * 0.5f + Time.time * waveSpeed * 1.2f + timeOffset);
        
        float waveSum = (wave1 + wave2 + wave3) / 3f;
        return waterLevelY + waveSum * waveHeight;
    }
    
    void ApplyWaterResistance()
    {
        // Basic water resistance
        rb.drag = waterDrag * boatSubmergedPercentage;
        rb.angularDrag = waterAngularDrag * boatSubmergedPercentage;
        
        // Calculate relative velocity
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        
        // Apply directional resistance
        Vector3 sideForce = -transform.right * localVelocity.x * sideResistance * boatSubmergedPercentage;
        rb.AddForce(sideForce, ForceMode.Force);
        
        // Apply turning resistance
        Vector3 turnForce = -transform.up * rb.angularVelocity.y * turningResistance * boatSubmergedPercentage;
        rb.AddTorque(turnForce, ForceMode.Force);
    }
    
    void ApplyStabilizationForces()
    {
        // Roll stabilization
        float currentRoll = Vector3.Dot(transform.right, Vector3.up);
        Vector3 rollCorrection = transform.forward * -currentRoll * rollStability * boatSubmergedPercentage;
        rb.AddTorque(rollCorrection, ForceMode.Force);
        
        // Pitch stabilization
        float currentPitch = Vector3.Dot(transform.forward, Vector3.up);
        Vector3 pitchCorrection = transform.right * -currentPitch * pitchStability * boatSubmergedPercentage;
        rb.AddTorque(pitchCorrection, ForceMode.Force);
    }
    
    void UpdateWaterLevel()
    {
        // This can be updated based on global water level or wave height at the ship's position
        if (useWaves)
        {
            Vector3 centerPos = transform.position;
            waterLevelY = CalculateWaveHeight(centerPos);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || buoyancyPointsPositions == null) return;
        
        Gizmos.color = Color.blue;
        foreach (Vector3 point in buoyancyPointsPositions)
        {
            Vector3 worldPoint = transform.TransformPoint(point);
            float waveHeight = CalculateWaveHeight(worldPoint);
            Gizmos.DrawLine(worldPoint, new Vector3(worldPoint.x, waveHeight, worldPoint.z));
            Gizmos.DrawSphere(worldPoint, 0.1f);
        }
        
        // Draw water level at ship position
        Gizmos.color = new Color(0f, 0.7f, 1f, 0.3f);
        Vector3 shipPos = transform.position;
        float shipWaterLevel = CalculateWaveHeight(shipPos);
        Vector3 cubeCenter = new Vector3(shipPos.x, shipWaterLevel, shipPos.z);
        Gizmos.DrawCube(cubeCenter, new Vector3(3f, 0.1f, 3f));
    }
}
