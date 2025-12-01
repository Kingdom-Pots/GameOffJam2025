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
    public float angularSpeed = 120f;
    public bool moveOnStart = true;

    [Header("Spread Settings")]
    [Tooltip("Random speed variation range (e.g., 0.5 means ±0.5 units/sec)")]
    public float speedRandomRange = 0.5f;
    [Tooltip("Time between path offset updates (seconds)")]
    public float pathOffsetUpdateInterval = 1.5f;
    [Tooltip("Maximum random offset from path (units)")]
    public float maxPathOffset = 2f;
    [Tooltip("Random radius around target point (units)")]
    public float targetOffsetRadius = 3f;

    [Header("Health Settings")]
    public float maxHealth = 50f;
    public float currentHealth;
    public bool destroyOnDeath = true;

    [Header("Combat Settings")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    public float attackRange = 2f;
    [Tooltip("Tags of objects to attack")]
    public string[] attackableTags = new string[] { "Castle", "Building", "Player" };

    [Header("Stagger Settings")]
    public float staggerDuration = 0.5f;
    public float staggerSpeedMultiplier = 0.2f;

    [Header("Rewards")]
    public int currencyAdd;
    [Tooltip("Maximum bonus currency when enemy is killed far from target")]
    public int maxDistanceBonus = 10;
    [Tooltip("Minimum currency multiplier (0-1). At stopping distance, reward = currencyAdd * minMultiplier")]
    [Range(0f, 1f)]
    public float minMultiplier = 0f;
    [Tooltip("Distance threshold for maximum bonus (units from target)")]
    public float maxBonusDistance = 100f;

    [Header("Setup")]
    [Tooltip("Auto setup child colliders for damage")]
    public bool autoSetupChildColliders = true;

    [Header("Events")]
    public UnityEvent OnDead;
    public UnityEvent OnHit;
    public UnityEvent OnAttack;
    public UnityEvent OnMove;
    public UnityEvent OnStopMoving;
    public UnityEvent OnStagger;

    [Header("Visual Effects")]
    public GameObject attackEffectPrefab;
    public GameObject damageEffectPrefab;

    [Header("Debug")]
    public bool showPath = true;
    public bool showAttackRange = true;
    public Color pathColor = Color.green;

    // Private state
    private NavMeshAgent agent;
    private Damageable targetDamageable;
    
    private bool hasReachedDestination = false;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isStaggered = false;
    private bool wasMovingLastFrame = false;
    
    private float lastAttackTime;
    private float staggerEndTime;
    private float originalSpeed;
    private float speedRandomFactor;
    
    // Spread system
    private Vector3 targetOffset;
    private Vector3 currentPathOffset;
    private float nextPathOffsetTime;

    #region Initialization

    void Start()
    {
        InitializeHealth();
        InitializeSpreadFactors();
        InitializeAgent();
        
        if (autoSetupChildColliders)
        {
            SetupChildColliders();
        }

        InitializeTarget();
        
        lastAttackTime = -attackCooldown;
    }

    void InitializeHealth()
    {
        currentHealth = maxHealth;
    }

    void InitializeSpreadFactors()
    {
        // Random speed variation
        speedRandomFactor = Random.Range(-speedRandomRange, speedRandomRange);
        
        // Random target offset (circular distribution)
        Vector2 randomCircle = Random.insideUnitCircle * targetOffsetRadius;
        targetOffset = new Vector3(randomCircle.x, 0, randomCircle.y);
        
        // Initialize path offset
        currentPathOffset = Vector3.zero;
        nextPathOffsetTime = Time.time + pathOffsetUpdateInterval;
    }

    void InitializeAgent()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
            Debug.Log("NavMeshAgent component added automatically.");
        }

        // Apply speed with random factor
        float finalSpeed = moveSpeed + speedRandomFactor;
        agent.speed = finalSpeed;
        agent.acceleration = acceleration;
        agent.stoppingDistance = stoppingDistance;
        agent.angularSpeed = angularSpeed;
        
        originalSpeed = finalSpeed;
        
        Debug.Log($"{gameObject.name} speed set to {finalSpeed} (base: {moveSpeed}, factor: {speedRandomFactor:F2})");
    }

    void InitializeTarget()
    {
        if (targetPoint == null)
        {
            FindTarget();
        }

        if (targetPoint != null)
        {
            AdjustStoppingDistanceForTarget();
            
            if (moveOnStart)
            {
                MoveToTarget();
            }
        }
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

    #endregion

    #region Update Loop

    void Update()
    {
        if (isDead) return;

        UpdateStagger();
        UpdatePathOffset();
        UpdateTargetDamageable();
        UpdateCombatBehavior();
        UpdateMovementEvents();
    }

    void UpdateStagger()
    {
        if (isStaggered && Time.time >= staggerEndTime)
        {
            EndStagger();
        }
    }

    void UpdatePathOffset()
    {
        // Only apply offset while moving and not in combat and not close to target
        if (Time.time >= nextPathOffsetTime && IsMoving() && !isAttacking)
        {
            // Don't apply path offset if we're close to the target
            float distanceToTarget = GetDistanceToTargetEdge();
            if (distanceToTarget > attackRange * 2f) // Only offset when far from target
            {
                ApplyRandomPathOffset();
            }
            nextPathOffsetTime = Time.time + pathOffsetUpdateInterval;
        }
    }

    void ApplyRandomPathOffset()
    {
        if (targetPoint == null || agent == null || !agent.enabled) return;

        // Generate random offset perpendicular to movement direction
        Vector2 randomCircle = Random.insideUnitCircle * maxPathOffset;
        Vector3 newOffset = new Vector3(randomCircle.x, 0, randomCircle.y);
        
        // Calculate offset position
        Vector3 offsetPosition = targetPoint.position + targetOffset + newOffset;
        
        // Validate position is on NavMesh
        if (NavMesh.SamplePosition(offsetPosition, out NavMeshHit hit, maxPathOffset * 2f, NavMesh.AllAreas))
        {
            currentPathOffset = newOffset;
            agent.SetDestination(hit.position);
        }
    }

    void UpdateTargetDamageable()
    {
        if (targetPoint != null && targetDamageable == null)
        {
            targetDamageable = targetPoint.GetComponent<Damageable>();
        }
    }

    void UpdateCombatBehavior()
    {
        if (targetPoint == null || !agent.enabled) return;

        float distanceToTarget = GetDistanceToTargetEdge();

        // Debug logging to diagnose issues
        if (hasReachedDestination && !isAttacking && Time.frameCount % 60 == 0)
        {
            Debug.Log($"{gameObject.name} - Distance: {distanceToTarget:F2}, AttackRange: {attackRange}, " +
                     $"HasDamageable: {targetDamageable != null}, IsStaggered: {isStaggered}, " +
                     $"CanAttack: {Time.time >= lastAttackTime + attackCooldown}");
        }

        if (distanceToTarget <= attackRange)
        {
            HandleInAttackRange();
        }
        else
        {
            HandleOutOfAttackRange(distanceToTarget);
        }
    }

    void HandleInAttackRange()
    {
        if (!agent.isStopped)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            hasReachedDestination = true;
        }

        // Check if we can attack
        if (Time.time >= lastAttackTime + attackCooldown && !isStaggered)
        {
            // If targetDamageable is null, try to get it again
            if (targetDamageable == null && targetPoint != null)
            {
                targetDamageable = targetPoint.GetComponent<Damageable>();
                
                if (targetDamageable == null)
                {
                    Debug.LogWarning($"{gameObject.name}: Target {targetPoint.name} has no Damageable component!");
                }
            }
            
            if (targetDamageable != null)
            {
                Attack();
            }
        }
    }

    void HandleOutOfAttackRange(float distanceToTarget)
    {
        // Resume movement if stopped and not staggered
        if (agent.isStopped && !isStaggered)
        {
            agent.isStopped = false;
            hasReachedDestination = false;
            
            // Re-set destination to ensure agent continues moving
            MoveToTarget();
        }

        isAttacking = false;

        if (!agent.pathPending && agent.hasPath)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (agent.velocity.sqrMagnitude < 0.1f && !hasReachedDestination)
                {
                    OnReachedDestination();
                    
                    // If we've reached destination but still out of attack range,
                    // try moving to target again (in case offset is too far)
                    if (distanceToTarget > attackRange)
                    {
                        Debug.Log($"{gameObject.name}: Reached destination but out of attack range. Re-pathing...");
                        MoveToTarget();
                    }
                }
            }
            else
            {
                hasReachedDestination = false;
            }
        }
        else if (!agent.hasPath && !hasReachedDestination)
        {
            // No path and haven't reached destination - try to repath
            Debug.Log($"{gameObject.name}: Lost path, attempting to repath to target...");
            MoveToTarget();
        }
    }

    void UpdateMovementEvents()
    {
        bool isMovingNow = IsMoving();

        if (isMovingNow && !wasMovingLastFrame)
        {
            OnMove.Invoke();
        }
        else if (!isMovingNow && wasMovingLastFrame)
        {
            OnStopMoving.Invoke();
        }

        wasMovingLastFrame = isMovingNow;
    }

    #endregion

    #region Combat

    void Attack()
    {
        if (isDead || targetDamageable == null) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        targetDamageable.TakeDamage(attackDamage);
        Debug.Log($"{gameObject.name} attacked {targetPoint.name} for {attackDamage} damage!");
        
        OnAttack.Invoke();

        if (attackEffectPrefab != null)
        {
            Vector3 effectPos = transform.position + transform.forward * 0.5f;
            Instantiate(attackEffectPrefab, effectPos, Quaternion.identity);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);
        
        OnHit.Invoke();
        Stagger();

        Debug.Log($"{gameObject.name} took {damageAmount} damage. Health: {currentHealth}/{maxHealth}");

        if (damageEffectPrefab != null)
        {
            Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    #endregion

    #region Stagger System

    public void Stagger()
    {
        if (isDead || isStaggered) return;

        isStaggered = true;
        staggerEndTime = Time.time + staggerDuration;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.speed = originalSpeed * staggerSpeedMultiplier;
        }

        OnStagger.Invoke();
        Debug.Log($"{gameObject.name} is staggered for {staggerDuration}s");
    }

    void EndStagger()
    {
        isStaggered = false;

        if (agent != null && agent.enabled && !isDead)
        {
            agent.speed = originalSpeed;
            
            // Only resume if not in attack range
            if (targetPoint != null)
            {
                float distanceToTarget = GetDistanceToTargetEdge();
                if (distanceToTarget > attackRange)
                {
                    agent.isStopped = false;
                }
            }
        }

        Debug.Log($"{gameObject.name} recovered from stagger");
    }

    #endregion

    #region Death

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{gameObject.name} has been killed!");

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        OnDead.Invoke();
        GiveCurrencyReward();
    }

    public void DeathAnimationFinished()
    {
        CleanupAfterDeath();
    }


    void GiveCurrencyReward()
    {
        CurrencyTracker currency = FindAnyObjectByType<CurrencyTracker>();
        if (currency != null)
        {
            int finalReward = CalculateDistanceBasedReward();
            currency.Gain(finalReward);
            Debug.Log($"{gameObject.name} gave {finalReward} currency (base: {currencyAdd}, distance modifier applied)");
        }
    }

    int CalculateDistanceBasedReward()
    {
        if (targetPoint == null)
        {
            return currencyAdd; // No modifier if no target
        }

        float distanceToTarget = GetDistanceToTargetEdge();
        
        // Normalize distance: 0 at stopping distance, 1 at max bonus distance or beyond
        float normalizedDistance = Mathf.Clamp01(
            (distanceToTarget - agent.stoppingDistance) / 
            (maxBonusDistance - agent.stoppingDistance)
        );
        
        // Calculate multiplier: ranges from minMultiplier to 1.0
        float multiplier = Mathf.Lerp(minMultiplier, 1f, normalizedDistance);
        
        // Calculate bonus: ranges from 0 to maxDistanceBonus
        int bonus = Mathf.RoundToInt(maxDistanceBonus * normalizedDistance);
        
        // Final reward = (base currency * multiplier) + bonus
        int baseReward = Mathf.RoundToInt(currencyAdd * multiplier);
        int finalReward = baseReward + bonus;
        
        return Mathf.Max(1, finalReward); // Ensure at least 1 currency
    }

    // Call this after the death animation finishes
    public void CleanupAfterDeath()
    {
        // Disable all colliders
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        // Disable rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        // Disable NavMeshAgent
        if (agent != null)
            agent.enabled = false;

        // Disable scripts on the enemy except this one
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour m in scripts)
        {
            if (m != this) m.enabled = false;
        }
    }


    #endregion

    #region Movement

    public void ApplySpawnSpeedModifier(float multiplier)
    {
        moveSpeed *= multiplier;

        if (agent != null)
            agent.speed = moveSpeed + speedRandomFactor;

        originalSpeed = moveSpeed + speedRandomFactor; // update stagger system
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

        // Apply target offset for spread
        Vector3 targetPos = targetPoint.position + targetOffset;
        MoveToPosition(targetPos);
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

        AdjustStoppingDistanceForTarget();
        MoveToTarget();
    }

    void AdjustStoppingDistanceForTarget()
    {
        if (targetPoint == null) return;

        Collider targetCollider = targetPoint.GetComponent<Collider>();
        if (targetCollider != null)
        {
            Bounds bounds = targetCollider.bounds;
            float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.z);

            float suggestedStoppingDistance = maxExtent + (attackRange * 0.3f);
            agent.stoppingDistance = Mathf.Max(stoppingDistance, suggestedStoppingDistance);

            Debug.Log($"Adjusted stopping distance to {agent.stoppingDistance} based on target size");
        }
    }

    #endregion

    #region Utility Methods

    float GetDistanceToTargetEdge()
    {
        if (targetPoint == null) return float.MaxValue;

        Collider targetCollider = targetPoint.GetComponent<Collider>();
        if (targetCollider != null)
        {
            Vector3 closestPoint = targetCollider.bounds.ClosestPoint(transform.position);
            return Vector3.Distance(transform.position, closestPoint);
        }

        return Vector3.Distance(transform.position, targetPoint.position);
    }

    public bool IsMoving()
    {
        return agent != null && agent.enabled && agent.hasPath && 
               agent.velocity.sqrMagnitude > 0.01f && !hasReachedDestination && 
               !isDead && !isStaggered;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsStaggered()
    {
        return isStaggered;
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    #endregion

    #region Gizmos

    void OnDrawGizmos()
    {
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

        if (showAttackRange)
        {
            Gizmos.color = isAttacking ? Color.red : new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
        
        // Show target offset in editor
        if (targetPoint != null && Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(targetPoint.position + targetOffset, 0.5f);
            Gizmos.DrawLine(targetPoint.position, targetPoint.position + targetOffset);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (showAttackRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
        
        // Show spread radius around target
        if (targetPoint != null)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
            Gizmos.DrawWireSphere(targetPoint.position, targetOffsetRadius);
        }
    }

    #endregion
}