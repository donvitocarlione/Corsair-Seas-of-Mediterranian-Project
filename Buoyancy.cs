using UnityEngine;

public class Buoyancy : MonoBehaviour
{
    [Header("Buoyancy Settings")]
    public float waterDensity = 1000f;  // Standard water density in kg/m³
    [SerializeField] private float waterLevelY = 0f;
    public float buoyancyForce = 1000f;  // Starting with ship mass as base force
    public float waterDrag = 2f;
    public float waterAngularDrag = 2f;
    
    [Header("Wave Settings")]
    public bool useWaves = true;
    public float waveHeight = 1f;  // Increased for better visibility
    public float waveFrequency = 1f;
    public float waveSpeed = 1f;
    public Vector2 waveDirection = new Vector2(1f, 1f);
    public float secondaryWaveRatio = 0.8f;
    
    [Header("Advanced Settings")]
    public float sideResistance = 5f;  // Increased side resistance
    public float turningResistance = 3f;  // Increased turning resistance
    public int buoyancyPoints = 8;  // Increased number of points for better stability
    
    [Header("Stabilization")]
    public float rollStability = 0.5f;  // Increased roll stability
    public float pitchStability = 0.3f;  // Increased pitch stability
    
    [Header("Shader Sync Settings")]
    public Material waterMaterial;
    private readonly int WaveHeightProperty = Shader.PropertyToID("Vector1_7273530c27a34c9f8ee5723b84f96baa");
    private readonly int WaveFrequencyProperty = Shader.PropertyToID("Vector1_6c82dffdd68049bcb019d3a9c64c92a0");
    private readonly int WaveSpeedProperty = Shader.PropertyToID("Vector1_6269b1025b26473ca8bc61634f34b537");
    private readonly int WaveDirectionProperty = Shader.PropertyToID("Vector2_4351ac2be1d74054986ec5378db9d578");
    
    [Header("Debug Info")]
    public float boatSubmergedPercentage = 0f;
    public bool isInWater;
    public Vector3 waterMovement;
    public bool showDebugLogs = false;  // Toggle for debug logs
    
    private Rigidbody rb;
    private Collider boatCollider;
    private float initialDrag;
    private float initialAngularDrag;
    private float timeOffset;
    private Vector3[] buoyancyPointsPositions;

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
        
        initialDrag = rb.linearDamping;
        initialAngularDrag = rb.angularDamping;
        
        // Set initial position slightly above water
        if (transform.position.y <= waterLevelY)
        {
            transform.position = new Vector3(transform.position.x, waterLevelY + 0.1f, transform.position.z);
        }
        
        rb.mass = 1000f;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        
        timeOffset = Random.Range(0f, 100f);
        
        InitializeBuoyancyPoints();
        
        if (waterMaterial != null)
        {
            SyncWithShader();
        }

