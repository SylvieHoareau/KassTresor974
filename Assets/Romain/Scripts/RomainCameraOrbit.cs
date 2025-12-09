using UnityEngine;
using UnityEngine.InputSystem;

public enum MouseRotateMode
{
    Always,         // La souris fait tourner la caméra dès qu'elle bouge
    RightMouseOnly  // La souris fait tourner la caméra uniquement quand clic droit est maintenu
}

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

    [Header("Souris")]
    public MouseRotateMode mouseRotateMode = MouseRotateMode.Always;

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

    [Header("Collision caméra")]
    [Tooltip("Empêcher la caméra de passer à travers le sol / murs")]
    public bool enableCollision = true;
    [Tooltip("Layers considérés comme obstacles (terrain, murs, etc.)")]
    public LayerMask collisionLayers = ~0;  // par défaut : tout
    [Tooltip("Rayon du SphereCast pour éviter de rentrer dans les objets")]
    public float collisionRadius = 0.3f;
    [Tooltip("Distance de sécurité par rapport à la surface touchée")]
    public float collisionBuffer = 0.2f;

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
        float lookSqrMag = look.sqrMagnitude;

        var lookCtrl = lookAction != null ? lookAction.action.activeControl : null;
        bool fromGamepadLook = lookCtrl != null && lookCtrl.device is Gamepad;
        bool fromMouseLook   = lookCtrl != null && lookCtrl.device is Mouse;

        // SOURIS
        if (fromMouseLook && lookSqrMag > 0.0001f)
        {
            bool allowMouse =
                mouseRotateMode == MouseRotateMode.Always ||
                (mouseRotateMode == MouseRotateMode.RightMouseOnly &&
                 Mouse.current != null &&
                 Mouse.current.rightButton.isPressed);

            if (allowMouse)
            {
                rotY += look.x * mouseSensitivity;
                rotX -= look.y * mouseSensitivity;
            }
        }
        // GAMEPAD
        else if (fromGamepadLook && lookSqrMag > 0.0001f)
        {
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

            bool isMoving = moveMag > moveThreshold;
            bool isLooking = lookMag > lookDeadZone;

            bool fromGamepadMove = moveAction.action.activeControl != null &&
                                   moveAction.action.activeControl.device is Gamepad;

            if (isMoving && !isLooking && (!onlyGamepad || fromGamepadMove))
            {
                float targetYaw = target.eulerAngles.y;
                rotY = Mathf.LerpAngle(rotY, targetYaw, alignSpeed * Time.deltaTime);
            }
        }

        // ----- POSITION / ROTATION CAMERA -----
        Quaternion rotation = Quaternion.Euler(rotX, rotY, 0);

        // Position "théorique" de la caméra (comme avant)
        Vector3 rawOffset = rotation * new Vector3(0, heightOffset, -distance);
        Vector3 desiredPosition = target.position + rawOffset;

        // Point que la caméra regarde (le haut du perso)
        Vector3 focusPoint = target.position + Vector3.up * heightOffset;

        // COLLISION : empêcher la caméra de passer sous la map / dans les murs
        if (enableCollision)
        {
            Vector3 dir = (desiredPosition - focusPoint).normalized;
            float maxDist = Vector3.Distance(focusPoint, desiredPosition);

            if (maxDist > 0.001f)
            {
                if (Physics.SphereCast(
                        focusPoint,
                        collisionRadius,
                        dir,
                        out RaycastHit hit,
                        maxDist,
                        collisionLayers,
                        QueryTriggerInteraction.Ignore))
                {
                    float safeDist = hit.distance - collisionBuffer;
                    if (safeDist < 0.1f) safeDist = 0.1f;
                    desiredPosition = focusPoint + dir * safeDist;
                }
            }
        }

        transform.position = desiredPosition;
        transform.LookAt(focusPoint);

        // ----- FOV DYNAMIQUE -----
        fieldOfView -= scroll * fovSpeed * 0.1f;
        fieldOfView = Mathf.Clamp(fieldOfView, minFOV, maxFOV);
        cam.fieldOfView = fieldOfView;
    }
}
