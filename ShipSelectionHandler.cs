using UnityEngine;

public class ShipSelectionHandler : MonoBehaviour
{
    [SerializeField] private Material selectedMaterial;
    private MeshRenderer[] targetRenderers;
    private Material[] originalMaterials;
    private SelectionManager selectionManager;
    private Ship ship;

    private void Awake()
    {
        ship = GetComponent<Ship>();
        targetRenderers = GetComponentsInChildren<MeshRenderer>();
        
        // Store original materials
        originalMaterials = new Material[targetRenderers.Length];
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            if (targetRenderers[i] != null)
            {
                originalMaterials[i] = targetRenderers[i].material;
                Debug.Log($"[ShipSelectionHandler] Stored original material for {targetRenderers[i].name}");
            }
        }

        selectionManager = FindFirstObjectByType<SelectionManager>();
        if (selectionManager == null)
        {
            Debug.LogError("[ShipSelectionHandler] SelectionManager not found in scene!");
        }
    }

    public bool Select()
    {
        ApplySelectedMaterial();
        if (selectionManager != null)
        {
            selectionManager.ShowSelectionAt(transform);
        }
        return true;
    }

    public void Deselect()
    {
        RestoreOriginalMaterials();
        if (selectionManager != null)
        {
            selectionManager.HideSelection();
        }
    }

    private void OnMouseDown()
    {
        if (ship != null && ship.ShipOwner is Player)
        {
            Debug.Log($"[ShipSelectionHandler] Ship {gameObject.name} clicked");
            Player player = (Player)ship.ShipOwner;
            player.SelectShip(ship);
            Select();
        }
        else
        {
            Debug.LogWarning($"[ShipSelectionHandler] Ship {gameObject.name} clicked but either ship is null or owner is not Player");
        }
    }

    private void ApplySelectedMaterial()
    {
        if (selectedMaterial == null)
        {
            Debug.LogError($"[ShipSelectionHandler] Selected material is null on {gameObject.name}");
            return;
        }

        if (targetRenderers == null)
        {
            Debug.LogError($"[ShipSelectionHandler] targetRenderers array is null when applying material on {gameObject.name}");
            return;
        }

        foreach (var renderer in targetRenderers)
        {
            if (renderer != null)
            {
                // Instantiate a new material to avoid overwriting the base one
                Material instanceMaterial = new Material(selectedMaterial);
                renderer.material = instanceMaterial;
                Debug.Log($"[ShipSelectionHandler] Applied selected material instance to {renderer.name}");
            }
            else
            {
                Debug.LogWarning($"[ShipSelectionHandler] Target renderer in {gameObject.name} is null when applying material.");
            }
        }
    }

    private void RestoreOriginalMaterials()
    {
        if (targetRenderers == null)
        {
            Debug.LogError($"[ShipSelectionHandler] targetRenderers array is null when restoring material on {gameObject.name}");
            return;
        }

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            if (targetRenderers[i] != null && originalMaterials[i] != null)
            {
                targetRenderers[i].material = originalMaterials[i];
                Debug.Log($"[ShipSelectionHandler] Restored original material to {targetRenderers[i].name}");
            }
        }
    }

    private void OnDestroy()
    {
        RestoreOriginalMaterials();
    }
}