using UnityEngine;

public class Buoyancy : MonoBehaviour
{
    [Header("Buoyancy Settings")]
    [SerializeField] private float waterDensity = 1000f;
    [SerializeField] private float waterLevelY = 0f;
    [SerializeField] private float buoyancyForce = 1000f;
    [SerializeField] private float waterDrag = 1f;
    [SerializeField] private float waterAngularDrag = 1f;
    
    [Header("Wave Settings")]
    [SerializeField] private bool useWaves = true;
    [SerializeField] private float waveHeight = 0.5f;
    [SerializeField] private float waveFrequency = 1f;
    [SerializeField] private float waveSpeed = 1f;
    [SerializeField] private Vector2 waveDirection = new Vector2(1f, 1f);
    [SerializeField] private float secondaryWaveRatio = 0.8f;
    
    [Header("Advanced Settings")]
    [SerializeField] private float sideResistance = 2f;
    [SerializeField] private float turningResistance = 1f;
    [SerializeField] private int buoyancyPoints = 8;
    
    [Header("Stabilization")]
    [SerializeField] private float rollStability = 0.3f;
    [SerializeField] private float pitchStability = 0.2f;
    [SerializeField] private float uprightForce = 1f;
    
    [Header("Debug Info")]
    public float boatSubmergedPercentage = 0f;
    public bool isInWater;
    public Vector3 waterMovement;
    public bool showDebugLogs = false;
    
    private Rigidbody rb;
    private Collider boatCollider;
    private float initialDrag;
    private float initialAngularDrag;
    private float timeOffset;
    private Vector3[] buoyancyPointsPositions;
    private float[] submersionHistory;
    private const int historyLength = 10;
    private int historyIndex = 0;
    
    public float WaterLevel => waterLevelY;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        boatCollider = GetComponent<Collider>();
        
        if (rb == null || boatCollider == null)
        {
            Debug.LogError("Ship needs both Rigidbody and Collider!");
            enabled = false;
            return;
        }
        
        initialDrag = rb.drag;
        initialAngularDrag = rb.angularDrag;
        timeOffset = Random.Range(0f, 100f);
        submersionHistory = new float[historyLength];
        
        // Ensure ship starts above water
        if (transform.position.y <= waterLevelY)
        {
            transform.position = new Vector3(transform.position.x, waterLevelY + 0.5f, transform.position.z);
        }
        
        InitializeBuoyancyPoints();
        