        Debug.Log("Buoyancy System Initialized - Ship Mass: " + rb.mass + "kg, Water Density: " + waterDensity + "kg/m³");
    }
    
    void SyncWithShader()
    {
        if (waterMaterial != null)
        {
            waveHeight = waterMaterial.GetFloat(WaveHeightProperty);
            waveFrequency = waterMaterial.GetFloat(WaveFrequencyProperty);
            waveSpeed = waterMaterial.GetFloat(WaveSpeedProperty);
            waveDirection = waterMaterial.GetVector(WaveDirectionProperty);
            
            if (showDebugLogs)
            {
                Debug.Log($"Shader Sync - Wave Height: {waveHeight}, Frequency: {waveFrequency}, Speed: {waveSpeed}");
            }
        }
    }
    
    void InitializeBuoyancyPoints()
    {
        buoyancyPointsPositions = new Vector3[buoyancyPoints];
        Bounds bounds = boatCollider.bounds;
        float length = bounds.size.z;
        float width = bounds.size.x;
        
        // Create more distributed points for better stability
        for (int i = 0; i < buoyancyPoints; i++)
        {
            float xPos = (i % 2 == 0) ? -width/3 : width/3;
            float zPos = (length/2) * ((float)i / buoyancyPoints - 0.5f);
            buoyancyPointsPositions[i] = new Vector3(xPos, -bounds.size.y/2, zPos);
        }
    }
    
    void FixedUpdate()
    {
        UpdateWaterLevel();
        
        float totalSubmerged = 0;
        Vector3 totalBuoyancyForce = Vector3.zero;
        
        foreach (Vector3 point in buoyancyPointsPositions)
        {
            Vector3 worldPoint = transform.TransformPoint(point);
            float waveHeight = CalculateWaveHeight(worldPoint);
            
            if (worldPoint.y < waveHeight)
            {
                float submersion = Mathf.Clamp01((waveHeight - worldPoint.y) / 1f);
                totalSubmerged += submersion;
                
                // Improved buoyancy calculation
                float depthFactor = Mathf.Clamp01((waveHeight - worldPoint.y) / 2f);
                float displacementMultiplier = depthFactor * (waterDensity / 1000f);
                Vector3 buoyancyForceAtPoint = Vector3.up * buoyancyForce * displacementMultiplier;
                
                Vector3 waveForce = CalculateWaveForce(worldPoint);
                buoyancyForceAtPoint += waveForce * displacementMultiplier;
                
                rb.AddForceAtPosition(buoyancyForceAtPoint, worldPoint, ForceMode.Force);
                totalBuoyancyForce += buoyancyForceAtPoint;
                
                if (showDebugLogs)
                {
                    Debug.Log($"Point {worldPoint} - Submersion: {submersion:F2}, Force: {buoyancyForceAtPoint.magnitude:F2}N");
                }
            }
        }
        
        boatSubmergedPercentage = totalSubmerged / buoyancyPoints;
        isInWater = boatSubmergedPercentage > 0;
        
        if (isInWater)
        {
            ApplyWaterResistance();
            ApplyStabilizationForces();
            
            if (showDebugLogs)
            {
                Debug.Log($"Total Buoyancy Force: {totalBuoyancyForce.magnitude:F2}N, Submerged: {boatSubmergedPercentage:P0}");
            }
        }
        else
        {
            rb.linearDamping = initialDrag;
            rb.angularDamping = initialAngularDrag;
        }
    }
    
    Vector3 CalculateWaveForce(Vector3 worldPosition)
    {
        if (!useWaves) return Vector3.zero;

        float dx = waveDirection.x * waveFrequency;
        float dz = waveDirection.y * waveFrequency;
        float time = Time.time * waveSpeed + timeOffset;
        
        float slopeX = Mathf.Cos(worldPosition.x * dx + time) * waveHeight;
        float slopeZ = Mathf.Cos(worldPosition.z * dz + time) * waveHeight;
        
        return new Vector3(slopeX, 0, slopeZ) * waveSpeed;
    }
    
    float CalculateWaveHeight(Vector3 worldPosition)
    {
        if (!useWaves)
            return waterLevelY;
            
        float x = worldPosition.x * waveDirection.x;
        float z = worldPosition.z * waveDirection.y;
        float time = Time.time * waveSpeed + timeOffset;
        
        float wave1 = Mathf.Sin(x * waveFrequency + time);
        float wave2 = Mathf.Sin(z * waveFrequency * secondaryWaveRatio + time * secondaryWaveRatio);
        float wave3 = Mathf.Sin((x + z) * waveFrequency * 0.5f + time * 1.2f);
        
        float waveSum = (wave1 + wave2 + wave3) / 3f;
        return waterLevelY + waveSum * waveHeight;
    }
    
    void ApplyWaterResistance()
    {
        rb.linearDamping = waterDrag * boatSubmergedPercentage;
        rb.angularDamping = waterAngularDrag * boatSubmergedPercentage;
        
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        
        // Improved directional resistance
        Vector3 sideForce = -transform.right * localVelocity.x * sideResistance * boatSubmergedPercentage;
        Vector3 forwardForce = -transform.forward * localVelocity.z * (sideResistance * 0.5f) * boatSubmergedPercentage;
        rb.AddForce(sideForce + forwardForce, ForceMode.Force);
        
        Vector3 turnForce = -transform.up * rb.angularVelocity.y * turningResistance * boatSubmergedPercentage;
        rb.AddTorque(turnForce, ForceMode.Force);
    }
    
    void ApplyStabilizationForces()
    {
        float currentRoll = Vector3.Dot(transform.right, Vector3.up);
        Vector3 rollCorrection = transform.forward * -currentRoll * rollStability * boatSubmergedPercentage;
        
        float currentPitch = Vector3.Dot(transform.forward, Vector3.up);
        Vector3 pitchCorrection = transform.right * -currentPitch * pitchStability * boatSubmergedPercentage;
        
        rb.AddTorque(rollCorrection + pitchCorrection, ForceMode.Force);
    }
    
    void UpdateWaterLevel()
    {
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
            
            // Draw buoyancy points and their depth
            Gizmos.DrawLine(worldPoint, new Vector3(worldPoint.x, waveHeight, worldPoint.z));
            Gizmos.DrawWireSphere(worldPoint, 0.1f);
            
            // Show forces when point is submerged
            if (worldPoint.y < waveHeight)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(worldPoint, Vector3.up * buoyancyForce * 0.001f);
                
                if (useWaves)
                {
                    Gizmos.color = Color.red;
                    Vector3 waveForce = CalculateWaveForce(worldPoint);
                    Gizmos.DrawRay(worldPoint, waveForce);
                }
                
                Gizmos.color = Color.blue;
            }
        }
        
        // Visualize water level
        Gizmos.color = new Color(0f, 0.7f, 1f, 0.3f);
        Vector3 shipPos = transform.position;
        float shipWaterLevel = CalculateWaveHeight(shipPos);
        Vector3 cubeCenter = new Vector3(shipPos.x, shipWaterLevel, shipPos.z);
        Gizmos.DrawCube(cubeCenter, new Vector3(5f, 0.1f, 5f));
    }
}
