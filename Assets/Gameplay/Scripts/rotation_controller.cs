using UnityEngine;

public class MechanicalRotator : MonoBehaviour
{
    [Header("Rotator Configuration")]
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y;
    
    [Header("Rotation Constraints")]
    [SerializeField] private bool useConstraints = false;
    [SerializeField] private float minRotation = -180f;
    [SerializeField] private float maxRotation = 180f;
    
    public enum RotationAxis
    {
        X, Y, Z
    }
    
    private Vector3 initialRotation;
    private float currentRotation = 0f;
    
    void Start()
    {
        initialRotation = transform.localEulerAngles;
        
        // Clamp initial rotation if constraints are enabled
        if (useConstraints)
        {
            currentRotation = Mathf.Clamp(currentRotation, minRotation, maxRotation);
        }
    }
    
    /// <summary>
    /// Add rotation to the rotator
    /// </summary>
    /// <param name="rotationAmount">Amount to rotate in degrees</param>
    public void AddRotation(float rotationAmount)
    {
        currentRotation += rotationAmount;
        
        if (useConstraints)
        {
            currentRotation = Mathf.Clamp(currentRotation, minRotation, maxRotation);
        }
        
        ApplyRotation();
    }
    
    /// <summary>
    /// Set absolute rotation for the rotator
    /// </summary>
    /// <param name="rotation">Target rotation in degrees</param>
    public void SetRotation(float rotation)
    {
        currentRotation = rotation;
        
        if (useConstraints)
        {
            currentRotation = Mathf.Clamp(currentRotation, minRotation, maxRotation);
        }
        
        ApplyRotation();
    }
    
    /// <summary>
    /// Get current rotation value
    /// </summary>
    public float GetRotation()
    {
        return currentRotation;
    }
    
    /// <summary>
    /// Check if rotation is at minimum limit
    /// </summary>
    public bool IsAtMinLimit()
    {
        return useConstraints && Mathf.Approximately(currentRotation, minRotation);
    }
    
    /// <summary>
    /// Check if rotation is at maximum limit
    /// </summary>
    public bool IsAtMaxLimit()
    {
        return useConstraints && Mathf.Approximately(currentRotation, maxRotation);
    }
    
    private void ApplyRotation()
    {
        Vector3 newRotation = initialRotation;
        
        switch (rotationAxis)
        {
            case RotationAxis.X:
                newRotation.x = initialRotation.x + currentRotation;
                break;
            case RotationAxis.Y:
                newRotation.y = initialRotation.y + currentRotation;
                break;
            case RotationAxis.Z:
                newRotation.z = initialRotation.z + currentRotation;
                break;
        }
        
        transform.localEulerAngles = newRotation;
    }
}