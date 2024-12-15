using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Game/Player")]
public class Player : Pirate
{
    private Ship selectedShip;
    [SerializeField, Tooltip("Reference to the InputManager component")]
    private InputManager inputManager;
    [SerializeField, Tooltip("Reference to the UI component for ship selection")]
    private ShipSelectionUI shipSelectionUI;
    
    public event System.Action<Ship> OnShipSelected;
    public event System.Action<Ship> OnShipDeselected;

    public Ship SelectedShip => selectedShip;

    protected override void Start()
    {
        Debug.Log("[Player] Starting initialization");
        
        // Initialize ship list
        ownedShips ??= new List<Ship>();
        Debug.Log($"[Player] Initialized with {ownedShips.Count} owned ships");

        // Find InputManager if not assigned
        InitializeInputManager();
    }

    private void InitializeInputManager()
    {
        if (inputManager != null)
        {
            Debug.Log("[Player] InputManager already assigned");
            return;
        }

        inputManager = GameObject.FindFirstObjectByType<InputManager>();
        if (inputManager == null)
        {
            Debug.LogError("[Player] InputManager not found! Player controls will be disabled.");
        }
        else
        {
            Debug.LogWarning("[Player] InputManager was found in scene but not assigned in inspector. Consider assigning it directly.");
        }
    }

    public override void SelectShip(Ship ship)
    {
        Debug.Log($"[Player] SelectShip called for {(ship != null ? ship.ShipName : "null")}");
        
        if (ship == null)
        {
            Debug.LogError("[Player] Attempting to select a null ship!");
            return;
        }

        if (!ownedShips.Contains(ship))
        {
            Debug.LogError($"[Player] Cannot select ship '{ship.ShipName}' - not owned by player. Owned ships: {ownedShips.Count}");
            foreach (var ownedShip in ownedShips)
            {
                Debug.Log($"[Player] Owned ship: {ownedShip.ShipName}");
            }
            return;
        }

        Debug.Log($"[Player] Selecting ship '{ship.ShipName}'");
        
        Ship previousShip = selectedShip;
        if (selectedShip != null)
        {
            Debug.Log($"[Player] Deselecting previous ship: {selectedShip.ShipName}");
            selectedShip.Deselect();
            OnShipDeselected?.Invoke(selectedShip);
        }

        selectedShip = ship;
        ship.Select();
        OnShipSelected?.Invoke(ship);

        if (shipSelectionUI != null)
        {
            Debug.Log("[Player] Updating UI with new selection");
            shipSelectionUI.UpdateSelection(ship);
        }
        else
        {
            Debug.LogWarning("[Player] No shipSelectionUI assigned!");
        }

        if (inputManager != null)
        {
            Debug.Log("[Player] Notifying InputManager of selection");
            inputManager.OnShipSelected(ship);
        }
        else
        {
            Debug.LogWarning("[Player] No InputManager assigned!");
        }
    }

    public override void AddShip(Ship ship)
    {
        Debug.Log($"[Player] Adding ship {(ship != null ? ship.ShipName : "null")}");
        
        if (ship == null)
        {
            Debug.LogError("[Player] Attempting to add a null ship!");
            return;
        }

        base.AddShip(ship);
        Debug.Log($"[Player] Ship added successfully. Total ships: {ownedShips.Count}");
        
        // Update UI if available
        if (shipSelectionUI != null)
        {
            shipSelectionUI.UpdateShipList(GetOwnedShips());
        }

        // Auto-select if this is the first ship
        if (selectedShip == null && ownedShips.Count == 1)
        {
            Debug.Log("[Player] Auto-selecting first ship");
            SelectShip(ship);
        }
    }

    public override void RemoveShip(Ship ship)
    {
        Debug.Log($"[Player] Removing ship {(ship != null ? ship.ShipName : "null")}");
        
        if (ship == null)
        {
            Debug.LogError("[Player] Attempting to remove a null ship!");
            return;
        }

        if (ship == selectedShip)
        {
            Debug.Log("[Player] Removing currently selected ship");
            selectedShip = null;
            OnShipDeselected?.Invoke(ship);
            inputManager?.OnShipSelected(null);
        }

        base.RemoveShip(ship);
        Debug.Log($"[Player] Ship removed successfully. Remaining ships: {ownedShips.Count}");
        
        if (shipSelectionUI != null)
        {
            shipSelectionUI.UpdateShipList(GetOwnedShips());
        }

        // Auto-select another ship if available
        if (selectedShip == null && ownedShips.Count > 0)
        {
            Debug.Log("[Player] Auto-selecting next available ship");
            SelectShip(ownedShips[0]);
        }
    }

    public void SelectNextShip()
    {
        Debug.Log("[Player] Selecting next ship");
        
        if (ownedShips.Count == 0)
        {
            Debug.Log("[Player] No ships to select");
            return;
        }
        
        int currentIndex = selectedShip != null ? ownedShips.IndexOf(selectedShip) : -1;
        int nextIndex = (currentIndex + 1) % ownedShips.Count;
        
        Debug.Log($"[Player] Switching from index {currentIndex} to {nextIndex}");
        SelectShip(ownedShips[nextIndex]);
    }
    
    public void MoveShipsInFormation(Vector3 targetPosition)
    {
        Debug.Log($"[Player] Moving ships in formation to {targetPosition}");
        
        if (ownedShips.Count == 0)
        {
            Debug.Log("[Player] No ships to move");
            return;
        }
        
        const float horizontalSpacing = 5f;
        const float verticalSpacing = 5f;
        const int shipsPerRow = 3;
        
        for (int i = 0; i < ownedShips.Count; i++)
        {
            Ship ship = ownedShips[i];
            if (ship == null) continue;

            int row = i / shipsPerRow;
            int col = i % shipsPerRow;

            Vector3 offset = new Vector3(
                col * horizontalSpacing - (horizontalSpacing * (shipsPerRow - 1) / 2f),
                0f,
                row * -verticalSpacing
            );
            
            if (ship.TryGetComponent<ShipMovement>(out var movement))
            {
                Debug.Log($"[Player] Setting target position for ship {ship.ShipName} at row {row}, col {col}");
                movement.SetTargetPosition(targetPosition + offset);
            }
        }
    }
    
    protected override void OnDestroy()
    {
        Debug.Log("[Player] Cleaning up");
        // Clean up event listeners
        OnShipSelected = null;
        OnShipDeselected = null;

        base.OnDestroy();
    }
}