using UnityEngine;

/// <summary>
/// Screen-space aimer that shows where your artillery gun is pointing
/// Always shows aimer at a fixed distance along the barrel direction
/// </summary>
public class ScreenspaceAimer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform aimerUI;
    [SerializeField] private Camera cam;
    [SerializeField] private Transform gunBarrel;
    
    [Header("Settings")]
    [SerializeField] private float aimDistance = 100f;
    [SerializeField] private float smoothTime = 0.1f;
    
    [Header("Screen Bounds")]
    [SerializeField] private bool confineToScreen = true;
    [SerializeField] private Vector2 edgePadding = new Vector2(50f, 50f);
    
    [Header("Raycast (Optional)")]
    [SerializeField] private LayerMask hitLayers = -1;
    [SerializeField] private float maxDistance = 500f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;
    [SerializeField] private Color rayColorHit = Color.green;
    [SerializeField] private Color rayColorMiss = Color.red;
    
    private Vector2 velocity;
    private Canvas canvas;
    
    private void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!aimerUI) aimerUI = GetComponent<RectTransform>();
        if (!gunBarrel) gunBarrel = transform;
        
        canvas = aimerUI.GetComponentInParent<Canvas>();
    }
    
    private void LateUpdate()
    {
        UpdateAimerPosition();
    }
    
    /// <summary>
    /// Positions aimer based on gun barrel direction at fixed distance
    /// </summary>
    private void UpdateAimerPosition()
    {
        // Calculate aim point along barrel direction
        Vector3 aimPoint = gunBarrel.position + gunBarrel.forward * aimDistance;
        
        // Convert to screen space
        Vector3 screenPos = cam.WorldToScreenPoint(aimPoint);
        
        // Handle behind camera
        if (screenPos.z < 0)
        {
            screenPos = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        }
        
        // Confine to screen
        if (confineToScreen)
        {
            screenPos.x = Mathf.Clamp(screenPos.x, edgePadding.x, Screen.width - edgePadding.x);
            screenPos.y = Mathf.Clamp(screenPos.y, edgePadding.y, Screen.height - edgePadding.y);
        }
        
        // Convert to canvas space
        Vector2 canvasPos = GetCanvasPosition(screenPos);
        
        // Smooth movement
        aimerUI.anchoredPosition = Vector2.SmoothDamp(
            aimerUI.anchoredPosition,
            canvasPos,
            ref velocity,
            smoothTime
        );
    }
    
    /// <summary>
    /// Converts screen position to canvas position
    /// </summary>
    private Vector2 GetCanvasPosition(Vector3 screenPos)
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return screenPos;
        }
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            canvas.worldCamera,
            out Vector2 canvasPos
        );
        
        return canvasPos;
    }
    
    /// <summary>
    /// Get the world position where the aimer is pointing
    /// </summary>
    public Vector3 GetAimWorldPosition()
    {
        return gunBarrel.position + gunBarrel.forward * aimDistance;
    }
    
    /// <summary>
    /// Check if gun is aimed at something (performs raycast)
    /// </summary>
    public bool IsAimingAtTarget(out RaycastHit hit)
    {
        Ray ray = new Ray(gunBarrel.position, gunBarrel.forward);
        return Physics.Raycast(ray, out hit, maxDistance, hitLayers);
    }
    
    /// <summary>
    /// Instantly snap to target without smoothing
    /// </summary>
    public void SnapToTarget()
    {
        velocity = Vector2.zero;
        Vector3 aimPoint = GetAimWorldPosition();
        Vector3 screenPos = cam.WorldToScreenPoint(aimPoint);
        aimerUI.anchoredPosition = GetCanvasPosition(screenPos);
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugRay || !gunBarrel) return;
        
        Ray ray = new Ray(gunBarrel.position, gunBarrel.forward);
        Vector3 aimPoint = gunBarrel.position + gunBarrel.forward * aimDistance;
        
        // Check if we hit something
        bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, hitLayers);
        
        Gizmos.color = hit ? rayColorHit : rayColorMiss;
        Gizmos.DrawLine(ray.origin, aimPoint);
        Gizmos.DrawWireSphere(aimPoint, 0.5f);
        
        // Show hit point if we hit something
        if (hit && hitInfo.distance < aimDistance)
        {
            Gizmos.color = rayColorHit;
            Gizmos.DrawWireSphere(hitInfo.point, 0.3f);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal * 2f);
        }
    }
}