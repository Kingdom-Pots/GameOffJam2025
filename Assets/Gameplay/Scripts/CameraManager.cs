using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private List<CinemachineCamera> cameras;
    [SerializeField] private int defaultCameraIndex = 0;
    [SerializeField] private int activePriority = 10;
    [SerializeField] private int inactivePriority = 0;
    
    private CinemachineCamera activeCamera;
    private int previousCameraIndex = -1;
    
    private void Start()
    {
        // Set all cameras to low priority
        foreach (var cam in cameras)
        {
            cam.Priority = inactivePriority;
        }
        
        // Activate first camera
        if (cameras.Count > 0)
        {
            SwitchToCamera(defaultCameraIndex);
        }
    }
    
    public void SwitchToCamera(int index)
    {
        if (index < 0 || index >= cameras.Count) return;
        
        // Store previous camera index
        if (activeCamera != null)
        {
            previousCameraIndex = cameras.IndexOf(activeCamera);
            activeCamera.Priority = inactivePriority;
        }
        
        // Activate new camera
        activeCamera = cameras[index];
        activeCamera.Priority = activePriority;
    }
    
    public void SwitchToCamera(CinemachineCamera camera)
    {
        int index = cameras.IndexOf(camera);
        if (index >= 0)
        {
            SwitchToCamera(index);
        }
    }
    
    public void NextCamera()
    {
        int current = cameras.IndexOf(activeCamera);
        int next = (current + 1) % cameras.Count;
        SwitchToCamera(next);
    }
    
    public void PreviousCamera()
    {
        int current = cameras.IndexOf(activeCamera);
        int prev = (current - 1 + cameras.Count) % cameras.Count;
        SwitchToCamera(prev);
    }
    
    public void RevertToDefault()
    {
        SwitchToCamera(defaultCameraIndex);
    }
    
    public void ToggleCamera(int targetIndex)
    {
        int currentIndex = cameras.IndexOf(activeCamera);
        
        // If we're on the target, switch to previous (or default if no previous)
        if (currentIndex == targetIndex)
        {
            if (previousCameraIndex >= 0 && previousCameraIndex != targetIndex)
            {
                SwitchToCamera(previousCameraIndex);
            }
            else
            {
                SwitchToCamera(defaultCameraIndex);
            }
        }
        else
        {
            // Switch to target
            SwitchToCamera(targetIndex);
        }
    }
}