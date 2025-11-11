using UnityEngine;

public class SmoothSnapToTarget : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Initial target to snap to. Will fallback to this when too far from other targets.")]
    public Transform initialTarget;
    
    [Tooltip("Tag of target objects to snap to.")]
    public string targetTag = "SnapTarget";
    
    [Tooltip("Maximum distance from initial target before snapping back to it.")]
    public float maxDistanceFromInitial = 10f;

    [Header("Snapping Settings")]
    [Tooltip("Speed at which this object moves to the target.")]
    public float snapSpeed = 5f;

    [Tooltip("Should rotation also be snapped?")]
    public bool snapRotation = true;

    [Tooltip("Lock movement on the Y axis.")]
    public bool lockY = false;

    [Header("Debug Info")]
    [Tooltip("Current target being snapped to (read-only).")]
    [SerializeField] private Transform currentTarget;
    
    [Tooltip("Distance to current target (read-only).")]
    [SerializeField] private float distanceToTarget;
    
    [Tooltip("Distance to initial target (read-only).")]
    [SerializeField] private float distanceToInitial;

    private bool snappingEnabled = true;

    void Start()
    {
        // Use initial target if no tagged targets are found
        if (initialTarget != null)
        {
            currentTarget = initialTarget;
        }
        
        FindClosestTarget();
    }

    void Update()
    {
        if (!snappingEnabled || currentTarget == null)
            return;

        // Check if we should fallback to initial target
        CheckDistanceFromInitialTarget();

        // Update debug info
        UpdateDebugInfo();

        // Perform snapping
        PerformSnapping();
    }

    private void CheckDistanceFromInitialTarget()
    {
        if (initialTarget == null)
            return;

        float distanceFromInitial = Vector3.Distance(transform.position, initialTarget.position);
        
        // If we're too far from initial target and not already targeting it, switch back
        if (distanceFromInitial > maxDistanceFromInitial && currentTarget != initialTarget)
        {
            currentTarget = initialTarget;
            Debug.Log($"Distance exceeded {maxDistanceFromInitial}. Switching back to initial target.");
        }
    }

    private void PerformSnapping()
    {
        Vector3 targetPos = currentTarget.position;

        // Preserve current Y if locked
        if (lockY)
        {
            targetPos.y = transform.position.y;
        }

        // Smooth position interpolation
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * snapSpeed);

        // Smooth rotation interpolation
        if (snapRotation)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, currentTarget.rotation, Time.deltaTime * snapSpeed);
        }
    }

    private void UpdateDebugInfo()
    {
        if (currentTarget != null)
        {
            distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        }
        
        if (initialTarget != null)
        {
            distanceToInitial = Vector3.Distance(transform.position, initialTarget.position);
        }
    }

    public void EnableSnapping()
    {
        snappingEnabled = true;
        FindClosestTarget();
    }

    public void DisableSnapping()
    {
        snappingEnabled = false;
    }

    public void FindClosestTarget()
    {
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(targetTag);
        
        if (taggedObjects.Length == 0)
        {
            if (initialTarget != null)
            {
                currentTarget = initialTarget;
                Debug.Log("No tagged objects found. Using initial target.");
            }
            else
            {
                Debug.LogWarning($"No GameObjects with tag '{targetTag}' found and no initial target assigned.");
                currentTarget = null;
            }
            return;
        }

        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        foreach (GameObject obj in taggedObjects)
        {
            float distance = Vector3.Distance(transform.position, obj.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = obj.transform;
            }
        }

        if (closestTarget != null)
        {
            currentTarget = closestTarget;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        if (newTarget != null)
        {
            currentTarget = newTarget;
        }
    }

    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }

    // Force switch back to initial target
    public void ResetToInitialTarget()
    {
        if (initialTarget != null)
        {
            currentTarget = initialTarget;
            Debug.Log("Manually reset to initial target.");
        }
        else
        {
            Debug.LogWarning("No initial target assigned to reset to.");
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw line to current target
        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }

        // Draw line to initial target
        if (initialTarget != null && initialTarget != currentTarget)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, initialTarget.position);
        }

        // Draw max distance sphere around initial target
        if (initialTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(initialTarget.position, maxDistanceFromInitial);
        }
    }
}