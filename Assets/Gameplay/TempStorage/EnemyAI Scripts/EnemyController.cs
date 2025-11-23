using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class EnemyController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform targetPoint;
    public string targetTag = "Castle";
    public string targetName = "";

    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float acceleration = 8f;
    public float stoppingDistance = 2f;
    public bool moveOnStart = true;
    public float angularSpeed = 120f;

    [Header("Health Settings")]
    public float maxHealth = 50f;
    public float currentHealth;
    public bool destroyOnDeath = true;

    [Tooltip("Auto setup child colliders for damage")]
    public bool autoSetupChildColliders = true;

    [Header("Combat Settings")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    public float attackRange = 2f;
    [Tooltip("Tags of objects to attack")]
    public string[] attackableTags = new string[] { "Castle", "Building", "Player" };

    public int currencyAdd;

    [Header("Events")]

    public UnityEvent OnDead;
    public UnityEvent OnHit;
    public UnityEvent OnAttack;

    [Header("Visual Effects")]
    public GameObject attackEffectPrefab;
    public GameObject damageEffectPrefab;

    [Header("Debug")]
    public bool showPath = true;
    public bool showAttackRange = true;
    public Color pathColor = Color.green;

    private NavMeshAgent agent;
    private bool hasReachedDestination = false;
    private bool isAttacking = false;
    private bool isDead = false;
    private float lastAttackTime;
    private Damageable targetDamageable;

    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;

        // Get or add NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
            Debug.Log("NavMeshAgent component added automatically.");
        }

        // Configure agent settings
        agent.speed = moveSpeed;
        agent.acceleration = acceleration;
        agent.stoppingDistance = stoppingDistance;
        agent.angularSpeed = angularSpeed;

        // Setup child colliders
        if (autoSetupChildColliders)
        {
            SetupChildColliders();
        }

        // Auto-find target if not assigned
        if (targetPoint == null)
        {
            FindTarget();
        }

        // Adjust stopping distance for target size
        if (targetPoint != null)
        {
            AdjustStoppingDistanceForTarget();
        }

        if (moveOnStart && targetPoint != null)
        {
            MoveToTarget();
        }

        lastAttackTime = -attackCooldown;
    }

    void SetupChildColliders()
    {
        Collider[] childColliders = GetComponentsInChildren<Collider>();

        foreach (Collider col in childColliders)
        {
            if (col.GetComponent<EnemyDamageForwarder>() == null)
            {
                EnemyDamageForwarder forwarder = col.gameObject.AddComponent<EnemyDamageForwarder>();
                forwarder.parentEnemy = this;
            }
        }
    }

    void FindTarget()
    {
        // Try to find by tag first
        if (!string.IsNullOrEmpty(targetTag))
        {
            GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
            if (targetObj != null)
            {
                targetPoint = targetObj.transform;
                Debug.Log($"{gameObject.name} found target by tag: {targetObj.name}");
                return;
            }
        }

        // Try to find by name if tag didn't work
        if (!string.IsNullOrEmpty(targetName))
        {
            GameObject targetObj = GameObject.Find(targetName);
            if (targetObj != null)
            {
                targetPoint = targetObj.transform;
                Debug.Log($"{gameObject.name} found target by name: {targetObj.name}");
                return;
            }
        }

        Debug.LogWarning($"{gameObject.name}: Could not find target automatically.");
    }

    void Update()
    {
        if (isDead) return;

        // Get target damageable component
        if (targetPoint != null && targetDamageable == null)
        {
            targetDamageable = targetPoint.GetComponent<Damageable>();
        }

        // Check if in attack range
        if (targetPoint != null)
        {
            float distanceToTarget = GetDistanceToTargetEdge();

            if (distanceToTarget <= attackRange)
            {
                // In attack range - stop moving immediately
                if (agent.enabled && !agent.isStopped)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero; // Stop immediately
                    hasReachedDestination = true;
                }

                // Attack if cooldown ready
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                }
            }
            else
            {
                // Out of range - resume moving
                if (agent.enabled && agent.isStopped)
                {
                    agent.isStopped = false;
                    hasReachedDestination = false;
                }

                isAttacking = false;

                // Check if reached stopping distance (but not in attack range)
                if (agent.enabled && !agent.pathPending && agent.hasPath)
                {
                    if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        if (agent.velocity.sqrMagnitude < 0.1f)
                        {
                            if (!hasReachedDestination)
                            {
                                OnReachedDestination();
                            }
                        }
                    }
                    else
                    {
                        hasReachedDestination = false;
                    }
                }
            }
        }
    }

    float GetDistanceToTargetEdge()
    {
        if (targetPoint == null) return float.MaxValue;

        // Calculate distance to center
        float distanceToCenter = Vector3.Distance(transform.position, targetPoint.position);

        // Get target's collider to subtract its radius
        Collider targetCollider = targetPoint.GetComponent<Collider>();
        if (targetCollider != null)
        {
            Bounds bounds = targetCollider.bounds;
            Vector3 closestPoint = bounds.ClosestPoint(transform.position);
            return Vector3.Distance(transform.position, closestPoint);
        }

        return distanceToCenter;
    }

    void Attack()
    {
        if (isDead || targetDamageable == null) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        // Deal damage
        targetDamageable.TakeDamage(attackDamage);
        Debug.Log($"{gameObject.name} attacked {targetPoint.name} for {attackDamage} damage!");
        OnAttack.Invoke();

        // Spawn attack effect
        if (attackEffectPrefab != null)
        {
            Vector3 effectPos = transform.position + transform.forward * 0.5f;
            Instantiate(attackEffectPrefab, effectPos, Quaternion.identity);
        }

        // Trigger animation (if you have animator)
        // GetComponent<Animator>()?.SetTrigger("Attack");
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);
        OnHit.Invoke();

        Debug.Log($"{gameObject.name} took {damageAmount} damage. Health: {currentHealth}/{maxHealth}");

        // Spawn damage effect
        if (damageEffectPrefab != null)
        {
            Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
        }

        // Check for death
        if (currentHealth <= 0)
        {
            OnDead.Invoke();
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{gameObject.name} has been killed!");

        // Stop movement
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }


        // Give player currency
        CurrencyTracker currency = FindAnyObjectByType<CurrencyTracker>();
        if (currency != null)
        {
            currency.Gain(currencyAdd); 
        }
    }

    void OnReachedDestination()
    {
        hasReachedDestination = true;
        Debug.Log($"{gameObject.name} reached destination!");
    }

    public void MoveToTarget()
    {
        if (targetPoint == null)
        {
            Debug.LogWarning("No target point assigned!");
            return;
        }

        MoveToPosition(targetPoint.position);
    }

    public void MoveToPosition(Vector3 position)
    {
        if (agent == null || !agent.enabled || isDead)
        {
            Debug.LogWarning("NavMeshAgent is not available!");
            return;
        }

        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            hasReachedDestination = false;
        }
        else
        {
            Debug.LogWarning("Target position is not on NavMesh!");
        }
    }

    public void SetTarget(Transform target)
    {
        targetPoint = target;
        targetDamageable = null;

        // Adjust stopping distance based on target's collider size
        AdjustStoppingDistanceForTarget();

        MoveToTarget();
    }

    void AdjustStoppingDistanceForTarget()
    {
        if (targetPoint == null) return;

        // Get target's collider to determine size
        Collider targetCollider = targetPoint.GetComponent<Collider>();
        if (targetCollider != null)
        {
            // Calculate distance to edge of collider
            Bounds bounds = targetCollider.bounds;
            float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.z);

            // Set stopping distance to be outside collider but within attack range
            float suggestedStoppingDistance = maxExtent + (attackRange * 0.3f);
            agent.stoppingDistance = Mathf.Max(stoppingDistance, suggestedStoppingDistance);

            Debug.Log($"Adjusted stopping distance to {agent.stoppingDistance} based on target size");
        }
    }

    public bool IsMoving()
    {
        return agent != null && agent.enabled && agent.hasPath && !hasReachedDestination && !isDead;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    // Visualize in scene
    void OnDrawGizmos()
    {
        // Path visualization
        if (showPath && agent != null && agent.hasPath)
        {
            Gizmos.color = pathColor;
            Vector3[] corners = agent.path.corners;

            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
                Gizmos.DrawSphere(corners[i], 0.1f);
            }

            if (corners.Length > 0)
            {
                Gizmos.DrawSphere(corners[corners.Length - 1], 0.1f);
            }
        }

        // Attack range
        if (showAttackRange)
        {
            Gizmos.color = isAttacking ? Color.red : new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (showAttackRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}