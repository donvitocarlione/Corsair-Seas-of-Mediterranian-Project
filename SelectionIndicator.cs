using UnityEngine;

public class SelectionIndicator : MonoBehaviour
{
    public float radius = 8f;              // Size of the circle
    public float lineWidth = 0.2f;         // Thickness of the line
    public Color selectionColor = new Color(0f, 1f, 1f, 0.5f); // Cyan semi-transparent
    public float rotationSpeed = 30f;      // How fast the circle rotates
    public float heightOffset = 0.5f;      // Height above water
    
    private LineRenderer lineRenderer;

    void Start()
    {
        // Create and configure the LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        InitializeLineRenderer();
        DrawCircle();
    }

    void Update()
    {
        // Rotate the selection ring
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    void InitializeLineRenderer()
    {
        // Set up the line renderer material
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;

        // Configure line appearance
        lineRenderer.startColor = selectionColor;
        lineRenderer.endColor = selectionColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 51;  // Number of points in the circle
        lineRenderer.useWorldSpace = false; // Use local space for easier rotation

        // Make sure the line renders on top of everything
        lineRenderer.material.renderQueue = 3000;
    }

    void DrawCircle()
    {
        float deltaTheta = (2f * Mathf.PI) / 50;
        float theta = 0f;

        for (int i = 0; i < 51; i++)
        {
            float x = radius * Mathf.Cos(theta);
            float z = radius * Mathf.Sin(theta);
            lineRenderer.SetPosition(i, new Vector3(x, heightOffset, z));
            theta += deltaTheta;
        }
    }

    public void UpdateColor(Color newColor)
    {
        selectionColor = newColor;
        if (lineRenderer != null)
        {
            lineRenderer.startColor = selectionColor;
            lineRenderer.endColor = selectionColor;
        }
    }

    public void UpdateSize(float newRadius)
    {
        radius = newRadius;
        if (lineRenderer != null)
        {
            DrawCircle();
        }
    }
}
