using System.Collections;
using UnityEngine;

public class MouthController : MonoBehaviour
{
    [System.Serializable]
    public class MouthBone
    {
        public Transform boneTransform;
        public Vector3 closedRotation;
        public Vector3 openRotation;
    }

    [Header("Configuration")]
    [SerializeField] private MouthBone[] mouthBones;
    [SerializeField] private float lerpSpeed = 2f;

    [Header("State")]
    [SerializeField] private bool isOpen = false;
    private Coroutine currentAnimation;

    public void ToggleMouth()
    {
        if (isOpen) CloseMouth();
        else OpenMouth();
    }

    public void OpenMouth()
    {
        if (isOpen) return;
        StartAnimation(true);
    }

    public void CloseMouth()
    {
        if (!isOpen) return;
        StartAnimation(false);
    }

    private void StartAnimation(bool openState)
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(AnimateMouth(openState));
    }

    private IEnumerator AnimateMouth(bool targetOpenState)
    {
        float progress = 0f;
        Quaternion[][] startRotations = new Quaternion[mouthBones.Length][];
        
        // Store initial rotations
        for (int i = 0; i < mouthBones.Length; i++)
        {
            startRotations[i] = new Quaternion[] 
            {
                mouthBones[i].boneTransform.localRotation,
                Quaternion.Euler(targetOpenState ? 
                    mouthBones[i].openRotation : 
                    mouthBones[i].closedRotation)
            };
        }

        while (progress < 1f)
        {
            progress += Time.deltaTime * lerpSpeed;
            for (int i = 0; i < mouthBones.Length; i++)
            {
                mouthBones[i].boneTransform.localRotation = Quaternion.Lerp(
                    startRotations[i][0],
                    startRotations[i][1],
                    progress
                );
            }
            yield return null;
        }

        // Ensure final rotation is exact
        for (int i = 0; i < mouthBones.Length; i++)
        {
            mouthBones[i].boneTransform.localRotation = startRotations[i][1];
        }

        isOpen = targetOpenState;
        currentAnimation = null;
    }
}