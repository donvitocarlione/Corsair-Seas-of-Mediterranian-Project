// SelectionIndicator.cs
using UnityEngine;

public class SelectionIndicator : MonoBehaviour
{
    [SerializeField] private float radius = 8f;
    [SerializeField] private float lineWidth = 0.2f;
    [SerializeField] private Color selectionColor = new Color(0f, 1f, 1f, 0.5f);
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float heightOffset = 0.5f;
    [SerializeField] private int circlePointCount = 50;
    
    private LineRenderer lineRenderer;
    private Material lineMaterial;

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
         InitializeLineRenderer();
         DrawCircle();
    }
     private void OnDestroy()
    {
        if(lineMaterial != null) Destroy(lineMaterial);
    }
     
    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    void InitializeLineRenderer()
    {
        lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material = lineMaterial;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;

        lineRenderer.startColor = selectionColor;
        lineRenderer.endColor = selectionColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = circlePointCount + 1;
        lineRenderer.useWorldSpace = false;

        lineRenderer.material.renderQueue = 3000;
    }

    void DrawCircle()
    {
       float deltaTheta = (2f * Mathf.PI) / circlePointCount;
        float theta = 0f;

       for (int i = 0; i <= circlePointCount; i++)
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