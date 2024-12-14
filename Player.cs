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
        // Initialize ship list
        ownedShips ??= new List<Ship>();

        // Find InputManager if not assigned
        InitializeInputManager();
    }

    private void InitializeInputManager()
    {
        if (inputManager != null) return;

        inputManager = GameObject.FindFirstObjectByType<InputManager>();
        if (inputManager == null)
        {
            Debug.LogError("InputManager not found! Player controls will be disabled.");
        }
        else
        {
            Debug.LogWarning("InputManager was found in scene but not assigned in inspector. Consider assigning it directly.");
        }
    }

    public override void SelectShip(Ship ship)
    {
        if (ship == null)
        {
            Debug.LogError("Attempting to select a null ship!");
            return;
        }

        if (!ownedShips.Contains(ship))
        {
            Debug.LogWarning($"Cannot select ship '{ship.ShipName}' - not owned by player");
            return;
        }

        // Cache previous selection
        Ship previousShip = selectedShip;
        
        // Deselect current ship if any
        if (selectedShip != null)
        {
            selectedShip.Deselect();
            OnShipDeselected?.Invoke(selectedShip);
        }

        // Select new ship
        selectedShip = ship;
        ship.Select();
        OnShipSelected?.Invoke(ship);

        // Update UI if available
        shipSelectionUI?.UpdateSelection(ship);

        // Notify input manager if available
        inputManager?.OnShipSelected(ship);
    }

    public override void AddShip(Ship ship)
    {
        if (ship == null)
        {
            Debug.LogError("Attempting to add a null ship!");
            return;
        }

        base.AddShip(ship);
        
        // Update UI if available
        shipSelectionUI?.UpdateShipList(GetOwnedShips());

        // Auto-select if this is the first ship
        if (selectedShip == null && ownedShips.Count == 1)
        {
            SelectShip(ship);
        }
    }

    public override void RemoveShip(Ship ship)
    {
        if (ship == null)
        {
            Debug.LogError("Attempting to remove a null ship!");
            return;
        }

        if (ship == selectedShip)
        {
            selectedShip = null;
            OnShipDeselected?.Invoke(ship);
            inputManager?.OnShipSelected(null);
        }

        base.RemoveShip(ship);
        
        shipSelectionUI?.UpdateShipList(GetOwnedShips());

        // Auto-select another ship if available
        if (selectedShip == null && ownedShips.Count > 0)
        {
            SelectShip(ownedShips[0]);
        }
    }

    public void SelectNextShip()
    {
        if (ownedShips.Count == 0) return;
        
        int currentIndex = selectedShip != null ? ownedShips.IndexOf(selectedShip) : -1;
        int nextIndex = (currentIndex + 1) % ownedShips.Count;
        
        SelectShip(ownedShips[nextIndex]);
    }
    
    public void MoveShipsInFormation(Vector3 targetPosition)
    {
        if (ownedShips.Count == 0) return;
        
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
                movement.SetTargetPosition(targetPosition + offset);
            }
        }
    }
    
    protected override void OnDestroy()
    {
        // Clean up event listeners
        OnShipSelected = null;
        OnShipDeselected = null;

        base.OnDestroy();
    }
}
