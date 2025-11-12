using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement: MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Target transform to move towards (optional - will auto-find if empty)")]
    public Transform targetPoint;

    [Tooltip("Tag of object to automatically target (e.g., 'Castle')")]
    public string targetTag = "Castle";

    [Tooltip("Name of object to automatically target (if tag not found)")]
    public string targetName = "";

    [Header("Movement Settings")]
    [Tooltip("Movement speed")]
    public float moveSpeed = 3.5f;

    [Tooltip("Agent acceleration")]
    public float acceleration = 8f;

    [Tooltip("How close to get to the destination")]
    public float stoppingDistance = 0.5f;

    [Tooltip("Start moving to target on Start")]
    public bool moveOnStart = true;

    [Header("Rotation Settings")]
    [Tooltip("Angular speed for rotation")]
    public float angularSpeed = 120f;

    [Header("Debug")]
    public bool showPath = true;
    public Color pathColor = Color.green;

    private NavMeshAgent agent;
    private bool hasReachedDestination = false;

    void Start()
    {
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

        // Auto-find target if not assigned
        if (targetPoint == null)
        {
            FindTarget();
        }

        if (moveOnStart && targetPoint != null)
        {
            MoveToTarget();
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

        Debug.LogWarning($"{gameObject.name}: Could not find target automatically. Check targetTag or targetName.");
    }

    void Update()
    {
        // Check if we've reached destination
        if (agent.enabled && !agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
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
        if (agent == null || !agent.enabled)
        {
            Debug.LogWarning("NavMeshAgent is not available!");
            return;
        }

        NavMeshPath path = new NavMeshPath();
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            hasReachedDestination = false;
            Debug.Log($"{gameObject.name} moving to {position}");
        }
        else
        {
            Debug.LogWarning("Target position is not on NavMesh!");
        }
    }

    public void MoveToTransform(Transform target)
    {
        targetPoint = target;
        MoveToTarget();
    }

    public void StopMoving()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            Debug.Log($"{gameObject.name} stopped moving.");
        }
    }

    public void ResumeMoving()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            Debug.Log($"{gameObject.name} resumed moving.");
        }
    }

    void OnReachedDestination()
    {
        hasReachedDestination = true;
        Debug.Log($"{gameObject.name} reached destination!");

        // You can add custom behavior here when destination is reached
        // For example: play animation, trigger event, etc.
    }

    public bool IsMoving()
    {
        return agent != null && agent.enabled && agent.hasPath && !hasReachedDestination;
    }

    public float GetRemainingDistance()
    {
        if (agent != null && agent.enabled && agent.hasPath)
        {
            return agent.remainingDistance;
        }
        return 0f;
    }

    // Visualize path in scene view
    void OnDrawGizmos()
    {
        if (!showPath || agent == null || !agent.hasPath) return;

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
}

// Optional: Click-to-move controller
public class ClickToMove : MonoBehaviour
{
    [Header("References")]
    public EnemyMovement mover;

    [Header("Settings")]
    [Tooltip("Which mouse button to use (0 = left, 1 = right, 2 = middle)")]
    public int mouseButton = 0;

    [Tooltip("Layer mask for ground/walkable surfaces")]
    public LayerMask groundLayer = -1;

    [Header("Visual Feedback")]
    public GameObject clickMarkerPrefab;
    public float markerLifetime = 1f;

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;

        if (mover == null)
        {
            mover = GetComponent<EnemyMovement>();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(mouseButton))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                mover.MoveToPosition(hit.point);

                // Spawn visual marker
                if (clickMarkerPrefab != null)
                {
                    GameObject marker = Instantiate(clickMarkerPrefab, hit.point, Quaternion.identity);
                    Destroy(marker, markerLifetime);
                }
            }
        }
    }
}

// Optional: Patrol between multiple points
public class NavMeshPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;

    [Tooltip("Wait time at each point")]
    public float waitTime = 2f;

    [Tooltip("Loop back to first point")]
    public bool loop = true;

    [Tooltip("Move in random order")]
    public bool randomOrder = false;

    private EnemyMovement mover;
    private int currentPointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    void Start()
    {
        mover = GetComponent<EnemyMovement>();
        if (mover == null)
        {
            Debug.LogError("NavMeshMover component required!");
            enabled = false;
            return;
        }

        if (patrolPoints.Length > 0)
        {
            GoToNextPoint();
        }
    }

    void Update()
    {
        if (patrolPoints.Length == 0) return;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                GoToNextPoint();
            }
        }
        else if (!mover.IsMoving())
        {
            // Reached patrol point, start waiting
            isWaiting = true;
            waitTimer = waitTime;
        }
    }

    void GoToNextPoint()
    {
        if (randomOrder)
        {
            currentPointIndex = Random.Range(0, patrolPoints.Length);
        }
        else
        {
            currentPointIndex++;

            if (currentPointIndex >= patrolPoints.Length)
            {
                if (loop)
                {
                    currentPointIndex = 0;
                }
                else
                {
                    enabled = false;
                    return;
                }
            }
        }

        if (patrolPoints[currentPointIndex] != null)
        {
            mover.MoveToTransform(patrolPoints[currentPointIndex]);
        }
    }
}