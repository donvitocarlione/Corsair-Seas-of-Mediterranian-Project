using UnityEngine;

public class InputManager : MonoBehaviour
{
    private Ship selectedShip;
    private Camera mainCamera;
    
    [SerializeField]
    private LayerMask shipLayerMask;
    [SerializeField]
    private LayerMask groundLayerMask; // For right-click movement target detection

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            return;
        }

        // Setup default layer masks if not set
        if (shipLayerMask == 0)
        {
            shipLayerMask = LayerMask.GetMask("Ship");
            Debug.LogWarning("Ship layer mask not set. Defaulting to 'Ship' layer.");
        }

        if (groundLayerMask == 0)
        {
            groundLayerMask = LayerMask.GetMask("Default", "Water"); // Add other ground layers as needed
            Debug.LogWarning("Ground layer mask not set. Defaulting to 'Default' and 'Water' layers.");
        }
    }

    public void OnShipSelected(Ship ship)
    {
        // Deselect previous ship if any
        if (selectedShip != null)
        {
            selectedShip.Deselect();
        }
        
        selectedShip = ship;
        if (selectedShip != null)
        {
            selectedShip.Select();
        }
    }

    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        // Only handle right-click when we have a selected ship
        if (selectedShip == null) return;

        if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask))
            {
                // Get ship movement component and set target position
                var movement = selectedShip.GetComponent<ShipMovement>();
                if (movement != null)
                {
                    movement.SetTargetPosition(hit.point);
                    Debug.Log($"[InputManager] Moving ship to position: {hit.point}");
                }
            }
        }
    }

    public Ship GetSelectedShip()
    {
        return selectedShip;
    }

    private void OnValidate()
    {
        // Help ensure proper layer masks are set in the inspector
        if (shipLayerMask == 0)
        {
            Debug.LogWarning("Ship layer mask not set in InputManager. Please set it in the inspector.");
        }
        if (groundLayerMask == 0)
        {
            Debug.LogWarning("Ground layer mask not set in InputManager. Please set it in the inspector.");
        }
    }
}
