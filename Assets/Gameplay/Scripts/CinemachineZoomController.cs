using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;

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
    
    [Header("UI Elements")]
    [Tooltip("Optional UI Scrollbar to display zoom level")]
    [SerializeField] private Scrollbar zoomScrollbar;
    
    [Tooltip("Invert the scrollbar direction (if checked, left/down = more zoom)")]
    [SerializeField] private bool invertScrollbar = false;
    
    [Header("Events")]
    [Tooltip("Event triggered when entering zoom state")]
    public UnityEvent onZoomIn;
    
    [Tooltip("Event triggered when exiting zoom state")]
    public UnityEvent onZoomOut;
    
    [Tooltip("Event with float parameter for FOV changes (normalized 0-1)")]
    public UnityEvent<float> onFOVChanged;
    
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
        
        // Setup scrollbar listener if assigned
        if (zoomScrollbar != null)
        {
            zoomScrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);
            UpdateScrollbarValue();
        }
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
        
        // Invoke zoom in event
        onZoomIn?.Invoke();
    }

    void OnZoomReleased(InputAction.CallbackContext context)
    {
        targetTransform = defaultPosition;
        isZoomedIn = false;
        // Reset FOV when zooming out
        targetFOV = cinemachineCamera.Lens.FieldOfView;
        
        // Invoke zoom out event
        onZoomOut?.Invoke();
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
        float previousFOV = currentFOV;
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * fovSmoothSpeed);
        cinemachineCamera.Lens.FieldOfView = currentFOV;
        
        // Update scrollbar and trigger event if FOV changed
        if (Mathf.Abs(currentFOV - previousFOV) > 0.01f)
        {
            UpdateScrollbarValue();
            
            // Invoke FOV changed event with normalized value (0 = min zoom, 1 = max zoom)
            float normalizedFOV = GetNormalizedFOV();
            onFOVChanged?.Invoke(normalizedFOV);
        }
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
    /// Updates the scrollbar to match current FOV
    /// </summary>
    private void UpdateScrollbarValue()
    {
        if (zoomScrollbar != null && isZoomedIn)
        {
            // Inverse lerp to get normalized value (0-1)
            float normalizedValue = Mathf.InverseLerp(minFOV, maxFOV, currentFOV);
            
            // Invert if needed (scrollbar at 1 = most zoomed in = lowest FOV)
            if (!invertScrollbar)
            {
                normalizedValue = 1f - normalizedValue;
            }
            
            zoomScrollbar.SetValueWithoutNotify(normalizedValue);
        }
    }

    /// <summary>
    /// Called when the scrollbar value is changed by user interaction
    /// </summary>
    private void OnScrollbarValueChanged(float value)
    {
        if (isZoomedIn)
        {
            // Convert scrollbar value (0-1) to FOV range
            float normalizedValue = invertScrollbar ? value : (1f - value);
            targetFOV = Mathf.Lerp(minFOV, maxFOV, normalizedValue);
        }
    }

    /// <summary>
    /// Gets the normalized FOV value (0 = min zoom/max FOV, 1 = max zoom/min FOV)
    /// </summary>
    private float GetNormalizedFOV()
    {
        float normalized = Mathf.InverseLerp(minFOV, maxFOV, currentFOV);
        return 1f - normalized; // Invert so 1 = most zoomed in
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
        
        // Invoke zoom in event
        onZoomIn?.Invoke();
    }

    /// <summary>
    /// Manually exit zoomed state
    /// </summary>
    public void ExitZoomState()
    {
        targetTransform = defaultPosition;
        isZoomedIn = false;
        targetFOV = cinemachineCamera.Lens.FieldOfView;
        
        // Invoke zoom out event
        onZoomOut?.Invoke();
    }
}