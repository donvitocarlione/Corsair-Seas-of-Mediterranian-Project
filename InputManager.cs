// InputManager.cs
using UnityEngine;
using CSM.Base;

public class InputManager : MonoBehaviour
{
    [SerializeField] private LayerMask selectableLayerMask;
    [SerializeField] private LayerMask terrainLayerMask;
        
    private Camera _mainCamera;
    private Ship _cachedSelectedShip;
    public event System.Action<Ship> OnShipSelectionChanged;
    [SerializeField] private KeyCode alternateSelectKey = KeyCode.Mouse1;

     public Ship selectedShip 
    {
        get => _cachedSelectedShip;
       private set 
        {
            if (_cachedSelectedShip != value)
            {
               _cachedSelectedShip = value;
                 OnShipSelectionChanged?.Invoke(_cachedSelectedShip);
            }
        }
   }

    private void Awake()
    {
        _mainCamera = Camera.main;
        if(_mainCamera == null)
        {
            Debug.LogError("Main camera not found in scene!");
            enabled = false;
            return;
        }
         selectableLayerMask = LayerMask.GetMask("Ship");
       terrainLayerMask = LayerMask.GetMask("Default", "Water");
    }

    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
         if (_cachedSelectedShip == null) return;
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayerMask))
            {
                var movement = selectedShip.GetComponent<IMoveable>();
                if (movement != null)
                {
                    movement.SetDestination(hit.point);
                    Debug.Log($"[InputManager] Moving ship to position: {hit.point}");
                }
            }
        }
    }

    private void OnValidate()
    {
       if (selectableLayerMask == 0)
        {
            Debug.LogWarning("Ship layer mask not set in InputManager. Please set it in the inspector.");
        }
       if (terrainLayerMask == 0)
       {
            Debug.LogWarning("Ground layer mask not set in InputManager. Please set it in the inspector.");
        }
    }
}