using UnityEngine;
using System.Collections.Generic;
using static ShipExtensions;
using CSM.Base;
using System.Linq;

[AddComponentMenu("Game/Player")]
public class Player : Pirate, IEntityOwner
{
    private ISelectable selectedShip;
     [SerializeField, Tooltip("Reference to the InputManager component")]
    private InputManager inputManager;
    [SerializeField, Tooltip("Reference to the UI component for ship selection")]
   private ShipSelectionUI shipSelectionUI;

   public event System.Action<Ship> OnShipSelected;
    public event System.Action<Ship> OnShipDeselected;

    public Ship SelectedShip => selectedShip as Ship;

    //Implement IEntityOwner
    public new string OwnerName => EntityName;

    protected override void Start()
    {
        // Initialize ship list
       ownedShips ??= new List<Ship>();

        // Find InputManager if not assigned
       InitializeInputManager();
        base.Start();
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
           Debug.LogWarning($"Cannot select ship '{ship.ShipName()}' - not owned by player");
            return;
       }

        // Cast to ISelectable and use interface
       ISelectable selectableShip = ship as ISelectable;
        if (selectableShip == null) return;

       // Deselect current selection
        if (selectedShip != null)
        {
            selectedShip.Deselect();
           OnShipDeselected?.Invoke(selectedShip as Ship);
        }

        // Select new ship
        selectedShip = selectableShip;
        selectableShip.Select();
       OnShipSelected?.Invoke(ship);

        // Update UI and input manager
        shipSelectionUI?.UpdateSelection(ship);
       inputManager?.OnShipSelected(ship);
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

           if (ship.TryGetComponent<IMoveable>(out var movement))
           {
               movement.SetDestination(targetPosition + offset);
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

    public override void Initialize(PirateData data)
   {
        base.Initialize(data);

       // Player specific initialization
        InitializeInputManager();

        // Set up any player-specific UI or controls
        if (shipSelectionUI == null)
        {
            shipSelectionUI = FindObjectOfType<ShipSelectionUI>(); // Using old version because is not breaking anything
            if (shipSelectionUI == null)
                 Debug.LogWarning("ShipSelectionUI not found for player!");
        }
    }
}