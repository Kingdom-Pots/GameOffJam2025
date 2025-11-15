using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class CinemachineZoomController : MonoBehaviour
{
    [Header("Camera Positions")]
    [Tooltip("Transform for the default (zoomed out) camera position")]
    [SerializeField] private Transform defaultPosition;
    
    [Tooltip("Transform for the zoomed in camera position")]
    [SerializeField] private Transform zoomedPosition;
    
    [Header("Settings")]
    [Tooltip("The CinemachineCamera to move")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    
    [Tooltip("Input action for zoom (press to zoom in, release to zoom out)")]
    [SerializeField] private InputActionReference zoomInputAction;
    
    [Tooltip("How fast the camera moves between positions")]
    [SerializeField] private float transitionSpeed = 5f;
    
    private Transform targetTransform;
    private Transform cameraTransform;

    void Awake()
    {
        if (cinemachineCamera == null)
        {
            Debug.LogError("CinemachineCamera not assigned!", this);
            enabled = false;
            return;
        }
        
        if (defaultPosition == null || zoomedPosition == null)
        {
            Debug.LogError("Default Position and Zoomed Position must be assigned!", this);
            enabled = false;
            return;
        }
        
        cameraTransform = cinemachineCamera.transform;
        targetTransform = defaultPosition;
    }

    void OnEnable()
    {
        if (zoomInputAction != null)
        {
            zoomInputAction.action.performed += OnZoomPressed;
            zoomInputAction.action.canceled += OnZoomReleased;
            zoomInputAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (zoomInputAction != null)
        {
            zoomInputAction.action.performed -= OnZoomPressed;
            zoomInputAction.action.canceled -= OnZoomReleased;
            zoomInputAction.action.Disable();
        }
    }

    void OnZoomPressed(InputAction.CallbackContext context)
    {
        targetTransform = zoomedPosition;
    }

    void OnZoomReleased(InputAction.CallbackContext context)
    {
        targetTransform = defaultPosition;
    }

    void LateUpdate()
    {
        // Smoothly move camera to target position and rotation
        cameraTransform.position = Vector3.Lerp(
            cameraTransform.position, 
            targetTransform.position, 
            Time.deltaTime * transitionSpeed
        );
        
        cameraTransform.rotation = Quaternion.Slerp(
            cameraTransform.rotation, 
            targetTransform.rotation, 
            Time.deltaTime * transitionSpeed
        );
    }
}