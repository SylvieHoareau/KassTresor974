using UnityEngine;
using UnityEngine.InputSystem;

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
    public float mouseSensitivity = 0.1f;      // souris (Delta)
    public float gamepadSensitivity = 120f;    // stick droit
    public float minY = -40f;
    public float maxY = 70f;

    [Header("Offset")]
    public float heightOffset = 1.5f;

    [Header("FOV")]
    public Camera cam;
    public float fieldOfView = 60f;
    public float minFOV = 30f;
    public float maxFOV = 90f;
    public float fovSpeed = 10f;

    [Header("Input System")]
    public InputActionReference lookAction;    // Player/Look
    public InputActionReference moveAction;    // Player/Move (stick gauche)

    [Header("Recentrage Auto")]
    public bool autoAlignOnMove = true;
    [Tooltip("Vitesse de recentrage quand on se déplace à la manette")]
    public float alignSpeed = 5f;
    [Tooltip("Minimum de déplacement pour déclencher le recentrage")]
    public float moveThreshold = 0.2f;
    [Tooltip("Si on bouge la caméra plus que ça, on coupe le recentrage")]
    public float lookDeadZone = 0.1f;
    [Tooltip("Ne recentrer que si l’input vient d’une manette")]
    public bool onlyGamepad = true;

    private float rotX;
    private float rotY;

    void OnEnable()
    {
        lookAction?.action.Enable();
        moveAction?.action.Enable();
    }

    void OnDisable()
    {
        lookAction?.action.Disable();
        moveAction?.action.Disable();
    }

    void Start()
    {
        if (cam == null) cam = GetComponent<Camera>();

        Vector3 angles = transform.eulerAngles;
        rotY = angles.y;
        rotX = angles.x;

        cam.fieldOfView = fieldOfView;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // ----- ZOOM -----
        float scroll = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
        distance -= scroll * zoomSpeed * 0.1f; // 0.1 pour calmer la molette
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // ----- LOOK (souris + stick droit) -----
        Vector2 look = lookAction != null ? lookAction.action.ReadValue<Vector2>() : Vector2.zero;

        // On détecte si c’est de la souris (delta non nul)
        bool usingMouse = Mouse.current != null &&
                          Mouse.current.delta.ReadValue() != Vector2.zero &&
                          look.sqrMagnitude > 0.0001f;

        if (usingMouse)
        {
            // Souris : delta pixels -> petit facteur
            rotY += look.x * mouseSensitivity;
            rotX -= look.y * mouseSensitivity;
        }
        else
        {
            // Manette : valeur normalisée [-1,1]
            rotY += look.x * gamepadSensitivity * Time.deltaTime;
            rotX -= look.y * gamepadSensitivity * Time.deltaTime;
        }

        rotX = Mathf.Clamp(rotX, minY, maxY);

        // ----- RECENTRAGE AUTO SUR LA DIRECTION DU PERSO -----
        if (autoAlignOnMove && moveAction != null && target != null)
        {
            Vector2 move = moveAction.action.ReadValue<Vector2>();
            float moveMag = move.magnitude;
            float lookMag = look.magnitude;

            // Est-ce qu’on bouge assez ?
            bool isMoving = moveMag > moveThreshold;

            // Est-ce que la caméra n’est pas en train d’être manipulée ?
            bool isLooking = lookMag > lookDeadZone;

            // Est-ce que l’input vient d’une manette ?
            bool fromGamepad = moveAction.action.activeControl != null &&
                               moveAction.action.activeControl.device is Gamepad;

            if (isMoving && !isLooking && (!onlyGamepad || fromGamepad))
            {
                float targetYaw = target.eulerAngles.y;
                rotY = Mathf.LerpAngle(rotY, targetYaw, alignSpeed * Time.deltaTime);
            }
        }

        // ----- POSITION / ROTATION CAMERA -----
        Quaternion rotation = Quaternion.Euler(rotX, rotY, 0);
        Vector3 offset = rotation * new Vector3(0, heightOffset, -distance);

        transform.position = target.position + offset;
        transform.LookAt(target.position + Vector3.up * heightOffset);

        // ----- FOV DYNAMIQUE -----
        fieldOfView -= scroll * fovSpeed * 0.1f;
        fieldOfView = Mathf.Clamp(fieldOfView, minFOV, maxFOV);
        cam.fieldOfView = fieldOfView;
    }
}
