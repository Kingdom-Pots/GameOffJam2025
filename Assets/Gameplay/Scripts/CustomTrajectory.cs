using UnityEngine;

namespace ProjectileSystem
{
    public class CustomTrajectory : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private bool alignWithVelocity = true;

        private Rigidbody rb;
        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
            rb = GetComponent<Rigidbody>();
            
            if (rb == null)
            {
                Debug.LogWarning("SimplifiedTrajectoryRotation requires a Rigidbody component!");
                enabled = false;
            }
        }

        private void FixedUpdate()
        {
            if (!alignWithVelocity || rb.linearVelocity.sqrMagnitude < 0.01f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(rb.linearVelocity);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        public void SetRotationSpeed(float speed) => rotationSpeed = speed;
        public void SetAlignmentEnabled(bool enabled) => alignWithVelocity = enabled;
    }
}