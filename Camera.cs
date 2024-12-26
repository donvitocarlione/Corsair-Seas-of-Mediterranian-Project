using UnityEngine;

public class GameCamera : MonoBehaviour
{
    [Header("Movement Settings")]
    public float zoomSpeed = 10f;
    public float panSpeed = 20f;
    public float rotationSpeed = 100f;
    public float smoothSpeed = 10f;

    [Header("Zoom Limits")]
    public float minZoom = 5f;
    public float maxZoom = 30f;

    [Header("Height Settings")]
    public float minHeight = 10f;
    public float maxHeight = 50f;
    public float heightDampening = 0.5f;

    [Header("Boundary Settings")]
    public bool useBoundary = true;
    public Vector2 boundaryX = new Vector2(-50f, 50f);
    public Vector2 boundaryZ = new Vector2(-50f, 50f);

    [Header("Edge Scrolling")]
    public bool useEdgeScrolling = true;
    public float edgeScrollThreshold = 20f;

    private Vector3 targetPosition;
    private float currentZoom;
    private float currentRotationAngle;
    private Vector3 lastMousePosition;

    void Start()
    {
        currentZoom = (minZoom + maxZoom) / 2f;
        targetPosition = transform.position;
        currentRotationAngle = transform.eulerAngles.y;
    }

    void Update()
    {
        HandleZooming();
        HandlePanning();
        HandleRotation();
        HandleEdgeScrolling();
        UpdateCameraPosition();
    }

    void HandleZooming()
    {
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        currentZoom = Mathf.Clamp(currentZoom - zoomInput * zoomSpeed, minZoom, maxZoom);
        
        // Adjust height based on zoom level
        float heightRatio = (currentZoom - minZoom) / (maxZoom - minZoom);
        float targetHeight = Mathf.Lerp(minHeight, maxHeight, heightRatio);
        targetPosition.y = Mathf.Lerp(targetPosition.y, targetHeight, Time.deltaTime * heightDampening);
    }

    void HandlePanning()
    {
        // Keyboard panning
        Vector3 panInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        
        if (panInput.magnitude > 0)
        {
            // Transform the input direction based on camera rotation
            Vector3 adjustedInput = Quaternion.Euler(0, currentRotationAngle, 0) * panInput;
            targetPosition += adjustedInput * panSpeed * Time.deltaTime * (currentZoom / minZoom);
        }

        // Middle mouse button panning
        if (Input.GetMouseButton(2))
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            Vector3 moveDirection = new Vector3(-mouseDelta.x, 0, -mouseDelta.y);
            targetPosition += transform.TransformDirection(moveDirection) * panSpeed * Time.deltaTime * 0.1f * (currentZoom / minZoom);
        }

        if (useBoundary)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, boundaryX.x, boundaryX.y);
            targetPosition.z = Mathf.Clamp(targetPosition.z, boundaryZ.x, boundaryZ.y);
        }
    }

    void HandleRotation()
    {
        // Right mouse button rotation
        if (Input.GetMouseButton(1))
        {
            float rotationDelta = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            currentRotationAngle += rotationDelta;
        }
    }

    void HandleEdgeScrolling()
    {
        if (!useEdgeScrolling) return;

        Vector3 mousePos = Input.mousePosition;
        Vector3 moveDirection = Vector3.zero;

        // Check screen edges
        if (mousePos.x < edgeScrollThreshold) moveDirection.x = -1;
        else if (mousePos.x > Screen.width - edgeScrollThreshold) moveDirection.x = 1;
        
        if (mousePos.y < edgeScrollThreshold) moveDirection.z = -1;
        else if (mousePos.y > Screen.height - edgeScrollThreshold) moveDirection.z = 1;

        if (moveDirection != Vector3.zero)
        {
            moveDirection = Quaternion.Euler(0, currentRotationAngle, 0) * moveDirection;
            targetPosition += moveDirection * panSpeed * Time.deltaTime * (currentZoom / minZoom);
        }
    }

    void UpdateCameraPosition()
    {
        // Smooth position and rotation updates
        transform.position = Vector3.Lerp(transform.position, 
            new Vector3(targetPosition.x, targetPosition.y, targetPosition.z), 
            Time.deltaTime * smoothSpeed);

        Quaternion targetRotation = Quaternion.Euler(45, currentRotationAngle, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);

        lastMousePosition = Input.mousePosition;
    }

    void OnDrawGizmos()
    {
        if (useBoundary)
        {
            // Draw boundary gizmos
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((boundaryX.x + boundaryX.y) / 2f, 0, (boundaryZ.x + boundaryZ.y) / 2f);
            Vector3 size = new Vector3(boundaryX.y - boundaryX.x, 1, boundaryZ.y - boundaryZ.x);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
