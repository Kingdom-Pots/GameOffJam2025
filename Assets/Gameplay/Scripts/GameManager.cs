using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 60; // Caps FPS to 60
    }

    public void CursorUnlock()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void CursorLock()
    {
        Cursor.visible = false;
    }


}
