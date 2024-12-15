using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private LayerMask groundLayerMask = Physics.DefaultRaycastLayers;
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private float cameraMovementSpeed = 10f;
    [SerializeField]
    private float cameraDragSpeed = 2f;
    
    private Ship selectedShip;
    private bool isDragging;
    private Vector3 lastMousePosition;

    private void Start()
    {
        Debug.Log("[InputManager] Initializing");
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[InputManager] No main camera found!");
            }
        }
    }

    private void Update()
    {
        if (mainCamera == null) return;

        // Handle right-click for movement
        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }

        // Handle left-click and drag for camera movement
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            HandleCameraDrag();
        }

        // Handle keyboard input
        HandleKeyboardInput();
    }

    private void HandleRightClick()
    {
        Debug.Log("[InputManager] Processing right click");
        
        if (selectedShip == null)
        {
            Debug.Log("[InputManager] No ship selected for movement");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Debug.Log($"[InputManager] Casting ray from {ray.origin} in direction {ray.direction}");

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
        {
            Debug.Log($"[InputManager] Hit point: {hit.point}");
            
            if (selectedShip.TryGetComponent<ShipMovement>(out var movement))
            {
                Debug.Log($"[InputManager] Setting target position for {selectedShip.ShipName}");
                movement.SetTargetPosition(hit.point);
            }
            else
            {
                Debug.LogWarning($"[InputManager] Selected ship {selectedShip.ShipName} has no ShipMovement component");
            }
        }
        else
        {
            Debug.Log("[InputManager] Right-click raycast didn't hit anything");
        }
    }

    private void HandleCameraDrag()
    {
        Vector3 deltaPosition = Input.mousePosition - lastMousePosition;
        Vector3 cameraMovement = new Vector3(-deltaPosition.x, 0, -deltaPosition.y) * cameraDragSpeed * Time.deltaTime;
        
        if (mainCamera.transform.parent != null)
        {
            mainCamera.transform.parent.Translate(cameraMovement, Space.World);
        }
        else
        {
            mainCamera.transform.Translate(cameraMovement, Space.World);
        }

        lastMousePosition = Input.mousePosition;
    }

    private void HandleKeyboardInput()
    {
        // WASD movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (horizontal != 0 || vertical != 0)
        {
            Vector3 movement = new Vector3(horizontal, 0, vertical) * cameraMovementSpeed * Time.deltaTime;
            
            if (mainCamera.transform.parent != null)
            {
                mainCamera.transform.parent.Translate(movement, Space.World);
            }
            else
            {
                mainCamera.transform.Translate(movement, Space.World);
            }
        }

        // Tab key for cycling through ships
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("[InputManager] Tab pressed - cycling ships");
            var player = FindFirstObjectByType<Player>();
            if (player != null)
            {
                player.SelectNextShip();
            }
        }
    }

    public void OnShipSelected(Ship ship)
    {
        Debug.Log($"[InputManager] Ship selection changed to {(ship != null ? ship.ShipName : "null")}");
        selectedShip = ship;
    }

    private void OnValidate()
    {
        if (cameraMovementSpeed <= 0)
        {
            cameraMovementSpeed = 10f;
            Debug.LogWarning("[InputManager] Camera movement speed must be positive - reset to default");
        }

        if (cameraDragSpeed <= 0)
        {
            cameraDragSpeed = 2f;
            Debug.LogWarning("[InputManager] Camera drag speed must be positive - reset to default");
        }
    }
}