using UnityEngine;

public class AnimatorBoolHelper : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    [Header("Smooth Follow Settings")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true;
    [SerializeField] private bool followZ = true;
    
    private bool isFollowing = false;
    
    private void Awake()
    {
        // Auto-assign animator if not set
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }
    
    private void Update()
    {
        // Execute smooth follow if enabled and target is assigned
        if (isFollowing && followTarget != null)
        {
            Vector3 currentPos = transform.position;
            Vector3 targetPos = followTarget.position;
            
            // Create desired position based on which axes to follow
            Vector3 desiredPos = new Vector3(
                followX ? targetPos.x : currentPos.x,
                followY ? targetPos.y : currentPos.y,
                followZ ? targetPos.z : currentPos.z
            );
            
            // Smoothly interpolate to the desired position
            transform.position = Vector3.Lerp(currentPos, desiredPos, smoothSpeed * Time.deltaTime);
        }
    }
    
    // Start following the assigned target
    public void StartFollowing()
    {
        if (followTarget == null)
        {
            Debug.LogWarning("Follow target is not assigned!");
            return;
        }
        isFollowing = true;
    }
    
    // Stop following
    public void StopFollowing()
    {
        isFollowing = false;
    }
    
    // Toggle following on/off
    public void ToggleFollowing()
    {
        if (followTarget == null && !isFollowing)
        {
            Debug.LogWarning("Follow target is not assigned!");
            return;
        }
        isFollowing = !isFollowing;
    }
    
    // Set a new target and optionally start following
    public void SetFollowTarget(Transform newTarget)
    {
        followTarget = newTarget;
    }
    
    // Toggle a bool parameter (flip its current value)
    public void ToggleBool(string parameterName)
    {
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned!");
            return;
        }
        
        bool currentValue = animator.GetBool(parameterName);
        animator.SetBool(parameterName, !currentValue);
    }
    
    // Set a bool parameter to true
    public void SetBoolTrue(string parameterName)
    {
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned!");
            return;
        }
        
        animator.SetBool(parameterName, true);
    }
    
    // Set a bool parameter to false
    public void SetBoolFalse(string parameterName)
    {
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned!");
            return;
        }
        
        animator.SetBool(parameterName, false);
    }
    
    // Set a bool parameter to a specific value
    public void SetBool(string parameterName, bool value)
    {
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned!");
            return;
        }
        
        animator.SetBool(parameterName, value);
    }
    
    // Reset all bool parameters to false (optional utility)
    public void ResetAllBools(string[] parameterNames)
    {
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned!");
            return;
        }
        
        foreach (string paramName in parameterNames)
        {
            animator.SetBool(paramName, false);
        }
    }

    // Move this object to a designated transform position
    public void MoveToTransform(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            Debug.LogError("Target transform is not assigned!");
            return;
        }
        
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
    }
    
    // Move this object to a designated transform position only (keep current rotation)
    public void MoveToPosition(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            Debug.LogError("Target transform is not assigned!");
            return;
        }
        
        transform.position = targetTransform.position;
    }
}