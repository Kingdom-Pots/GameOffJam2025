using UnityEngine;
using UnityEngine.Events;

public class ToggleHelper : MonoBehaviour
{
    [Header("Toggle Events")]
    [SerializeField] private UnityEvent defaultEvent;
    [SerializeField] private UnityEvent secondaryEvent;

    private bool isDefault = true;

    public void Toggle()
    {
        if (isDefault)
        {
            defaultEvent?.Invoke();
        }
        else
        {
            secondaryEvent?.Invoke();
        }

        isDefault = !isDefault;
    }

    public void ResetToDefault()
    {
        isDefault = true;
    }
}
