using UnityEngine;

public class FiringSystem : MonoBehaviour
{
    private static FiringSystem instance;
    public static FiringSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<FiringSystem>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("FiringSystem");
                    instance = obj.AddComponent<FiringSystem>();
                }
            }
            return instance;
        }
    }

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float projectileLifetime = 3f;
    [SerializeField] private float projectileSpawnOffset = 2f; // Distance in front of ship to spawn projectile

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugGizmos = true;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void FireProjectile(Ship attacker, Ship target)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("[FiringSystem] No projectile prefab assigned!");
            return;
        }

        // Calculate spawn position in front of the ship
        Vector3 spawnPosition = attacker.transform.position + 
                              (attacker.transform.forward * projectileSpawnOffset) + 
                              Vector3.up; // Slight upward offset

        // Calculate direction to target with some lead for moving targets
        Vector3 targetPosition = target.transform.position;
        if (target.GetComponent<Rigidbody>() != null)
        {
            // Add basic prediction
            targetPosition += target.GetComponent<Rigidbody>().velocity * (projectileLifetime * 0.5f);
        }

        Vector3 fireDirection = (targetPosition - spawnPosition).normalized;

        // Create the projectile
        GameObject projectileObj = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.LookRotation(fireDirection)
        );

        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(attacker, target, projectileSpeed, attacker.AttackDamage, projectileLifetime);
            Debug.Log($"[FiringSystem] {attacker.ShipName} fired projectile at {target.ShipName}");
        }
        else
        {
            Debug.LogError("[FiringSystem] Projectile prefab missing Projectile component!");
            Destroy(projectileObj);
        }

        // Visual feedback
        CreateMuzzleFlash(spawnPosition, fireDirection);
    }

    private void CreateMuzzleFlash(Vector3 position, Vector3 direction)
    {
        // TODO: Add particle effect for muzzle flash
        // For now, just draw debug line
        if (showDebugGizmos)
        {
            Debug.DrawRay(position, direction * 5f, Color.yellow, 0.5f);
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw projectile spawn points for selected ships
        Ship[] selectedShips = FindObjectsByType<Ship>(FindObjectsSortMode.None);
        foreach (Ship ship in selectedShips)
        {
            if (ship.IsSelected)
            {
                Vector3 spawnPos = ship.transform.position + 
                                  (ship.transform.forward * projectileSpawnOffset) + 
                                  Vector3.up;
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(spawnPos, 0.5f);
                Gizmos.DrawLine(ship.transform.position, spawnPos);
            }
        }
    }
}