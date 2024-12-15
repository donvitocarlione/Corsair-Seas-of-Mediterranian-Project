using UnityEngine;

[RequireComponent(typeof(ShipSelectionHandler))]
public class Ship : MonoBehaviour
{
    [SerializeField]
    private string shipName = "Unnamed Ship";
    [SerializeField]
    private float maxHealth = 100f;
    [SerializeField]
    private float currentHealth;
    
    private ShipSelectionHandler selectionHandler;
    private bool isSelected;
    private bool isSinking;
    private Pirate shipOwner;

    public string ShipName => shipName;
    public Pirate ShipOwner => shipOwner;
    public bool IsSelected => isSelected;
    public bool IsSinking => isSinking;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        Debug.Log($"[Ship] Initializing {shipName}");
        currentHealth = maxHealth;
        selectionHandler = GetComponent<ShipSelectionHandler>();
        
        if (selectionHandler == null)
        {
            Debug.LogError($"[Ship] No ShipSelectionHandler found on {shipName}");
        }
    }

    public void SetOwner(Pirate owner)
    {
        Debug.Log($"[Ship] Setting owner for {shipName} to {(owner != null ? owner.GetType().Name : "null")}");
        shipOwner = owner;
    }

    public bool Select()
    {
        Debug.Log($"[Ship] Attempting to select {shipName}");
        
        if (isSinking)
        {
            Debug.LogWarning($"[Ship] Cannot select {shipName} - ship is sinking");
            return false;
        }

        if (selectionHandler != null && selectionHandler.Select())
        {
            isSelected = true;
            Debug.Log($"[Ship] Successfully selected {shipName}");
            return true;
        }
        
        Debug.LogWarning($"[Ship] Failed to select {shipName}");
        return false;
    }

    public void Deselect()
    {
        Debug.Log($"[Ship] Deselecting {shipName}");
        if (selectionHandler != null)
        {
            selectionHandler.Deselect();
        }
        isSelected = false;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"[Ship] {shipName} taking {damage} damage");
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        Debug.Log($"[Ship] {shipName} health: {currentHealth}/{maxHealth}");

        // Check if ship should start sinking
        if (currentHealth <= 0 && !isSinking)
        {
            StartSinking();
        }
    }

    private void StartSinking()
    {
        Debug.Log($"[Ship] {shipName} starting to sink");
        isSinking = true;
        
        // Deselect if currently selected
        if (isSelected)
        {
            Deselect();
        }

        // Remove from owner's fleet
        if (shipOwner != null)
        {
            Debug.Log($"[Ship] Removing {shipName} from owner's fleet");
            shipOwner.RemoveShip(this);
        }

        // Disable selection handler
        if (selectionHandler != null)
        {
            selectionHandler.enabled = false;
        }
    }

    public void Repair(float amount)
    {
        if (isSinking)
        {
            Debug.LogWarning($"[Ship] Cannot repair {shipName} - ship is sinking");
            return;
        }

        Debug.Log($"[Ship] Repairing {shipName} for {amount}");
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"[Ship] {shipName} health after repair: {currentHealth}/{maxHealth}");
    }

    private void OnValidate()
    {
        // Ensure currentHealth doesn't exceed maxHealth in the inspector
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
}
