using MoreMountains.Feedbacks;
using UnityEngine;

public class TaxCollectorHelper : MonoBehaviour
{
    GameManager gm = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gm = FindFirstObjectByType<GameManager>();
    }

    public void CursorLock()
    {
        if (gm)
        {
            gm.CursorLock();
        }
    }

    public void CursorUnlock()
    {
        if (gm)
        {
            gm.CursorUnlock();
        }
    }
}
