using UnityEngine;

public class AimAtCamera : MonoBehaviour
{
void LateUpdate()
{
    // Get the main camera
    Camera mainCamera = Camera.main;

    // Calculate direction to camera
    Vector3 directionToCamera = (mainCamera.transform.position - transform.position).normalized;

    // Flip the direction if needed (use -directionToCamera for reverse facing)
    directionToCamera.y = 0; // Optional: Keep the UI upright

    // Apply rotation
    transform.rotation = Quaternion.LookRotation(-directionToCamera);
}
}
