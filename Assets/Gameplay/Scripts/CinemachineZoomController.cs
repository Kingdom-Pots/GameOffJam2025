using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;

public class CinemachineZoomController : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Camera References")]
    [Tooltip("The CinemachineCamera to control")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    
    [Tooltip("The UI Camera to sync FOV with")]
    [SerializeField] private Camera uiCamera;
    
    [Header("Camera Positions")]
    [Tooltip("Transform for the default (zoomed out) camera position")]
    [SerializeField] private Transform defaultPosition;
    
    [Tooltip("Transform for the zoomed in camera position")]
    [SerializeField] private Transform zoomedPosition;
    
    [Header("Input Actions")]
    [Tooltip("Input action for zoom (press to zoom in, release to zoom out)")]
    [SerializeField] private InputActionReference zoomInputAction;
    
    [Tooltip("Input action for mousewheel scroll")]
    [SerializeField] private InputActionReference scrollInputAction;
    
    [Header("Transition Settings")]
    [Tooltip("How fast the camera moves between positions")]
    [SerializeField] private float transitionSpeed = 5f;
    
    [Tooltip("How fast FOV changes")]
    [SerializeField] private float fovSmoothSpeed = 10f;
    
    [Header("FOV Settings")]
    [Tooltip("Enable mousewheel FOV zoom when zoomed in")]
    [SerializeField] private bool enableFOVZoom = true;
    
    [Tooltip("Default FOV when not zoomed")]
    [SerializeField] private float defaultFOV = 60f;
    
    [Tooltip("Default FOV when entering zoomed state")]
    [SerializeField] private float defaultZoomedFOV = 40f;
    
    [Tooltip("Minimum FOV value (maximum zoom)")]
    [SerializeField] private float minFOV = 20f;
    
    [Tooltip("Maximum FOV value (minimum zoom)")]
    [SerializeField] private float maxFOV = 60f;
    
    [Tooltip("How sensitive the mousewheel zoom is")]
    [SerializeField] private float zoomSensitivity = 5f;
    
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
    
    #endregion
    
    #region Private Fields
    
    private Transform cameraTransform;
    private Transform targetTransform;
    private bool isZoomedIn = false;
    private float currentFOV;
    private float targetFOV;
    private CinemachineOrbitalFollow orbitalFollow;
    
    #endregion
    
    #region Unity Lifecycle
    
    void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }
        
        InitializeComponents();
        InitializeFOV();
        SetupScrollbar();
    }

    void OnEnable()
    {
        RegisterInputActions();
    }

    void OnDisable()
    {
        UnregisterInputActions();
    }

    void Update()
    {
        HandleMousewheelZoom();
        UpdateFOV();
    }

    void LateUpdate()
    {
        UpdateCameraTransform();
    }
    
    #endregion
    
    #region Initialization
    
    private bool ValidateReferences()
    {
        if (cinemachineCamera == null)
        {
            Debug.LogError("CinemachineCamera not assigned!", this);
            return false;
        }
        
        if (defaultPosition == null || zoomedPosition == null)
        {
            Debug.LogError("Default Position and Zoomed Position must be assigned!", this);
            return false;
        }
        
        return true;
    }
    
    private void InitializeComponents()
    {
        cameraTransform = cinemachineCamera.transform;
        targetTransform = defaultPosition;
        orbitalFollow = cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();
    }
    
    private void InitializeFOV()
    {
        // Ensure maxFOV is set to defaultFOV
        maxFOV = defaultFOV;
        
        // Initialize both cameras to default FOV
        currentFOV = defaultFOV;
        targetFOV = defaultFOV;
        cinemachineCamera.Lens.FieldOfView = defaultFOV;
        
        if (uiCamera != null)
        {
            uiCamera.fieldOfView = defaultFOV;
        }
    }
    
    private void SetupScrollbar()
    {
        if (zoomScrollbar != null)
        {
            zoomScrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);
            UpdateScrollbarValue();
        }
    }
    
    #endregion
    
    #region Input Handling
    
    private void RegisterInputActions()
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
    
    private void UnregisterInputActions()
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
    
    private void OnZoomPressed(InputAction.CallbackContext context)
    {
        EnterZoomState();
    }

    private void OnZoomReleased(InputAction.CallbackContext context)
    {
        ExitZoomState();
    }
    
    #endregion
    
    #region Update Methods
    
    private void HandleMousewheelZoom()
    {
        if (!enableFOVZoom || !isZoomedIn || scrollInputAction == null)
            return;
        
        float scrollInput = scrollInputAction.action.ReadValue<Vector2>().y;
        
        if (scrollInput != 0)
        {
            targetFOV -= scrollInput * zoomSensitivity;
            targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
        }
    }
    
    private void UpdateFOV()
    {
        float previousFOV = currentFOV;
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * fovSmoothSpeed);
        
        // Sync both cameras
        cinemachineCamera.Lens.FieldOfView = currentFOV;
        if (uiCamera != null)
        {
            uiCamera.fieldOfView = currentFOV;
        }
        
        // Update UI and trigger events if FOV changed significantly
        if (Mathf.Abs(currentFOV - previousFOV) > 0.01f)
        {
            UpdateScrollbarValue();
            
            float normalizedFOV = GetNormalizedFOV();
            onFOVChanged?.Invoke(normalizedFOV);
        }
    }
    
    private void UpdateCameraTransform()
    {
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
    
    #endregion
    
    #region Scrollbar Handling
    
    private void UpdateScrollbarValue()
    {
        if (zoomScrollbar == null || !isZoomedIn)
            return;
        
        float normalizedValue = Mathf.InverseLerp(minFOV, maxFOV, currentFOV);
        
        // Invert if needed (scrollbar at 1 = most zoomed in = lowest FOV)
        if (!invertScrollbar)
        {
            normalizedValue = 1f - normalizedValue;
        }
        
        zoomScrollbar.SetValueWithoutNotify(normalizedValue);
    }
    
    private void OnScrollbarValueChanged(float value)
    {
        if (!isZoomedIn)
            return;
        
        float normalizedValue = invertScrollbar ? value : (1f - value);
        targetFOV = Mathf.Lerp(minFOV, maxFOV, normalizedValue);
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Manually enter zoomed state
    /// </summary>
    public void EnterZoomState()
    {
        targetTransform = zoomedPosition;
        isZoomedIn = true;
        targetFOV = defaultZoomedFOV;
        
        onZoomIn?.Invoke();
    }

    /// <summary>
    /// Manually exit zoomed state and reset FOV to default
    /// </summary>
    public void ExitZoomState()
    {
        targetTransform = defaultPosition;
        isZoomedIn = false;
        
        // Reset FOV to default
        targetFOV = defaultFOV;
        currentFOV = defaultFOV;
        
        // Immediately set both cameras to default FOV
        cinemachineCamera.Lens.FieldOfView = defaultFOV;
        if (uiCamera != null)
        {
            uiCamera.fieldOfView = defaultFOV;
        }
        
        onZoomOut?.Invoke();
    }

    /// <summary>
    /// Adjusts the minimum FOV limit (increases maximum zoom capability)
    /// </summary>
    /// <param name="amount">Amount to decrease minFOV by (positive values allow more zoom)</param>
    public void IncreaseZoom(float amount)
    {
        minFOV -= amount;
        minFOV = Mathf.Max(minFOV, 5f); // Prevent going below 5 degrees
        
        // Clamp current target if needed
        if (targetFOV < minFOV)
        {
            targetFOV = minFOV;
        }
    }

    /// <summary>
    /// Sets the FOV to a specific value (only works when zoomed in)
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
    /// Gets the normalized FOV value (0 = min zoom/max FOV, 1 = max zoom/min FOV)
    /// </summary>
    public float GetNormalizedFOV()
    {
        float normalized = Mathf.InverseLerp(minFOV, maxFOV, currentFOV);
        return 1f - normalized; // Invert so 1 = most zoomed in
    }
    
    #endregion
}