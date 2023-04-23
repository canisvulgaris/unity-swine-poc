using UnityEngine;

public class IsometricCameraFollow : MonoBehaviour
{
    public Transform target; // Assign the player object's transform in the Inspector
    public Vector3 offset; // Set the desired offset from the target in the Inspector
    public Vector3 positionOffset;
    public float smoothSpeed = 0.125f; // Smoothing factor for camera movement

    private void Update()
    {
        // Calculate the desired position of the camera
        Vector3 desiredPosition = target.position + positionOffset + offset;

        // Smoothly interpolate between the current camera position and the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Update the camera position
        transform.position = smoothedPosition;

        // Make the camera look at the target
        transform.LookAt(target);
    }
}
