using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCursor : MonoBehaviour
{
    public Camera mainCamera; // Assign the main camera in the Inspector
    public LayerMask groundLayer; // Assign the ground layer in the Inspector
    public float maxDistance = 100f; // Maximum distance to raycast for ground

    void Update()
    {
        // Get the mouse position in screen space
        Vector2 mouseScreenPosition = Input.mousePosition;

        // Create a ray from the camera through the mouse position
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);

        // Raycast against the ground layer to find the point where the ray intersects the ground
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance, groundLayer))
        {
            // Calculate the direction vector from the player object to the hit point
            Vector3 directionToLook = hit.point - transform.position;

            // Remove any Y-axis component in the direction to make the player object only rotate on the Y-axis
            directionToLook.y = 0;

            // Rotate the player object to face the hit point
            transform.rotation = Quaternion.LookRotation(directionToLook);
        }
    }
}
