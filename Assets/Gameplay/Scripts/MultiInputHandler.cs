using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MultiInputHandler : MonoBehaviour
{
    [System.Serializable]
    public class InputConfig
    {
        public InputActionReference inputAction;
        
        [Header("Cooldown")]
        public float cooldownTime = 0f;
        
        [Header("Events")]
        public UnityEvent onPress;
        public UnityEvent onHold;
        public UnityEvent onRelease;
        public UnityEvent onCooldownEnd;
        
        [HideInInspector] public bool isHeld;
        [HideInInspector] public bool isOnCooldown;
        [HideInInspector] public float cooldownEndTime;
    }

    [System.Serializable]
    public class InputContext
    {
        public string contextName = "Default";
        public List<InputConfig> inputs = new List<InputConfig>();
        
        [Header("Context Events")]
        public UnityEvent onContextEnter;
        public UnityEvent onContextExit;
    }
    
    [Header("Context Setup")]
    public List<InputContext> contexts = new List<InputContext>();
    
    [Header("Options")]
    public int maxUses = 0; // 0 = unlimited
    public bool switchContextOnPlayerEnter = false;
    public string enterContextName = "Default";
    
    [Header("Trigger Events")]
    public UnityEvent onPlayerEnter;
    public UnityEvent onPlayerExit;
    
    private bool playerInRange;
    private int useCount;
    private InputContext activeContext;
    private int activeContextIndex = 0;
    
    void Start()
    {
        // Initialize with first context
        if (contexts.Count > 0)
        {
            SetContext(0, false);
        }
    }
    
    void OnEnable()
    {
        EnableCurrentContext();
    }
    
    void OnDisable()
    {
        DisableAllContexts();
    }
    
    void Update()
    {
        if (activeContext == null) return;
        
        UpdateCooldowns();
        
        if (!playerInRange) return;
        
        foreach (var input in activeContext.inputs)
        {
            if (input.inputAction == null || input.isOnCooldown) continue;
            
            bool pressed = input.inputAction.action.IsPressed();
            
            if (pressed && !input.isHeld && CanUse())
            {
                input.onPress?.Invoke();
                useCount++;
                StartCooldown(input);
            }
            else if (pressed && input.isHeld)
            {
                input.onHold?.Invoke();
            }
            else if (!pressed && input.isHeld)
            {
                input.onRelease?.Invoke();
            }
            
            input.isHeld = pressed;
        }
    }
    
    void UpdateCooldowns()
    {
        if (activeContext == null) return;
        
        foreach (var input in activeContext.inputs)
        {
            if (input.isOnCooldown && Time.time >= input.cooldownEndTime)
            {
                input.isOnCooldown = false;
                input.onCooldownEnd?.Invoke();
            }
        }
    }
    
    void StartCooldown(InputConfig input)
    {
        if (input.cooldownTime > 0)
        {
            input.isOnCooldown = true;
            input.cooldownEndTime = Time.time + input.cooldownTime;
        }
    }
    
    bool CanUse() => maxUses == 0 || useCount < maxUses;
    
    // Context Management Methods
    
    /// <summary>
    /// Switch to a context by name
    /// </summary>
    public void SwitchContext(string contextName)
    {
        int index = contexts.FindIndex(c => c.contextName == contextName);
        if (index >= 0)
        {
            SetContext(index, true);
        }
        else
        {
            Debug.LogWarning($"Context '{contextName}' not found!");
        }
    }
    
    /// <summary>
    /// Switch to a context by index
    /// </summary>
    public void SwitchContextByIndex(int index)
    {
        if (index >= 0 && index < contexts.Count)
        {
            SetContext(index, true);
        }
        else
        {
            Debug.LogWarning($"Context index {index} out of range!");
        }
    }
    
    /// <summary>
    /// Cycle to the next context
    /// </summary>
    public void NextContext()
    {
        if (contexts.Count <= 1) return;
        
        int nextIndex = (activeContextIndex + 1) % contexts.Count;
        SetContext(nextIndex, true);
    }
    
    /// <summary>
    /// Cycle to the previous context
    /// </summary>
    public void PreviousContext()
    {
        if (contexts.Count <= 1) return;
        
        int prevIndex = activeContextIndex - 1;
        if (prevIndex < 0) prevIndex = contexts.Count - 1;
        SetContext(prevIndex, true);
    }
    
    /// <summary>
    /// Get the name of the currently active context
    /// </summary>
    public string GetActiveContextName()
    {
        return activeContext?.contextName ?? "None";
    }
    
    /// <summary>
    /// Reset use count (useful when switching contexts)
    /// </summary>
    public void ResetUseCount()
    {
        useCount = 0;
    }
    
    private void SetContext(int index, bool invokeEvents)
    {
        // Exit current context
        if (activeContext != null && invokeEvents)
        {
            activeContext.onContextExit?.Invoke();
            DisableContext(activeContext);
        }
        
        // Set new context
        activeContextIndex = index;
        activeContext = contexts[index];
        
        // Enable new context
        if (activeContext != null)
        {
            EnableContext(activeContext);
            if (invokeEvents)
            {
                activeContext.onContextEnter?.Invoke();
            }
        }
    }
    
    private void EnableContext(InputContext context)
    {
        foreach (var input in context.inputs)
        {
            if (input.inputAction != null)
                input.inputAction.action.Enable();
        }
    }
    
    private void DisableContext(InputContext context)
    {
        foreach (var input in context.inputs)
        {
            if (input.inputAction != null)
            {
                input.inputAction.action.Disable();
                
                // Release held inputs when context switches
                if (input.isHeld)
                {
                    input.isHeld = false;
                    input.onRelease?.Invoke();
                }
            }
        }
    }
    
    private void EnableCurrentContext()
    {
        if (activeContext != null)
        {
            EnableContext(activeContext);
        }
    }
    
    private void DisableAllContexts()
    {
        foreach (var context in contexts)
        {
            DisableContext(context);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            onPlayerEnter?.Invoke();
            
            // Optionally switch context when player enters
            if (switchContextOnPlayerEnter && !string.IsNullOrEmpty(enterContextName))
            {
                SwitchContext(enterContextName);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            // Release all held inputs
            if (activeContext != null)
            {
                foreach (var input in activeContext.inputs)
                {
                    if (input.isHeld)
                    {
                        input.isHeld = false;
                        input.onRelease?.Invoke();
                    }
                }
            }
            
            onPlayerExit?.Invoke();
        }
    }
}