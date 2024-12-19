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
                instance = Object.FindFirstObjectByType<FiringSystem>();
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

        Transform[] firingPoints = attacker.GetFiringPoints();
        if (firingPoints == null || firingPoints.Length == 0)
        {
            Debug.LogError($"[FiringSystem] No firing points found on {attacker.ShipName}!");
            return;
        }

        foreach (Transform firingPoint in firingPoints)
        {
            // Calculate direction to target with some lead for moving targets
            Vector3 targetPosition = target.transform.position;
            if (target.GetComponent<Rigidbody>() != null)
            {
                // Add basic prediction
                targetPosition += target.GetComponent<Rigidbody>().velocity * (projectileLifetime * 0.5f);
            }

            Vector3 fireDirection = (targetPosition - firingPoint.position).normalized;

            // Check if this firing point is on the correct side to fire
            float angleToTarget = Vector3.Angle(firingPoint.forward, fireDirection);
            if (angleToTarget > attacker.FiringArc * 0.5f)
            {
                continue; // Skip this firing point if it's not facing the target
            }

            // Create the projectile
            GameObject projectileObj = Instantiate(
                projectilePrefab,
                firingPoint.position,
                Quaternion.LookRotation(fireDirection)
            );

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(attacker, target, projectileSpeed, attacker.AttackDamage, projectileLifetime);
            }
            else
            {
                Debug.LogError("[FiringSystem] Projectile prefab missing Projectile component!");
                Destroy(projectileObj);
            }

            // Visual feedback
            CreateMuzzleFlash(firingPoint.position, fireDirection);
        }

        Debug.Log($"[FiringSystem] {attacker.ShipName} fired at {target.ShipName}");
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

        // Draw firing points for selected ships
        Ship[] selectedShips = Object.FindObjectsByType<Ship>(FindObjectsSortMode.None);
        foreach (Ship ship in selectedShips)
        {
            if (ship.IsSelected)
            {
                Transform[] firingPoints = ship.GetFiringPoints();
                if (firingPoints != null)
                {
                    foreach (Transform firingPoint in firingPoints)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireSphere(firingPoint.position, 0.3f);
                        Gizmos.DrawLine(firingPoint.position, firingPoint.position + firingPoint.forward * 2f);
                    }
                }
            }
        }
    }
}