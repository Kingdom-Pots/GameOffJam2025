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
    
    [Tooltip("Input action for mousewheel scroll")]
    [SerializeField] private InputActionReference scrollInputAction;
    
    [Tooltip("How fast the camera moves between positions")]
    [SerializeField] private float transitionSpeed = 5f;
    
    [Header("FOV Zoom Settings")]
    [Tooltip("Enable mousewheel FOV zoom when zoomed in")]
    [SerializeField] private bool enableFOVZoom = true;
    
    [Tooltip("Minimum FOV value")]
    [SerializeField] private float minFOV = 20f;
    
    [Tooltip("Maximum FOV value")]
    [SerializeField] private float maxFOV = 60f;
    
    [Tooltip("Default FOV when entering zoomed state")]
    [SerializeField] private float defaultZoomedFOV = 40f;
    
    [Tooltip("How sensitive the mousewheel zoom is")]
    [SerializeField] private float zoomSensitivity = 5f;
    
    [Tooltip("How fast FOV changes")]
    [SerializeField] private float fovSmoothSpeed = 10f;
    
    private Transform targetTransform;
    private Transform cameraTransform;
    private bool isZoomedIn = false;
    private float currentFOV;
    private float targetFOV;
    private CinemachineOrbitalFollow orbitalFollow;

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
        
        // Get the orbital follow component if it exists
        orbitalFollow = cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();
        
        // Initialize FOV
        currentFOV = cinemachineCamera.Lens.FieldOfView;
        targetFOV = currentFOV;
    }

    void OnEnable()
    {
        if (zoomInputAction != null)
        {
            zoomInputAction.action.performed += OnZoomPressed;
            zoomInputAction.action.canceled += OnZoomReleased;
            zoomInputAction.action.Enable();
        }
        
        if (scrollInputAction != null)
        {
            scrollInputAction.action.Enable();
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
        
        if (scrollInputAction != null)
        {
            scrollInputAction.action.Disable();
        }
    }

    void OnZoomPressed(InputAction.CallbackContext context)
    {
        targetTransform = zoomedPosition;
        isZoomedIn = true;
        targetFOV = defaultZoomedFOV;
    }

    void OnZoomReleased(InputAction.CallbackContext context)
    {
        targetTransform = defaultPosition;
        isZoomedIn = false;
        // Reset FOV when zooming out
        targetFOV = cinemachineCamera.Lens.FieldOfView;
    }

    void Update()
    {
        // Handle mousewheel FOV zoom only when zoomed in
        if (enableFOVZoom && isZoomedIn && scrollInputAction != null)
        {
            float scrollInput = scrollInputAction.action.ReadValue<Vector2>().y;
            
            if (scrollInput != 0)
            {
                targetFOV -= scrollInput * zoomSensitivity;
                targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
            }
        }
        
        // Smoothly interpolate FOV
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * fovSmoothSpeed);
        cinemachineCamera.Lens.FieldOfView = currentFOV;
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

    /// <summary>
    /// Increases the FOV zoom by the specified amount (decreases FOV value)
    /// </summary>
    /// <param name="amount">Amount to zoom in (positive values zoom in, negative zoom out)</param>
    public void IncreaseZoom(float amount)
    {
        if (isZoomedIn)
        {
            targetFOV -= amount;
            targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
        }
    }

    /// <summary>
    /// Sets the FOV to a specific value
    /// </summary>
    /// <param name="fov">The target FOV value</param>
    public void SetFOV(float fov)
    {
        if (isZoomedIn)
        {
            targetFOV = Mathf.Clamp(fov, minFOV, maxFOV);
        }
    }

    /// <summary>
    /// Returns the current FOV value
    /// </summary>
    public float GetCurrentFOV()
    {
        return currentFOV;
    }

    /// <summary>
    /// Returns whether the camera is currently in zoomed state
    /// </summary>
    public bool IsZoomedIn()
    {
        return isZoomedIn;
    }

    /// <summary>
    /// Manually enter zoomed state
    /// </summary>
    public void EnterZoomState()
    {
        targetTransform = zoomedPosition;
        isZoomedIn = true;
        targetFOV = defaultZoomedFOV;
    }

    /// <summary>
    /// Manually exit zoomed state
    /// </summary>
    public void ExitZoomState()
    {
        targetTransform = defaultPosition;
        isZoomedIn = false;
        targetFOV = cinemachineCamera.Lens.FieldOfView;
    }
}