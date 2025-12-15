using UnityEngine;

public class WowCameraOrbit : MonoBehaviour
{
    [Header("Cible")]
    public Transform target;          // ton joueur
    public float heightOffset = 1.7f; // hauteur du point à suivre (tête du perso)

    [Header("Distance")]
    public float distance = 6f;
    public float minDistance = 3f;
    public float maxDistance = 15f;
    public float zoomSpeed = 5f;

    [Header("Rotation")]
    public float mouseSensitivity = 3f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    [Header("Alignement")]
    [Tooltip("Correction si la caméra n'est pas pile derrière le perso")]
    public float yawOffset = 0f;   // corrige le décalage gauche/droite

    private float yaw;   // rotation horizontale (autour de Y)
    private float pitch; // rotation verticale (haut/bas)

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("WowCameraOrbit : pas de target assigné !");
            return;
        }

        // On démarre derrière le perso
        yaw   = target.eulerAngles.y + yawOffset; // <- offset de correction
        pitch = 15f;

        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        UpdateCameraPosition();
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // --- Rotation avec la souris (clic gauche OU clic droit) ---
        bool rotateMousePressed = Input.GetMouseButton(0) || Input.GetMouseButton(1);

        if (rotateMousePressed)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw   += mouseX * mouseSensitivity;
            pitch -= mouseY * mouseSensitivity;

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // --- Zoom molette ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPos   = target.position + Vector3.up * heightOffset;
        Vector3 desiredPos  = targetPos - rotation * Vector3.forward * distance;

        transform.position = desiredPos;
        transform.rotation = rotation;
    }
}
