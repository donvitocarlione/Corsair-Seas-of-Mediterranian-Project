using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShipSelectionHandler : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer[] targetRenderers;
    [SerializeField]
    private GameObject selectionIndicator;
    [SerializeField]
    private LayerMask selectableLayerMask = Physics.DefaultRaycastLayers;
    [SerializeField]
    private Material selectedMaterial;
    
    private Material[] originalMaterials;
    private Ship shipReference;
    private ShipMovement movementComponent;
    
    private void OnEnable()
    {
        Debug.Log($"[ShipSelectionHandler] OnEnable start for {gameObject.name}");
        // Initialize original materials if not already done
        if (originalMaterials == null || originalMaterials.Length != targetRenderers.Length)
        {
            StoreOriginalMaterials();
        }
    }
    
    private void Awake()
    {
        shipReference = GetComponent<Ship>();
        movementComponent = GetComponent<ShipMovement>();
        
        if (shipReference == null)
        {
            Debug.LogError($"[ShipSelectionHandler] No Ship component found on {gameObject.name}");
            return;
        }

        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            Debug.Log("[ShipSelectionHandler] No target renderers assigned, auto-finding renderers");
            targetRenderers = GetComponentsInChildren<MeshRenderer>();
        }

        StoreOriginalMaterials();

        // Ensure this object is on the correct layer
        if (gameObject.layer != LayerMask.NameToLayer("Ship"))
        {
            SetLayerRecursively(gameObject, LayerMask.NameToLayer("Ship"));
        }
    }

    private void StoreOriginalMaterials()
    {
        originalMaterials = new Material[targetRenderers.Length];
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            if (targetRenderers[i] != null)
            {
                originalMaterials[i] = targetRenderers[i].material;
            }
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public bool Select()
    {
        if (!CanBeSelected())
        {
            return false;
        }

        ApplySelectedMaterial();
        ShowSelectionIndicator(true);
        return true;
    }

    public void Deselect()
    {
        RestoreOriginalMaterials();
        ShowSelectionIndicator(false);
    }

    private bool CanBeSelected()
    {
        if (shipReference == null)
        {
            Debug.LogError($"[ShipSelectionHandler] Cannot select - shipReference is null on {gameObject.name}");
            return false;
        }

        if (shipReference.ShipOwner == null)
        {
            Debug.LogError($"[ShipSelectionHandler] Cannot select - ship has no owner on {gameObject.name}");
            return false;
        }

        if (!(shipReference.ShipOwner is Player))
        {
            Debug.LogWarning($"[ShipSelectionHandler] Cannot select - ship's owner is not a Player on {gameObject.name}");
            return false;
        }

        return true;
    }

    private void ApplySelectedMaterial()
    {
        if (selectedMaterial != null)
        {
            foreach (var renderer in targetRenderers)
            {
                if (renderer != null)
                {
                    renderer.material = selectedMaterial;
                }
            }
        }
    }

    private void RestoreOriginalMaterials()
    {
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            if (targetRenderers[i] != null && originalMaterials[i] != null)
            {
                targetRenderers[i].material = originalMaterials[i];
            }
        }
    }

    private void ShowSelectionIndicator(bool show)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(show);
        }
    }

    private void OnMouseDown()
    {
        if (Camera.main == null) return;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayerMask))
        {
            if (hit.collider.gameObject != gameObject) return;

            if (CanBeSelected() && shipReference.ShipOwner is Player player)
            {
                player.SelectShip(shipReference);
            }
        }
    }

    private void OnDestroy()
    {
        if (Application.isPlaying)
        {
            foreach (var material in originalMaterials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
        }
    }
}