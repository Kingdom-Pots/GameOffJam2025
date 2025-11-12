using Blobcreate.Universal;
using UnityEngine;

namespace Blobcreate.ProjectileToolkit.Demo
{
    public class ArtilleryManager : MonoBehaviour
    {
        public Rigidbody shell;
        public Transform launchPoint;
        public float launchSpeed = 20f;

        [Header("Trajectory Preview")]
        [Tooltip("Show trajectory line in game")]
        public bool showTrajectory = true;

        [Tooltip("Number of points to calculate for trajectory")]
        public int trajectoryPoints = 30;

        [Tooltip("Time step between trajectory points")]
        public float trajectoryTimeStep = 0.1f;

        [Tooltip("Landing point marker (optional)")]
        public GameObject landingMarker;

        [Tooltip("Trajectory line renderer")]
        public LineRenderer trajectoryLine;

        [Header("Visual Settings")]
        public Color trajectoryColor = Color.red;
        public float lineWidth = 0.1f;

        private Vector3 predictedLandingPoint;
        private GameObject instantiatedMarker;

        void Start()
        {
            // Create line renderer if not assigned
            if (trajectoryLine == null && showTrajectory)
            {
                GameObject lineObj = new GameObject("TrajectoryLine");
                lineObj.transform.SetParent(transform);
                trajectoryLine = lineObj.AddComponent<LineRenderer>();

                // Configure line renderer
                trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
                trajectoryLine.startColor = trajectoryColor;
                trajectoryLine.endColor = trajectoryColor;
                trajectoryLine.startWidth = lineWidth;
                trajectoryLine.endWidth = lineWidth;
                trajectoryLine.positionCount = 0;
            }
        }

        void Update()
        {
            if (showTrajectory)
            {
                UpdateTrajectoryPreview();
            }
        }

        void UpdateTrajectoryPreview()
        {
            if (launchPoint == null || trajectoryLine == null) return;

            Vector3 velocity = launchPoint.forward * launchSpeed;
            Vector3 position = launchPoint.position;
            Vector3 gravity = Physics.gravity;

            trajectoryLine.positionCount = trajectoryPoints;
            bool foundLanding = false;

            for (int i = 0; i < trajectoryPoints; i++)
            {
                trajectoryLine.SetPosition(i, position);

                // Calculate next position
                float time = trajectoryTimeStep;
                Vector3 newPosition = position + velocity * time + 0.5f * gravity * time * time;
                velocity += gravity * time;

                // Check if hit ground
                if (!foundLanding && CheckGroundHit(position, newPosition, out Vector3 hitPoint))
                {
                    predictedLandingPoint = hitPoint;
                    foundLanding = true;

                    // Trim trajectory at landing point
                    trajectoryLine.positionCount = i + 1;
                    trajectoryLine.SetPosition(i, hitPoint);

                    UpdateLandingMarker(hitPoint);
                    break;
                }

                position = newPosition;
            }

            if (!foundLanding)
            {
                // Estimate landing point at end of trajectory
                predictedLandingPoint = position;
                UpdateLandingMarker(position);
            }
        }

        bool CheckGroundHit(Vector3 start, Vector3 end, out Vector3 hitPoint)
        {
            Vector3 direction = end - start;
            float distance = direction.magnitude;

            if (Physics.Raycast(start, direction.normalized, out RaycastHit hit, distance))
            {
                hitPoint = hit.point;
                return true;
            }

            hitPoint = Vector3.zero;
            return false;
        }

        void UpdateLandingMarker(Vector3 position)
        {
            if (landingMarker == null) return;

            // Create marker if doesn't exist
            if (instantiatedMarker == null)
            {
                instantiatedMarker = Instantiate(landingMarker, position, Quaternion.identity);
                instantiatedMarker.name = "Landing Point Marker";
            }
            else
            {
                instantiatedMarker.transform.position = position;
            }
        }

        public void Launch()
        {
            if (shell == null || launchPoint == null)
            {
                Debug.LogWarning("Missing required components for launch!");
                return;
            }

            // Spawn bullet at launch point
            var bullet = Instantiate(shell, launchPoint.position, Quaternion.identity);

            // Launch in the direction the launch point is facing
            var projectileBehaviour = bullet.GetComponent<ProjectileBehaviour>();
            if (projectileBehaviour != null)
            {
                projectileBehaviour.Launch(Vector3.one);
            }

            bullet.AddForce(launchPoint.forward * launchSpeed, ForceMode.VelocityChange);

            Debug.Log($"Launched! Predicted landing: {predictedLandingPoint}");
        }

        public Vector3 GetPredictedLandingPoint()
        {
            return predictedLandingPoint;
        }

        // Draw trajectory in editor
        void OnDrawGizmos()
        {
            if (!showTrajectory || launchPoint == null) return;

            Vector3 velocity = launchPoint.forward * launchSpeed;
            Vector3 position = launchPoint.position;
            Vector3 gravity = Physics.gravity;

            Gizmos.color = Color.yellow;
            Vector3 previousPosition = position;

            for (int i = 0; i < trajectoryPoints; i++)
            {
                float time = trajectoryTimeStep;
                Vector3 newPosition = position + velocity * time + 0.5f * gravity * time * time;
                velocity += gravity * time;

                Gizmos.DrawLine(previousPosition, newPosition);

                // Check ground hit
                if (CheckGroundHit(previousPosition, newPosition, out Vector3 hitPoint))
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(hitPoint, 0.5f);
                    break;
                }

                previousPosition = newPosition;
                position = newPosition;
            }
        }

        void OnDestroy()
        {
            // Clean up marker
            if (instantiatedMarker != null)
            {
                Destroy(instantiatedMarker);
            }
        }
    }
}