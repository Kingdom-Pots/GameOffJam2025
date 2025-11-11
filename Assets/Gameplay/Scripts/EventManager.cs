using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TeddyEvent : UnityEvent { }

public class EventManager : MonoBehaviour
{
    public TeddyEvent onTeddyEventTriggered;

    public void TeddyTriggerEvent()
    {
        if (onTeddyEventTriggered != null)
            onTeddyEventTriggered.Invoke();
    }

    public TeddyEvent onKeyEventTriggered;

    public void KeyTriggerEvent()
    {
        if (onKeyEventTriggered != null)
            onKeyEventTriggered.Invoke();
    }    
}
