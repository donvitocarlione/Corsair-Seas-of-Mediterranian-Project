using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("Layer Settings")]
    [SerializeField] private LayerMask surfaceLayer;
    [SerializeField] private LayerMask shipLayer;
    
    [Header("Game Settings")]
    [SerializeField] private float waterLevel = 0f;
    [SerializeField] private Camera mainCamera;

    [Header("Combat Settings")]
    [SerializeField] private float maxTargetingDistance = 150f;
    [SerializeField] private KeyCode cancelTargetKey = KeyCode.Escape;
    
    private Player player;
    private Ship hoveredShip;

    private void Start()
    {
        player = FindFirstObjectByType<Player>();
        if (player == null)
        {
            Debug.LogError("[InputManager] No Player found in scene!");
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[InputManager] No main camera found!");
                return;
            }
        }

        Debug.Log($"[InputManager] Initialized with surfaceLayer: {LayerMask.LayerToName(surfaceLayer)} and shipLayer: {LayerMask.LayerToName(shipLayer)}");
    }

    private void Update()
    {
        if (player == null) return;

        UpdateHoveredShip();

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            player.SelectNextShip();
        }

        if (Input.GetMouseButtonDown(1)) // Right click
        {
            Ship selectedShip = player.GetSelectedShip();
            if (selectedShip != null)
            {
                HandleRightClickInput(selectedShip);
            }
        }

        if (Input.GetKeyDown(cancelTargetKey))
        {
            CancelCurrentTarget();
        }
    }

    private void UpdateHoveredShip()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxTargetingDistance, shipLayer))
        {
            Ship ship = hit.collider.GetComponent<Ship>();
            if (ship != null && !ship.IsSinking)
            {
                if (hoveredShip != ship)
                {
                    hoveredShip = ship;
                    Debug.Log($"[InputManager] Hovering over ship: {ship.ShipName}");
                }
            }
        }
        else if (hoveredShip != null)
        {
            hoveredShip = null;
        }
    }

    private void HandleRightClickInput(Ship selectedShip)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Check if we're clicking on a ship first
        if (Physics.Raycast(ray, out hit, maxTargetingDistance, shipLayer))
        {
            Ship targetShip = hit.collider.GetComponent<Ship>();
            if (targetShip != null && !targetShip.IsSinking && targetShip != selectedShip)
            {
                HandleCombatInput(selectedShip, targetShip);
                return;
            }
        }

        // If no valid ship target, handle movement
        HandleMovementInput(selectedShip, ray);
    }

    private void HandleCombatInput(Ship selectedShip, Ship targetShip)
    {
        ShipMovement movement = selectedShip.GetComponent<ShipMovement>();
        if (movement == null) return;

        // Check if ships are from different factions
        if (selectedShip.ShipOwner != null && targetShip.ShipOwner != null && 
            selectedShip.ShipOwner.Faction == targetShip.ShipOwner.Faction)
        {
            // For friendly ships, just move to their position
            movement.SetTargetPosition(targetShip.transform.position);
            Debug.Log($"[InputManager] Moving to friendly ship {targetShip.ShipName}'s position");
            return;
        }

        // Set combat target
        movement.SetTargetPosition(targetShip.transform.position, targetShip);
        Debug.Log($"[InputManager] {selectedShip.ShipName} targeting enemy ship {targetShip.ShipName}");
    }

    private void HandleMovementInput(Ship selectedShip, Ray ray)
    {
        ShipMovement movement = selectedShip.GetComponent<ShipMovement>();
        if (movement == null) return;

        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, surfaceLayer))
        {
            targetPoint = hit.point;
        }
        else
        {
            Plane waterPlane = new Plane(Vector3.up, new Vector3(0, waterLevel, 0));
            float enter;
            if (!waterPlane.Raycast(ray, out enter))
            {
                return;
            }
            targetPoint = ray.GetPoint(enter);
        }

        // If ship is in combat and moving far, clear combat
        if (movement.IsInCombat())
        {
            Ship targetShip = movement.GetTargetShip();
            if (targetShip != null && !targetShip.IsSinking)
            {
                float distanceToTarget = Vector3.Distance(targetPoint, targetShip.transform.position);
                if (distanceToTarget > maxTargetingDistance * 1.5f)
                {
                    movement.ClearCombatTarget();
                }
            }
        }

        movement.SetTargetPosition(targetPoint);
        Debug.Log($"[InputManager] Set movement target for {selectedShip.name} to {targetPoint}");
    }

    private void CancelCurrentTarget()
    {
        Ship selectedShip = player.GetSelectedShip();
        if (selectedShip == null) return;

        ShipMovement movement = selectedShip.GetComponent<ShipMovement>();
        if (movement != null)
        {
            movement.ClearCombatTarget();
        }

        if (CombatSystem.Instance != null)
        {
            CombatSystem.Instance.ClearCombatTarget(selectedShip);
            Debug.Log($"[InputManager] Cleared combat target for {selectedShip.ShipName}");
        }
    }
}