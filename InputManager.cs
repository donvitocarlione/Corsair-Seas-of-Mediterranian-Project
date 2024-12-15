using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private LayerMask surfaceLayer;  // Changed from groundLayer to surfaceLayer
    [SerializeField] private float waterLevel = 0f;   // Added water level reference
    [SerializeField] private Camera mainCamera;
    private Player player;

    private void Start()
    {
        player = FindFirstObjectByType<Player>();
        if (player == null)
        {
            Debug.LogError("[InputManager] No Player found in scene!");
        }

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
        if (player == null) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            player.SelectNextShip();
        }

        if (Input.GetMouseButtonDown(1)) // Right click
        {
            HandleMovementInput();
        }
    }

    private void HandleMovementInput()
    {
        Ship selectedShip = player.GetSelectedShip();
        if (selectedShip == null)
        {
            Debug.Log("[InputManager] No ship selected for movement");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // First try to hit the surface layer
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, surfaceLayer))
        {
            SetShipTargetPosition(selectedShip, hit.point);
        }
        // If no surface hit, project the ray to the water plane
        else
        {
            Plane waterPlane = new Plane(Vector3.up, new Vector3(0, waterLevel, 0));
            float enter;
            if (waterPlane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                SetShipTargetPosition(selectedShip, hitPoint);
            }
            else
            {
                Debug.Log("[InputManager] Could not determine target position on water surface");
            }
        }
    }

    private void SetShipTargetPosition(Ship ship, Vector3 position)
    {
        ShipMovement movement = ship.GetComponent<ShipMovement>();
        if (movement != null)
        {
            // Ensure the target position is at water level
            position.y = waterLevel;
            movement.SetTargetPosition(position);
            Debug.Log($"[InputManager] Set target position for {ship.name} to {position}");
        }
        else
        {
            Debug.LogError($"[InputManager] Selected ship {ship.name} has no ShipMovement component!");
        }
    }
}