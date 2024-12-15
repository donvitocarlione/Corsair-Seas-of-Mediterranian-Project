using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimplifiedBuoyancy : MonoBehaviour
{
    [Header("Basic Settings")]
    public float floatHeight = 0f;
    public float bobSpeed = 1f;
    public float bobAmount = 0.1f;
    
    [Header("Movement Settings")]
    public float stabilizationSpeed = 2f;
    public float rollStability = 0.3f;
    public float pitchStability = 0.2f;

    [Header("Wave Sync")]
    public Material waterMaterial;
    [SerializeField] private float waveAmplitude = 1f;
    [SerializeField] private float waveFrequency = 1f;
    [SerializeField] private float waveSpeed = 1f;

    private Rigidbody rb;
    private float timeOffset;
    private MaterialPropertyBlock propertyBlock;
    private MeshRenderer meshRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        timeOffset = Random.Range(0f, 100f); // Randomize bobbing phase
        meshRenderer = GetComponent<MeshRenderer>();
        propertyBlock = new MaterialPropertyBlock();
        
        // Configure rigidbody for controlled movement
        rb.useGravity = true;
        rb.mass = 1000f;
    }

    void FixedUpdate()
    {
        // Calculate base height with simple sine wave
        float time = Time.time * waveSpeed + timeOffset;
        float xPos = transform.position.x;
        float zPos = transform.position.z;
        
        // Combine multiple waves for more natural movement
        float wave1 = Mathf.Sin(time + xPos * waveFrequency) * waveAmplitude;
        float wave2 = Mathf.Sin(time * 0.8f + zPos * waveFrequency * 0.8f) * (waveAmplitude * 0.5f);
        float targetHeight = floatHeight + wave1 + wave2 + Mathf.Sin(time * bobSpeed) * bobAmount;
        
        // Smoothly move towards target height
        float currentHeight = transform.position.y;
        float heightDifference = targetHeight - currentHeight;
        Vector3 buoyancyForce = Vector3.up * (heightDifference * stabilizationSpeed);
        
        // Apply force
        rb.AddForce(buoyancyForce * rb.mass);
        
        // Apply stabilization
        ApplyStabilization();
        
        // Update shader displacement
        UpdateShaderDisplacement(wave1 + wave2);
    }

    void ApplyStabilization()
    {
        // Stabilize roll
        float currentRoll = Vector3.Dot(transform.right, Vector3.up);
        Vector3 rollCorrection = -transform.forward * (currentRoll * rollStability);
        
        // Stabilize pitch
        float currentPitch = Vector3.Dot(transform.forward, Vector3.up);
        Vector3 pitchCorrection = -transform.right * (currentPitch * pitchStability);
        
        // Apply stabilization torques
        rb.AddTorque(rollCorrection * rb.mass);
        rb.AddTorque(pitchCorrection * rb.mass);
    }

    void UpdateShaderDisplacement(float waveHeight)
    {
        if (meshRenderer != null)
        {
            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat("_WaveDisplacement", waveHeight);
            meshRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
