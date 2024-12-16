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
        }
        else
        {
            Debug.LogError("[Projectile] No Rigidbody component found!");
        }
    }

    private void Update()
    {
        if (Time.time - spawnTime >= lifetime)
        {
            Debug.Log("[Projectile] Lifetime expired, destroying");
            Destroy(gameObject);
            return;
        }

        // Optional: Add tracking behavior for more advanced projectiles
        if (targetShip != null && !targetShip.IsSinking)
        {
            Vector3 directionToTarget = (targetShip.transform.position - transform.position).normalized;
            float trackingSpeed = 2.0f; // Adjust this value to control how quickly the projectile tracks
            rb.velocity = Vector3.Lerp(rb.velocity.normalized, directionToTarget, Time.deltaTime * trackingSpeed) * speed;
            transform.forward = rb.velocity.normalized;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Ship hitShip = collision.gameObject.GetComponent<Ship>();

        if (hitShip != null)
        {
            // Don't damage the ship that fired the projectile
            if (hitShip != ownerShip)
            {
                HandleShipHit(hitShip);
            }
        }
        else
        {
            // Hit something else (water, terrain, etc.)
            HandleEnvironmentHit(collision);
        }

        // Destroy the projectile regardless of what it hit
        Destroy(gameObject);
    }

    private void HandleShipHit(Ship hitShip)
    {
        Debug.Log($"[Projectile] Hit ship {hitShip.ShipName} from {ownerShip.ShipName}");

        // Apply damage to the hit ship
        hitShip.TakeDamage(damage);

        // Spawn hit effect if we have one
        SpawnHitEffect(true);

        // Could trigger sound effects here
        PlayHitSound(true);
    }

    private void HandleEnvironmentHit(Collision collision)
    {
        Debug.Log($"[Projectile] Hit environment at {collision.contacts[0].point}");

        // Spawn water splash or other environmental effect
        SpawnHitEffect(false);

        // Play appropriate sound effect
        PlayHitSound(false);
    }

    private void SpawnHitEffect(bool isShipHit)
    {
        // TODO: Implement hit effects
        // Example:
        // if (isShipHit && shipHitEffectPrefab != null)
        // {
        //     Instantiate(shipHitEffectPrefab, transform.position, Quaternion.identity);
        // }
        // else if (!isShipHit && waterHitEffectPrefab != null)
        // {
        //     Instantiate(waterHitEffectPrefab, transform.position, Quaternion.identity);
        // }
    }

    private void PlayHitSound(bool isShipHit)
    {
        // TODO: Implement sound effects
        // Example:
        // AudioSource.PlayClipAtPoint(isShipHit ? shipHitSound : waterHitSound, transform.position);
    }

    private void OnDrawGizmos()
    {
        // Draw debug visualization in editor
        if (targetShip != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetShip.transform.position);
        }
    }
}