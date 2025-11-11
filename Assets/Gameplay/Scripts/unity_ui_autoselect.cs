using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UIAutoSelectManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float searchDelay = 0.1f; // Small delay to ensure UI is fully initialized
    [SerializeField] private bool debugMode = false;
    
    [Header("UI Panels to Monitor")]
    [SerializeField] private List<GameObject> uiPanels = new List<GameObject>();
    
    private EventSystem eventSystem;
    private Dictionary<GameObject, bool> previousPanelStates = new Dictionary<GameObject, bool>();
    
    void Start()
    {
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogError("UIAutoSelectManager: No EventSystem found in scene!");
            return;
        }
        
        // Initialize panel states
        foreach (var panel in uiPanels)
        {
            if (panel != null)
            {
                previousPanelStates[panel] = panel.activeInHierarchy;
            }
        }
    }
    
    void Update()
    {
        CheckForUIChanges();
    }
    
    private void CheckForUIChanges()
    {
        foreach (var panel in uiPanels)
        {
            if (panel == null) continue;
            
            bool currentState = panel.activeInHierarchy;
            
            if (previousPanelStates.ContainsKey(panel))
            {
                bool previousState = previousPanelStates[panel];
                
                // Panel was just activated
                if (!previousState && currentState)
                {
                    if (debugMode)
                        Debug.Log($"Panel activated: {panel.name}");
                    
                    StartCoroutine(SelectFirstButtonDelayed(panel));
                }
            }
            
            previousPanelStates[panel] = currentState;
        }
    }
    
    private IEnumerator SelectFirstButtonDelayed(GameObject panel)
    {
        yield return new WaitForSeconds(searchDelay);
        SelectFirstButton(panel);
    }
    
    public void SelectFirstButton(GameObject panel)
    {
        if (panel == null || !panel.activeInHierarchy)
            return;
            
        Button firstButton = FindFirstSelectableButton(panel);
        
        if (firstButton != null)
        {
            if (debugMode)
                Debug.Log($"Selecting first button: {firstButton.name} in panel: {panel.name}");
                
            eventSystem.SetSelectedGameObject(firstButton.gameObject);
        }
        else
        {
            if (debugMode)
                Debug.LogWarning($"No selectable button found in panel: {panel.name}");
        }
    }
    
    private Button FindFirstSelectableButton(GameObject parent)
    {
        // Get all buttons in the panel, including children
        Button[] buttons = parent.GetComponentsInChildren<Button>();
        
        // Filter to only active and interactable buttons
        var selectableButtons = buttons.Where(b => 
            b.gameObject.activeInHierarchy && 
            b.interactable && 
            b.navigation.mode != Navigation.Mode.None
        ).ToArray();
        
        if (selectableButtons.Length == 0)
            return null;
        
        // Sort by hierarchy order (top to bottom, left to right based on transform sibling index)
        var sortedButtons = selectableButtons.OrderBy(b => GetHierarchyOrder(b.transform, parent.transform)).ToArray();
        
        return sortedButtons[0];
    }
    
    private int GetHierarchyOrder(Transform child, Transform root)
    {
        int order = 0;
        Transform current = child;
        
        while (current != root && current.parent != null)
        {
            order += current.GetSiblingIndex() * 1000; // Weight by depth
            current = current.parent;
        }
        
        return order;
    }
    
    // Manual method to force selection on a specific panel
    public void ForceSelectFirstButton(GameObject panel)
    {
        SelectFirstButton(panel);
    }
    
    // Method to add panels at runtime
    public void AddPanelToMonitor(GameObject panel)
    {
        if (panel != null && !uiPanels.Contains(panel))
        {
            uiPanels.Add(panel);
            previousPanelStates[panel] = panel.activeInHierarchy;
            
            if (debugMode)
                Debug.Log($"Added panel to monitor: {panel.name}");
        }
    }
    
    // Method to remove panels from monitoring
    public void RemovePanelFromMonitor(GameObject panel)
    {
        if (uiPanels.Contains(panel))
        {
            uiPanels.Remove(panel);
            previousPanelStates.Remove(panel);
            
            if (debugMode)
                Debug.Log($"Removed panel from monitor: {panel.name}");
        }
    }
    
    // Alternative method for panels that need immediate selection (for runtime-generated UI)
    public void OnPanelActivated(GameObject panel)
    {
        if (debugMode)
            Debug.Log($"Manual panel activation called: {panel.name}");
            
        StartCoroutine(SelectFirstButtonDelayed(panel));
    }
}

// Extension script to add to individual UI panels for more control
[System.Serializable]
public class UIPanelAutoSelect : MonoBehaviour
{
    [Header("Auto-Select Settings")]
    [SerializeField] private bool autoSelectOnEnable = true;
    [SerializeField] private float selectionDelay = 0.1f;
    [SerializeField] private Button specificFirstButton; // Override automatic detection
    
    private UIAutoSelectManager autoSelectManager;
    
    void Start()
    {
        autoSelectManager = FindFirstObjectByType<UIAutoSelectManager>();
    }
    
    void OnEnable()
    {
        if (autoSelectOnEnable)
        {
            StartCoroutine(SelectFirstButtonDelayed());
        }
    }
    
    private IEnumerator SelectFirstButtonDelayed()
    {
        yield return new WaitForSeconds(selectionDelay);
        
        if (specificFirstButton != null)
        {
            EventSystem.current?.SetSelectedGameObject(specificFirstButton.gameObject);
        }
        else if (autoSelectManager != null)
        {
            autoSelectManager.SelectFirstButton(gameObject);
        }
    }
    
    public void ForceSelectFirst()
    {
        StartCoroutine(SelectFirstButtonDelayed());
    }
}