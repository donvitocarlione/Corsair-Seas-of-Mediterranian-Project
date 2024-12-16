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

    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private float projectileSpeed = 20f;
    [SerializeField]
    private float projectileLifetime = 3f;

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

        Vector3 spawnPosition = attacker.transform.position + attacker.transform.forward * 2f + Vector3.up;
        Vector3 fireDirection = (target.transform.position - spawnPosition).normalized;

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
    }
}