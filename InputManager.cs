using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Camera mainCamera;
    private Player player;

    private void Start()
    {
        player = FindObjectOfType<Player>();
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

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Debug.Log($"[InputManager] Ray hit at position: {hit.point}");
            ShipMovement movement = selectedShip.GetComponent<ShipMovement>();
            
            if (movement != null)
            {
                movement.SetTargetPosition(hit.point);
                Debug.Log($"[InputManager] Set target position for {selectedShip.name} to {hit.point}");
            }
            else
            {
                Debug.LogError($"[InputManager] Selected ship {selectedShip.name} has no ShipMovement component!");
            }
        }
        else
        {
            Debug.Log("[InputManager] Ray did not hit ground layer");
        }
    }
}