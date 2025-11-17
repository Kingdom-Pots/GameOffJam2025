using UnityEngine;
using Unity.Cinemachine;

public class CinemachineRotationConfiner : CinemachineExtension
{
    [Header("Pitch (X)")]
    public float minPitch = -60f;
    public float maxPitch = 60f;

    [Header("Yaw (Y)")]
    public float minYaw = -90f;
    public float maxYaw = 90f;

    [Header("Settings")]
    public bool constrainPitch = true;
    public bool constrainYaw = true;
    public Transform yawReference;


    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Aim) 
            return;

        var e = state.RawOrientation.eulerAngles;

        float pitch = Normalize(e.x);
        float yaw   = Normalize(e.y);
        float roll  = e.z;
        bool changed = false;

        // ----- Pitch -----
        if (constrainPitch)
            changed |= Clamp(ref pitch, minPitch, maxPitch);

        // ----- Yaw -----
        if (constrainYaw)
        {
            if (yawReference)
            {
                float refYaw = Normalize(yawReference.eulerAngles.y);
                float rel = Normalize(yaw - refYaw);

                if (Clamp(ref rel, minYaw, maxYaw))
                {
                    yaw = refYaw + rel;
                    changed = true;
                }
            }
            else
            {
                changed |= Clamp(ref yaw, minYaw, maxYaw);
            }
        }

        if (changed)
            state.RawOrientation = Quaternion.Euler(pitch, yaw, roll);
    }


    // Normalize to [-180..180]
    static float Normalize(float a)
    {
        a %= 360f;
        return (a > 180f) ? a - 360f : (a < -180f) ? a + 360f : a;
    }

    // Clamp + return true if modified
    static bool Clamp(ref float v, float min, float max)
    {
        float o = v;
        v = Mathf.Clamp(v, min, max);
        return o != v;
    }
}
