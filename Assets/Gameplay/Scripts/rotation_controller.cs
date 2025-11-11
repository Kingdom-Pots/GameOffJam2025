using UnityEngine;

public class MechanicalRotator : MonoBehaviour
{
    [Header("Rotator Configuration")]
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y;
    
    public enum RotationAxis
    {
        X, Y, Z
    }
    
    private Vector3 initialRotation;
    private float currentRotation = 0f;
    
    void Start()
    {
        initialRotation = transform.localEulerAngles;
    }
    
    /// <summary>
    /// Add rotation to the rotator
    /// </summary>
    /// <param name="rotationAmount">Amount to rotate in degrees</param>
    public void AddRotation(float rotationAmount)
    {
        currentRotation += rotationAmount;
        ApplyRotation();
    }
    
    /// <summary>
    /// Set absolute rotation for the rotator
    /// </summary>
    /// <param name="rotation">Target rotation in degrees</param>
    public void SetRotation(float rotation)
    {
        currentRotation = rotation;
        ApplyRotation();
    }
    
    /// <summary>
    /// Get current rotation value
    /// </summary>
    public float GetRotation()
    {
        return currentRotation;
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