        // Initialize history
        for (int i = 0; i < historyLength; i++)
        {
            submersionHistory[i] = 0f;
        }
    }
    
    private void InitializeBuoyancyPoints()
    {
        buoyancyPointsPositions = new Vector3[buoyancyPoints];
        Bounds bounds = boatCollider.bounds;
        float length = bounds.size.z;
        float width = bounds.size.x;
        
        // Create distributed points for better stability
        for (int i = 0; i < buoyancyPoints; i++)
        {
            float xPos = (i % 2 == 0) ? -width/4 : width/4;
            float zPos = (length/2) * ((float)i / buoyancyPoints - 0.5f);
            float yPos = -bounds.size.y/3; // Moved up slightly for better stability
            buoyancyPointsPositions[i] = new Vector3(xPos, yPos, zPos);
        }
    }
    
    private void FixedUpdate()
    {
        float totalSubmerged = 0f;
        Vector3 totalBuoyancyForce = Vector3.zero;
        
        foreach (Vector3 point in buoyancyPointsPositions)
        {
            Vector3 worldPoint = transform.TransformPoint(point);
            float waveHeight = CalculateWaveHeight(worldPoint);
            
            if (worldPoint.y < waveHeight)
            {
                float submersion = Mathf.Clamp01((waveHeight - worldPoint.y) / 1f);
                totalSubmerged += submersion;
                
                // Calculate buoyancy force with improved depth handling
                float depthFactor = Mathf.Exp(-Mathf.Abs(waveHeight - worldPoint.y));
                float displacementMultiplier = submersion * (waterDensity / 1000f) * depthFactor;
                
                // Base buoyancy force
                Vector3 buoyancyForceAtPoint = Vector3.up * buoyancyForce * displacementMultiplier;
                
                // Add wave influence
                if (useWaves)
                {
                    Vector3 waveForce = CalculateWaveForce(worldPoint) * displacementMultiplier * 0.5f;
                    buoyancyForceAtPoint += waveForce;
                }
                
                rb.AddForceAtPosition(buoyancyForceAtPoint, worldPoint, ForceMode.Force);
                totalBuoyancyForce += buoyancyForceAtPoint;
            }
        }
        
        // Update submersion history
        submersionHistory[historyIndex] = totalSubmerged / buoyancyPoints;
        historyIndex = (historyIndex + 1) % historyLength;
        
        // Calculate average submersion for smoother transitions
        float averageSubmersion = 0f;
        for (int i = 0; i < historyLength; i++)
        {
            averageSubmersion += submersionHistory[i];
        }
        averageSubmersion /= historyLength;
        
        boatSubmergedPercentage = averageSubmersion;
        isInWater = averageSubmersion > 0.1f;
        
        if (isInWater)
        {
            ApplyWaterResistance();
            ApplyStabilization();
        }
        else
        {
            rb.drag = initialDrag;
            rb.angularDrag = initialAngularDrag;
        }
    }
    
    private Vector3 CalculateWaveForce(Vector3 worldPosition)
    {
        if (!useWaves) return Vector3.zero;
        
        float time = Time.time * waveSpeed + timeOffset;
        float dx = waveDirection.x * waveFrequency;
        float dz = waveDirection.y * waveFrequency;
        
        // Calculate wave slopes
        float slopeX = -waveHeight * dx * Mathf.Sin(worldPosition.x * dx + time);
        float slopeZ = -waveHeight * dz * Mathf.Sin(worldPosition.z * dz + time);
        
        return new Vector3(slopeX, 0, slopeZ) * waveSpeed;
    }
    
    private float CalculateWaveHeight(Vector3 worldPosition)
    {
        if (!useWaves)
            return waterLevelY;
            
        float time = Time.time * waveSpeed + timeOffset;
        float x = worldPosition.x * waveDirection.x;
        float z = worldPosition.z * waveDirection.y;
        
        // Primary wave
        float wave1 = Mathf.Sin(x * waveFrequency + time);
        
        // Secondary waves for more natural look
        float wave2 = Mathf.Sin(z * waveFrequency * secondaryWaveRatio + time * 0.8f);
        float wave3 = Mathf.Sin((x + z) * waveFrequency * 0.5f + time * 1.2f);
        
        float waveSum = (wave1 * 0.6f + wave2 * 0.25f + wave3 * 0.15f);
        return waterLevelY + waveSum * waveHeight;
    }
    
    private void ApplyWaterResistance()
    {
        // Convert velocity to local space
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        
        // Calculate drag forces
        float submersionFactor = Mathf.Clamp01(boatSubmergedPercentage * 2f);
        Vector3 dragForce = new Vector3(
            -localVelocity.x * sideResistance,
            -Mathf.Abs(localVelocity.y) * waterDrag,
            -localVelocity.z * (sideResistance * 0.5f)
        ) * submersionFactor;
        
        // Apply drag in world space
        rb.AddRelativeForce(dragForce, ForceMode.Force);
        
        // Angular drag
        Vector3 angularDragForce = -rb.angularVelocity * turningResistance * submersionFactor;
        rb.AddTorque(angularDragForce, ForceMode.Force);
    }
    
    private void ApplyStabilization()
    {
        // Get current orientation
        Vector3 up = transform.up;
        Vector3 forward = transform.forward;
        
        // Calculate correction forces
        float submersionFactor = Mathf.Clamp01(boatSubmergedPercentage * 2f);
        
        // Roll stabilization
        float currentRoll = Vector3.Dot(transform.right, Vector3.up);
        Vector3 rollCorrection = Vector3.Cross(transform.right, Vector3.up) * 
                               currentRoll * rollStability * submersionFactor;
        
        // Pitch stabilization
        float currentPitch = Vector3.Dot(transform.forward, Vector3.up);
        Vector3 pitchCorrection = Vector3.Cross(transform.forward, Vector3.up) * 
                                currentPitch * pitchStability * submersionFactor;
        
        // Apply upright force
        Vector3 uprightCorrection = Vector3.up * uprightForce * submersionFactor;
        
        // Apply all stabilization forces
        rb.AddTorque(rollCorrection + pitchCorrection, ForceMode.Force);
        rb.AddForce(uprightCorrection, ForceMode.Force);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || buoyancyPointsPositions == null) return;
        
        // Draw buoyancy points
        foreach (Vector3 point in buoyancyPointsPositions)
        {
            Vector3 worldPoint = transform.TransformPoint(point);
            float waveHeight = CalculateWaveHeight(worldPoint);
            
            Gizmos.color = worldPoint.y < waveHeight ? Color.green : Color.red;
            Gizmos.DrawWireSphere(worldPoint, 0.1f);
            
            // Draw water level at point
            Gizmos.color = Color.blue;
            Vector3 waterPoint = new Vector3(worldPoint.x, waveHeight, worldPoint.z);
            Gizmos.DrawLine(worldPoint, waterPoint);
            
            // Draw forces when submerged
            if (worldPoint.y < waveHeight)
            {
                Gizmos.color = Color.yellow;
                Vector3 buoyancyForceVis = Vector3.up * buoyancyForce * 0.001f;
                Gizmos.DrawRay(worldPoint, buoyancyForceVis);
                
                if (useWaves)
                {
                    Gizmos.color = Color.cyan;
                    Vector3 waveForce = CalculateWaveForce(worldPoint) * 0.1f;
                    Gizmos.DrawRay(worldPoint, waveForce);
                }
            }
        }
        
        // Draw water plane
        Gizmos.color = new Color(0f, 0.7f, 1f, 0.3f);
        float waterLevel = CalculateWaveHeight(transform.position);
        Vector3 center = new Vector3(transform.position.x, waterLevel, transform.position.z);
        Gizmos.DrawCube(center, new Vector3(5f, 0.1f, 5f));
    }
}
