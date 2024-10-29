using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl: MonoBehaviour
{
    public float mouseSensitivity = 100f;  // Mouse sensitivity for rotation speed
    public Transform playerBody;           // Reference to the root player body for horizontal rotation

    private float xRotation = 0f;          // Tracks vertical rotation (clamped between -90 and 90 degrees)

    private void Start()
    {
        // Lock the cursor to the center of the screen and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Get mouse input for both axes
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Horizontal rotation (left/right) applied to playerBody (entire player)
        playerBody.Rotate(Vector3.up * mouseX);

        // Vertical rotation (up/down) applied to the camera itself
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Clamp to prevent excessive up/down rotation

        // Apply the rotation to the camera's local X-axis (up/down)
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
