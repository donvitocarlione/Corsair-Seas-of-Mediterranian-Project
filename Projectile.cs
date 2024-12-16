using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    private Ship ownerShip;
    private Ship targetShip;
    private float speed;
    private float damage;
    private float lifetime;
    private float spawnTime;
    private Rigidbody rb;
    private TrailRenderer trailRenderer; // Optional: for visual effect

    [Header("Projectile Settings")]
    [SerializeField] private float trackingSpeed = 2.0f; // How quickly projectile adjusts course
    [SerializeField] private bool useTracking = true; // Whether projectile follows target

    [Header("Visual Effects")]
    [SerializeField] private GameObject hitEffectPrefab; // Optional: effect when hitting something
    [SerializeField] private GameObject waterSplashPrefab; // Optional: effect when hitting water

    public void Initialize(Ship owner, Ship target, float projectileSpeed, float projectileDamage, float projectileLifetime)
    {
        ownerShip = owner;
        targetShip = target;
        speed = projectileSpeed;
        damage = projectileDamage;
        lifetime = projectileLifetime;
        spawnTime = Time.time;

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.velocity = transform.forward * speed;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        else
        {
            Debug.LogError("[Projectile] No Rigidbody component found!");
        }

        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
        }

        Debug.Log($"[Projectile] Initialized: Speed={speed}, Damage={damage}, Lifetime={lifetime}");
    }

    private void Update()
    {
        if (Time.time - spawnTime >= lifetime)
        {
            OnProjectileExpire();
            return;
        }

        if (useTracking && targetShip != null && !targetShip.IsSinking)
        {
            UpdateTracking();
        }
    }

    private void UpdateTracking()
    {
        Vector3 directionToTarget = (targetShip.transform.position - transform.position).normalized;
        rb.velocity = Vector3.Lerp(rb.velocity.normalized, directionToTarget, Time.deltaTime * trackingSpeed) * speed;
        transform.forward = rb.velocity.normalized;
    }

    private void OnTriggerEnter(Collider other)
    {
        Ship hitShip = other.GetComponent<Ship>();

        if (hitShip != null)
        {
            if (hitShip != ownerShip)
            {
                HandleShipHit(hitShip);
            }
        }
        else
        {
            // Hit something else (water, terrain, etc.)
            HandleEnvironmentHit(other);
        }

        DestroyProjectile();
    }

    private void HandleShipHit(Ship hitShip)
    {
        Debug.Log($"[Projectile] Hit ship {hitShip.ShipName} from {ownerShip.ShipName}");
        hitShip.TakeDamage(damage);

        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    private void HandleEnvironmentHit(Collider other)
    {
        Debug.Log($"[Projectile] Hit environment: {other.gameObject.name}");

        // If it hit water level
        if (transform.position.y <= 0)
        {
            if (waterSplashPrefab != null)
            {
                Vector3 splashPos = transform.position;
                splashPos.y = 0;
                Instantiate(waterSplashPrefab, splashPos, Quaternion.identity);
            }
        }
    }

    private void OnProjectileExpire()
    {
        Debug.Log("[Projectile] Lifetime expired");
        DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (targetShip != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetShip.transform.position);
        }
    }
}