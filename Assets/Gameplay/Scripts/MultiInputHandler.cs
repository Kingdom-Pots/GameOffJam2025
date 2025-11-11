using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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

public class MultiInputHandler : MonoBehaviour
{
    [Header("Setup")]
    public List<InputConfig> inputs = new List<InputConfig>();
    
    [Header("Options")]
    public int maxUses = 0; // 0 = unlimited
    
    [Header("Trigger Events")]
    public UnityEvent onPlayerEnter;
    public UnityEvent onPlayerExit;
    
    private bool playerInRange;
    private int useCount;
    
    void OnEnable()
    {
        foreach (var input in inputs)
        {
            if (input.inputAction != null)
                input.inputAction.action.Enable();
        }
    }
    
    void OnDisable()
    {
        foreach (var input in inputs)
        {
            if (input.inputAction != null)
                input.inputAction.action.Disable();
        }
    }
    
    void Update()
    {
        UpdateCooldowns();
        
        if (!playerInRange) return;
        
        foreach (var input in inputs)
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
        foreach (var input in inputs)
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
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            onPlayerEnter?.Invoke();
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            foreach (var input in inputs)
            {
                if (input.isHeld)
                {
                    input.isHeld = false;
                    input.onRelease?.Invoke();
                }
            }
            
            onPlayerExit?.Invoke();
        }
    }
}