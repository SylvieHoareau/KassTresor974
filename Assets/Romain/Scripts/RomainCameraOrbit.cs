using UnityEngine;

public class RomainCameraOrbit : MonoBehaviour
{
    [Header("Cible")]
    public Transform target;

    [Header("Distance")]
    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 12f;
    public float zoomSpeed = 2f;

    [Header("Rotation")]
    public float mouseSensitivity = 3f;
    public float minY = -40f;
    public float maxY = 70f;
    public bool clampHorizontal = false;
    public float minX = -80f;
    public float maxX = 80f;

    [Header("Décalage Vertical")]
    public float heightOffset = 1.5f;

    [Header("Champ de Vision (FOV)")]
    public Camera cam;
    public float fieldOfView = 60f;
    public float minFOV = 30f;
    public float maxFOV = 90f;
    public float fovSpeed = 10f;

    private float rotX = 0f;
    private float rotY = 0f;

    void Start()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        Vector3 angles = transform.eulerAngles;
        rotY = angles.y;
        rotX = angles.x;

        cam.fieldOfView = fieldOfView;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Zoom molette
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Rotation clic droit
        if (Input.GetMouseButton(1))
        {
            rotY += Input.GetAxis("Mouse X") * mouseSensitivity;
            rotX -= Input.GetAxis("Mouse Y") * mouseSensitivity;

            rotX = Mathf.Clamp(rotX, minY, maxY);

            if (clampHorizontal)
                rotY = Mathf.Clamp(rotY, minX, maxX);
        }

        // Appliquer rotation
        Quaternion rotation = Quaternion.Euler(rotX, rotY, 0);

        // Calcul position caméra
        Vector3 offset = rotation * new Vector3(0, heightOffset, -distance);
        transform.position = target.position + offset;

        // Regarder la cible
        transform.LookAt(target.position + Vector3.up * heightOffset);

        // FOV dynamique
        fieldOfView -= scroll * fovSpeed;
        fieldOfView = Mathf.Clamp(fieldOfView, minFOV, maxFOV);
        cam.fieldOfView = fieldOfView;
    }
}
