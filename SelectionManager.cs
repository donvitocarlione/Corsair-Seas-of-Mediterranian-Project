using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    [Header("References")]
    public GameObject selectionIndicatorPrefab;

    private GameObject currentIndicator;
    private bool isInitialized = false;

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
        if (!isInitialized)
        {
            Debug.Log("[SelectionManager] Initializing...");
            if (selectionIndicatorPrefab == null)
            {
                Debug.LogError("[SelectionManager] Selection indicator prefab is not assigned!");
                return;
            }

            Debug.Log("[SelectionManager] Initialization complete");
            isInitialized = true;
        }
    }

    public void ShowSelectionAt(Transform target)
    {
        Debug.Log($"[SelectionManager] Showing selection at {(target != null ? target.name : "null")}");
        if (!isInitialized)
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
            Debug.Log("[SelectionManager] Creating new indicator instance");
            currentIndicator = Instantiate(selectionIndicatorPrefab);
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
        Debug.Log("[SelectionManager] Hiding selection");
        if (currentIndicator != null)
        {
            currentIndicator.SetActive(false);
            currentIndicator.transform.SetParent(null);
            Debug.Log("[SelectionManager] Selection hidden");
        }
        else
        {
            Debug.Log("[SelectionManager] No indicator to hide");
        }
    }

    void OnDestroy()
    {
        if (currentIndicator != null)
        {
            Debug.Log("[SelectionManager] Cleaning up indicator");
            Destroy(currentIndicator);
        }
    }
}
