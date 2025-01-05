// SelectionManager.cs
using UnityEngine;
using CSM.Base;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject selectionIndicatorPrefab;

    private ISelectable _currentSelection;
    private GameObject currentIndicator;
    private bool _isInitialized = false;

    void Awake()
    {
        Debug.Log("[SelectionManager] Awake called");
        if (Instance == null)
        {
            Instance = this;
            Initialize();
            Debug.Log("[SelectionManager] Instance initialized");
        }
        else
        {
            Debug.Log("[SelectionManager] Duplicate instance destroyed");
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
         if (!_isInitialized)
        {
            Debug.Log("[SelectionManager] Initializing...");
           if (selectionIndicatorPrefab == null)
            {
                Debug.LogError("[SelectionManager] Selection indicator prefab is not assigned!");
                return;
            }
            _isInitialized = true;
             currentIndicator = GetCurrentIndicator();

            Debug.Log("[SelectionManager] Initialization complete");
        }
    }

    public void Select(ISelectable selectable)
    {
        if (_currentSelection != null)
        {
            _currentSelection.Deselect();
             HideSelection();
        }

       _currentSelection = selectable;
        if (selectable != null)
        {
            ShowSelectionAt(selectable.GetTransform());
            selectable.Select();
        }
    }

    public void ShowSelectionAt(Transform target)
    {
        Debug.Log($"[SelectionManager] Showing selection at {(target != null ? target.name : "null")}");
        if (!_isInitialized)
        {
            Debug.LogWarning("[SelectionManager] Not initialized, cannot show selection");
            return;
        }
       if (target == null)
        {
            Debug.LogWarning("[SelectionManager] Target is null, cannot show selection");
            return;
        }
        if (currentIndicator == null)
        {
             Debug.LogError("[SelectionManager] Current indicator not found, failed to show selection");
            return;
        }
        currentIndicator.transform.position = target.position;
        currentIndicator.transform.SetParent(target);
        currentIndicator.SetActive(true);
         Debug.Log($"[SelectionManager] Indicator positioned at {currentIndicator.transform.position}");
       
       Renderer targetRenderer = target.GetComponent<Renderer>();
        if (targetRenderer != null)
        {
            float targetSize = Mathf.Max(targetRenderer.bounds.size.x, targetRenderer.bounds.size.z);
            SelectionIndicator indicator = currentIndicator.GetComponent<SelectionIndicator>();
            if (indicator != null)
            {
                indicator.UpdateSize(targetSize * 0.6f);
                Debug.Log($"[SelectionManager] Indicator size updated to {targetSize * 0.6f}");
            }
            else
            {
                Debug.LogWarning("[SelectionManager] Selection indicator component not found on prefab");
            }
        }
       else
        {
            Debug.LogWarning($"[SelectionManager] No renderer found on target {target.name}");
        }
    }

     public void HideSelection()
    {
         if (currentIndicator != null)
        {
           Debug.Log("[SelectionManager] Hiding selection");
           currentIndicator.SetActive(false);
          currentIndicator.transform.SetParent(null);
           Debug.Log("[SelectionManager] Selection hidden");
       }
        else
        {
           Debug.Log("[SelectionManager] No indicator to hide");
        }
    }
    private GameObject GetCurrentIndicator()
    {
        if (currentIndicator != null) return currentIndicator;
       
        if (selectionIndicatorPrefab != null)
        {
            currentIndicator = Instantiate(selectionIndicatorPrefab, transform);
            return currentIndicator;
        }
        return null;
    }
    void OnDestroy()
    {
         Debug.Log("[SelectionManager] Cleaning up on destroy");
         if (currentIndicator != null)
        {
           Debug.Log("[SelectionManager] Cleaning up indicator");
           Destroy(currentIndicator);
        }
    }
}