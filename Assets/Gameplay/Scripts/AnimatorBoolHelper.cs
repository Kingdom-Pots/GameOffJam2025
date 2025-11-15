using UnityEngine;

public class AnimatorBoolHelper : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    private void Awake()
    {
        // Auto-assign animator if not set
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
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