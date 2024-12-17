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
    [SerializeField] private float maxTargetingDistance = 50f;
    [SerializeField] private KeyCode attackKey = KeyCode.Space;
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
            Debug.Log("[InputManager] Tab pressed - selecting next ship");
            player.SelectNextShip();
        }

        if (Input.GetMouseButtonDown(1)) // Right click
        {
            Debug.Log("[InputManager] Right click detected - handling movement/target input");
            HandleMovementOrTargetInput();
        }

        if (Input.GetKeyDown(attackKey))
        {
            HandleAttackInput();
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

    private void HandleMovementOrTargetInput()
    {
        Ship selectedShip = player.GetSelectedShip();
        if (selectedShip == null)
        {
            Debug.LogWarning("[InputManager] No ship selected for movement/targeting");
            return;
        }

        Debug.Log($"[InputManager] Handling input for selected ship: {selectedShip.ShipName}");
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Debug.Log($"[InputManager] Mouse Position: {Input.mousePosition}");
        RaycastHit hit;

        // First check for target ships
        if (Physics.Raycast(ray, out hit, maxTargetingDistance, shipLayer))
        {
            Debug.Log($"[InputManager] Hit ship layer at point {hit.point}, object: {hit.collider.gameObject.name}, layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            Ship targetShip = hit.collider.GetComponent<Ship>();
            if (targetShip != null && !targetShip.IsSinking && targetShip != selectedShip)
            {
                HandleTargetSelection(selectedShip, targetShip, hit.point);
                return;
            }
        }

        // If no ship hit, handle movement
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, surfaceLayer))
        {
            Debug.Log($"[InputManager] Hit surface at point {hit.point}, object: {hit.collider.gameObject.name}");
            SetShipTargetPosition(selectedShip, hit.point);
        }
        else
        {
            // Project to water plane if no surface hit
            Plane waterPlane = new Plane(Vector3.up, new Vector3(0, waterLevel, 0));
            float enter;
            if (waterPlane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                Debug.Log($"[InputManager] Projected to water plane at point {hitPoint}");
                SetShipTargetPosition(selectedShip, hitPoint);
            }
            else
            {
                Debug.LogWarning("[InputManager] Could not determine target position - ray did not intersect water plane");
            }
        }
    }

    private void HandleTargetSelection(Ship attacker, Ship target, Vector3 targetPoint)
    {
        if (attacker.ShipOwner != null && target.ShipOwner != null && 
            attacker.ShipOwner.Faction == target.ShipOwner.Faction)
        {
            Debug.Log($"[InputManager] Cannot target friendly ship {target.ShipName} (same faction)");
            return;
        }

        ShipMovement movement = attacker.GetComponent<ShipMovement>();
        if (movement != null)
        {
            movement.SetTargetPosition(targetPoint, target);
            Debug.Log($"[InputManager] {attacker.ShipName} targeting {target.ShipName} at position {targetPoint}");
        }
        else
        {
            Debug.LogError($"[InputManager] {attacker.ShipName} has no ShipMovement component!");
        }
    }

    private void HandleAttackInput()
    {
        Ship selectedShip = player.GetSelectedShip();
        if (selectedShip == null) return;

        Ship currentTarget = null;
        if (CombatSystem.Instance != null)
        {
            currentTarget = CombatSystem.Instance.GetCurrentTarget(selectedShip);
        }

        if (currentTarget != null && selectedShip.CanFire)
        {
            Debug.Log($"[InputManager] {selectedShip.ShipName} attempting to fire at {currentTarget.ShipName}");
            selectedShip.Fire(currentTarget);
        }
        else
        {
            Debug.Log($"[InputManager] Cannot fire: Target={currentTarget != null}, CanFire={selectedShip.CanFire}");
        }
    }

    private void CancelCurrentTarget()
    {
        Ship selectedShip = player.GetSelectedShip();
        if (selectedShip == null) return;

        if (CombatSystem.Instance != null)
        {
            CombatSystem.Instance.ClearCombatTarget(selectedShip);
            Debug.Log($"[InputManager] Cleared combat target for {selectedShip.ShipName}");
        }
    }

    private void SetShipTargetPosition(Ship ship, Vector3 position)
    {
        ShipMovement movement = ship.GetComponent<ShipMovement>();
        if (movement != null)
        {
            position.y = waterLevel;
            movement.SetTargetPosition(position);
            Debug.Log($"[InputManager] Set target position for {ship.name} to {position}");

            // Clear any existing combat target
            if (CombatSystem.Instance != null)
            {
                CombatSystem.Instance.ClearCombatTarget(ship);
            }
        }
        else
        {
            Debug.LogError($"[InputManager] Selected ship {ship.name} has no ShipMovement component!");
        }
    }
}