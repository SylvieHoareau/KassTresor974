using UnityEngine;

public class RomainCameraOrbit : MonoBehaviour
{
    [Header("Cible")]
    public Transform target;

    [Header("Paramètres")]
    public float distance = 5f;
    public float mouseSensitivity = 3f;
    public float minY = -40f;
    public float maxY = 70f;

    private float rotX = 0f;
    private float rotY = 0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        rotY = angles.y;
        rotX = angles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Rotation seulement quand clic droit
        if (Input.GetMouseButton(1))
        {
            rotY += Input.GetAxis("Mouse X") * mouseSensitivity;
            rotX -= Input.GetAxis("Mouse Y") * mouseSensitivity;

            rotX = Mathf.Clamp(rotX, minY, maxY);
        }

        Quaternion rotation = Quaternion.Euler(rotX, rotY, 0);

        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        transform.position = target.position + offset;

        transform.LookAt(target.position);
    }
}
