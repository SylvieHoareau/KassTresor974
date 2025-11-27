using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Cibles")]
    public Transform target; // Le joueur
    public Transform cameraTransform; // La caméra elle-même

    [Header("Paramètres")]
    public float distance = 4f;
    public float height = 1.7f;
    public float sensitivity = 150f;

    private float yaw;   // rotation horizontale
    private float pitch; // rotation verticale

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -20f, 60f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 desiredPosition = target.position
                                - rotation * Vector3.forward * distance
                                + Vector3.up * height;

        cameraTransform.position = desiredPosition;
        cameraTransform.LookAt(target.position + Vector3.up * height);
    }
}
