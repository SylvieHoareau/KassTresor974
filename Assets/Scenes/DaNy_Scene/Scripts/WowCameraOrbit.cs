using UnityEngine;

public class WowCameraOrbit : MonoBehaviour
{
    [Header("Cible")]
    public Transform target;
    public float heightOffset = 1.7f;

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
    public float yawOffset = 0f;

    [Header("Lissage")]
    [Tooltip("Plus haut = plus doux (mais plus 'flottant')")]
    public float rotationSmoothTime = 0.08f; // 0.05 - 0.15
    public float positionSmoothTime = 0.06f; // 0.03 - 0.12
    public bool smoothOnlyWhenRotating = false;

    private float yaw;
    private float pitch;

    // valeurs cibles "brutes"
    private float targetYaw;
    private float targetPitch;

    // vitesses pour SmoothDamp
    private float yawVel;
    private float pitchVel;
    private Vector3 posVel;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("WowCameraOrbit : pas de target assigné !");
            enabled = false;
            return;
        }

        targetYaw = target.eulerAngles.y + yawOffset;
        targetPitch = 15f;

        yaw = targetYaw;
        pitch = targetPitch;

        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // place direct au départ
        SnapToTarget();
    }

    void LateUpdate()
    {
        if (target == null) return;

        bool rotateMousePressed = Input.GetMouseButton(0) || Input.GetMouseButton(1);

        // --- Input rotation ---
        if (rotateMousePressed)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            targetYaw += mouseX * mouseSensitivity;
            targetPitch -= mouseY * mouseSensitivity;
            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
        }

        // --- Zoom ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // --- Lissage angles ---
        bool doSmooth = !smoothOnlyWhenRotating || rotateMousePressed;

        if (doSmooth)
        {
            yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref yawVel, rotationSmoothTime);
            pitch = Mathf.SmoothDampAngle(pitch, targetPitch, ref pitchVel, rotationSmoothTime);
        }
        else
        {
            yaw = targetYaw;
            pitch = targetPitch;
        }

        UpdateCameraPositionSmooth();
    }

    private void UpdateCameraPositionSmooth()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPos = target.position + Vector3.up * heightOffset;
        Vector3 desiredPos = targetPos - rotation * Vector3.forward * distance;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref posVel, positionSmoothTime);
        transform.rotation = rotation;
    }

    private void SnapToTarget()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPos = target.position + Vector3.up * heightOffset;
        Vector3 desiredPos = targetPos - rotation * Vector3.forward * distance;

        transform.position = desiredPos;
        transform.rotation = rotation;
    }
}
