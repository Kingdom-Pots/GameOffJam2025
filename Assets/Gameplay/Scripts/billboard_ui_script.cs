using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    [SerializeField] private bool lockX, lockY, lockZ;
    private Transform cam;
    
    void Awake()
    {
        Camera activeCamera = Camera.main ?? FindAnyObjectByType<Camera>();
        if (activeCamera != null)
            cam = activeCamera.transform;
    }
    
    void LateUpdate()
    {
        if (cam != null)
        {
            Vector3 targetDirection = cam.forward;
            Vector3 currentEuler = transform.eulerAngles;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Vector3 targetEuler = targetRotation.eulerAngles;
            
            if (lockX) targetEuler.x = currentEuler.x;
            if (lockY) targetEuler.y = currentEuler.y;
            if (lockZ) targetEuler.z = currentEuler.z;
            
            transform.rotation = Quaternion.Euler(targetEuler);
        }
    }
}