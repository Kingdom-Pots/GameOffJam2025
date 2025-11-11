using Blobcreate.Universal;
using UnityEngine;

namespace Blobcreate.ProjectileToolkit.Demo
{
    public class ArtilleryManager : MonoBehaviour
    {
        public Rigidbody shell;
        public Transform launchPoint;
        public float launchSpeed = 20f;
//
//        void Update()
//        {
//            if (Input.GetMouseButtonDown(0))
//            {
//                Launch();
//            }
//        }

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
        }
    }
}