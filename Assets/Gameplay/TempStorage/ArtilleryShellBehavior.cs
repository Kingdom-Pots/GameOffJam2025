using UnityEngine;

public class ArtilleryShellBehavior: MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Damage dealt on direct hit")]
    public float damage = 50f;

    [Tooltip("Explosion radius prefab (uses its scale for size)")]
    public Transform explosionRadiusPrefab;

    [Tooltip("Manual explosion radius (used if no prefab assigned)")]
    public float explosionRadius = 3f;

    [Tooltip("Maximum damage distance from center")]
    public float maxDamageDistance = 1f;

    [Tooltip("Use damage falloff with distance")]
    public bool useDamageFalloff = true;

    [Header("Layers")]
    [Tooltip("Layers that can be damaged")]
    public LayerMask damageableLayers = -1;

    [Header("Destruction")]
    [Tooltip("Destroy shell after impact")]
    public bool destroyOnImpact = true;

    [Tooltip("Delay before destroying")]
    public float destroyDelay = 0.1f;

    private bool hasExploded = false;
    private float currentExplosionRadius;

    void Start()
    {
        // Calculate explosion radius from prefab if assigned
        UpdateExplosionRadius();
    }

    void UpdateExplosionRadius()
    {
        if (explosionRadiusPrefab != null)
        {
            // Use the average scale of the prefab as radius
            Vector3 scale = explosionRadiusPrefab.localScale;
            currentExplosionRadius = (scale.x + scale.y + scale.z) / 3f;
            Debug.Log($"Explosion radius set from prefab: {currentExplosionRadius}");
        }
        else
        {
            currentExplosionRadius = explosionRadius;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasExploded)
        {
            Explode(collision.contacts[0].point);
        }
    }

    void Explode(Vector3 explosionPoint)
    {
        hasExploded = true;

        // Update radius in case prefab changed
        UpdateExplosionRadius();

        Debug.Log($"Artillery shell exploded at {explosionPoint} with radius {currentExplosionRadius}");

        // Deal damage
        if (currentExplosionRadius > 0)
        {
            // Area damage
            DealAreaDamage(explosionPoint);
        }

        // Destroy shell
        if (destroyOnImpact)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    void DealAreaDamage(Vector3 center)
    {
        // Find all colliders in explosion radius
        Collider[] hitColliders = Physics.OverlapSphere(center, currentExplosionRadius, damageableLayers);

        foreach (Collider hitCollider in hitColliders)
        {
            // Try to damage enemies
            EnemyController enemy = hitCollider.GetComponent<EnemyController>();
            if (enemy != null && !enemy.IsDead())
            {
                float distance = Vector3.Distance(center, hitCollider.transform.position);
                float damageAmount = CalculateDamage(distance);
                enemy.TakeDamage(damageAmount);
                Debug.Log($"Damaged {hitCollider.name} for {damageAmount} damage");
            }

            // Try to damage other damageable objects
            Damageable damageable = hitCollider.GetComponentInParent<Damageable>();
            if (damageable != null && !damageable.IsDead())
            {
                float distance = Vector3.Distance(center, hitCollider.transform.position);
                float damageAmount = CalculateDamage(distance);
                damageable.TakeDamage(damageAmount);
                Debug.Log($"Damaged {hitCollider.name} for {damageAmount} damage");
            }
        }
    }

    float CalculateDamage(float distance)
    {
        if (!useDamageFalloff)
        {
            return damage;
        }

        // Full damage within maxDamageDistance
        if (distance <= maxDamageDistance)
        {
            return damage;
        }

        // Linear falloff from maxDamageDistance to explosionRadius
        float falloffDistance = currentExplosionRadius - maxDamageDistance;
        float damagePercent = 1f - ((distance - maxDamageDistance) / falloffDistance);
        damagePercent = Mathf.Clamp01(damagePercent);

        return damage * damagePercent;
    }

    // Visualize explosion radius
    void OnDrawGizmosSelected()
    {
        // Update radius for gizmo display
        if (explosionRadiusPrefab != null)
        {
            Vector3 scale = explosionRadiusPrefab.localScale;
            currentExplosionRadius = (scale.x + scale.y + scale.z) / 3f;
        }
        else
        {
            currentExplosionRadius = explosionRadius;
        }

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, currentExplosionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, currentExplosionRadius);

        // Max damage radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDamageDistance);
    }
}
