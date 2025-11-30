using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform playerBody;  // ton Player (avec le collider)
    public float mouseSensitivity = 100f;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotation verticale (caméra) – on clamp
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f); // pas plus haut/bas que ça

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotation horizontale (corps du joueur)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
