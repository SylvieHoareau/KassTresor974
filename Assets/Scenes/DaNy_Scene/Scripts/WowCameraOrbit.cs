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

    private float yaw;   // rotation horizontale (autour de Y)
    private float pitch; // rotation verticale (haut/bas)

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("WowCameraOrbit : pas de target assigné !");
            return;
        }

        // Init yaw/pitch à partir de la position actuelle de la caméra
        Vector3 dir = (transform.position - target.position).normalized;
        yaw   = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        pitch = Mathf.Asin(dir.y) * Mathf.Rad2Deg;
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
        // IMPORTANT : pas de "else" → si on ne clique pas, on ne touche pas à yaw/pitch

        // --- Zoom molette ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // --- Calcul final de la position / rotation caméra ---
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPos   = target.position + Vector3.up * heightOffset;
        Vector3 desiredPos  = targetPos - rotation * Vector3.forward * distance;

        transform.position = desiredPos;
        transform.rotation = rotation;
    }
}
