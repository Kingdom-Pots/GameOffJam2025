using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Blobcreate.ProjectileToolkit.Demo
{
    public class ArtilleryManager : MonoBehaviour
    {
        #region Inspector Fields
        
        [Header("Launch Settings")]
        [SerializeField] private Rigidbody shell;
        [SerializeField] private Transform launchPoint;
        [SerializeField] private float launchSpeed = 20f;

        [Header("Rotation Settings")]
        [SerializeField] private Transform horizontalRotator;
        [SerializeField] private Transform verticalRotator;
        [SerializeField] private float rotationSpeed = 50f;

        [Header("Horizontal Constraints")]
        [SerializeField] private bool constrainHorizontal = false;
        [SerializeField] private float horizontalMin = -180f;
        [SerializeField] private float horizontalMax = 180f;

        [Header("Vertical Constraints")]
        [SerializeField] private bool constrainVertical = true;
        [SerializeField] private float verticalMin = -10f;
        [SerializeField] private float verticalMax = 45f;

        [Header("Input")]
        [SerializeField] private InputActionReference rotateAction;
        [SerializeField] private InputActionReference fireAction;

        [Header("Events")]
        public UnityEvent OnMove;
        public UnityEvent OnStopMoving;
        
        #endregion

        #region Private Fields
        
        private Vector3 horizontalInitialRotation;
        private Vector3 verticalInitialRotation;
        private float horizontalRotation;
        private float verticalRotation;
        private bool isMoving;
        
        private bool inputEnabled;
        private float additionalDamage;
        
        #endregion

        #region Unity Lifecycle
        
        private void OnDisable()
        {
            DisableInput();
        }

        private void Start()
        {
            CacheInitialRotations();
        }

        private void Update()
        {
            if (inputEnabled)
            {
                ProcessRotationInput();
            }
        }
        
        #endregion

        #region Input Management
        
        /// <summary>
        /// Enable input controls for rotation and firing
        /// </summary>
        public void EnableInput()
        {
            if (inputEnabled) return;

            if (rotateAction != null)
            {
                rotateAction.action.Enable();
            }
            
            if (fireAction != null)
            {
                fireAction.action.Enable();
                fireAction.action.performed += OnFireInput;
            }

            inputEnabled = true;
        }

        /// <summary>
        /// Disable input controls for rotation and firing
        /// </summary>
        public void DisableInput()
        {
            if (!inputEnabled) return;

            if (rotateAction != null)
            {
                rotateAction.action.Disable();
            }
            
            if (fireAction != null)
            {
                fireAction.action.performed -= OnFireInput;
                fireAction.action.Disable();
            }

            inputEnabled = false;
        }

        private void OnFireInput(InputAction.CallbackContext context)
        {
            Launch();
        }
        
        #endregion

        #region Rotation System
        
        private void CacheInitialRotations()
        {
            if (horizontalRotator != null)
            {
                horizontalInitialRotation = horizontalRotator.localEulerAngles;
            }
            
            if (verticalRotator != null)
            {
                verticalInitialRotation = verticalRotator.localEulerAngles;
            }
        }

        private void ProcessRotationInput()
        {
            if (rotateAction == null) return;

            Vector2 input = rotateAction.action.ReadValue<Vector2>();
            bool hasInput = input.sqrMagnitude > 0.01f;

            if (hasInput)
            {
                ApplyRotation(input);
                HandleMovementEvents(true);
            }
            else
            {
                HandleMovementEvents(false);
            }
        }

        private void ApplyRotation(Vector2 input)
        {
            float delta = rotationSpeed * Time.deltaTime;

            // Rotate horizontal axis
            if (horizontalRotator != null && Mathf.Abs(input.x) > 0.01f)
            {
                horizontalRotation += input.x * delta;
                
                if (constrainHorizontal)
                {
                    horizontalRotation = Mathf.Clamp(horizontalRotation, horizontalMin, horizontalMax);
                }
                
                UpdateHorizontalRotation();
            }

            // Rotate vertical axis
            if (verticalRotator != null && Mathf.Abs(input.y) > 0.01f)
            {
                verticalRotation += input.y * delta;
                
                if (constrainVertical)
                {
                    verticalRotation = Mathf.Clamp(verticalRotation, verticalMin, verticalMax);
                }
                
                UpdateVerticalRotation();
            }
        }

        private void UpdateHorizontalRotation()
        {
            horizontalRotator.localEulerAngles = new Vector3(
                horizontalInitialRotation.x,
                horizontalInitialRotation.y + horizontalRotation,
                horizontalInitialRotation.z
            );
        }

        private void UpdateVerticalRotation()
        {
            verticalRotator.localEulerAngles = new Vector3(
                verticalInitialRotation.x - verticalRotation,
                verticalInitialRotation.y,
                verticalInitialRotation.z
            );
        }

        private void HandleMovementEvents(bool moving)
        {
            if (moving && !isMoving)
            {
                OnMove?.Invoke();
                isMoving = true;
            }
            else if (!moving && isMoving)
            {
                OnStopMoving?.Invoke();
                isMoving = false;
            }
        }
        
        #endregion

        #region Launch System
        
        public void Launch()
        {
            if (!ValidateLaunchComponents()) return;

            // Instantiate projectile
            Rigidbody projectile = Instantiate(shell, launchPoint.position, Quaternion.identity);

            ArtilleryShellBehavior shellBehavior = projectile.GetComponent<ArtilleryShellBehavior>();
            shellBehavior.damage += additionalDamage;
            Debug.Log($"bullet does {shellBehavior.damage} damage");
            
            // Initialize projectile behaviour if present
            var behaviour = projectile.GetComponent<Blobcreate.Universal.ProjectileBehaviour>();
            behaviour?.Launch(Vector3.one);
            
            // Apply launch force
            projectile.AddForce(launchPoint.forward * launchSpeed, ForceMode.VelocityChange);
        }

        private bool ValidateLaunchComponents()
        {
            if (shell == null || launchPoint == null)
            {
                Debug.LogWarning("ArtilleryManager: Missing shell or launch point!");
                return false;
            }
            return true;
        }
        
        #endregion

        #region Public API
        
        /// <summary>
        /// Set the horizontal rotation to a specific angle
        /// </summary>
        public void SetHorizontalRotation(float angle)
        {
            horizontalRotation = constrainHorizontal 
                ? Mathf.Clamp(angle, horizontalMin, horizontalMax) 
                : angle;
            UpdateHorizontalRotation();
        }

        /// <summary>
        /// Set the vertical rotation to a specific angle
        /// </summary>
        public void SetVerticalRotation(float angle)
        {
            verticalRotation = constrainVertical 
                ? Mathf.Clamp(angle, verticalMin, verticalMax) 
                : angle;
            UpdateVerticalRotation();
        }

        /// <summary>
        /// Get the current horizontal rotation angle
        /// </summary>
        public float GetHorizontalRotation() => horizontalRotation;

        /// <summary>
        /// Get the current vertical rotation angle
        /// </summary>
        public float GetVerticalRotation() => verticalRotation;

        /// <summary>
        /// Check if horizontal rotation is at minimum limit
        /// </summary>
        public bool IsAtHorizontalMin() => constrainHorizontal && Mathf.Approximately(horizontalRotation, horizontalMin);

        /// <summary>
        /// Check if horizontal rotation is at maximum limit
        /// </summary>
        public bool IsAtHorizontalMax() => constrainHorizontal && Mathf.Approximately(horizontalRotation, horizontalMax);

        /// <summary>
        /// Check if vertical rotation is at minimum limit
        /// </summary>
        public bool IsAtVerticalMin() => constrainVertical && Mathf.Approximately(verticalRotation, verticalMin);

        /// <summary>
        /// Check if vertical rotation is at maximum limit
        /// </summary>
        public bool IsAtVerticalMax() => constrainVertical && Mathf.Approximately(verticalRotation, verticalMax);
        
        /// <summary>
        /// Increases the launch speed by the specified amount
        /// </summary>
        /// <param name="amount">Amount to apply</param>
        public void IncreaseLaunchSpeed(float amount) => launchSpeed += amount;
        
        /// <summary>
        /// Increases the rotation speed by the specified amount
        /// </summary>
        /// <param name="amount">Amount to apply</param>
        public void IncreaseRotationSpeed(float amount) => rotationSpeed += amount;

        /// <summary>
        /// Increases the damage of the shell by the specified amount
        /// </summary>
        /// <param name="amount">Amount to apply</param>
        public void IncreaseDamage(float amount) => additionalDamage += amount;

        #endregion
    }
}
