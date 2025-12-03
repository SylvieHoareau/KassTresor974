using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // <- IMPORTANT pour TextMeshPro

public class RomainCarDriver : MonoBehaviour
{
    [Header("Interaction")]
    [Tooltip("Tag du joueur (optionnel, sécurité en plus)")]
    public string playerTag = "Player";

    [Tooltip("Point où le joueur réapparaît en sortant de la voiture")]
    public Transform exitPoint;

    [Header("UI d'interaction")]
    [Tooltip("Texte TMP à afficher quand le joueur peut interagir (ex: 'Interagir')")]
    [SerializeField] private GameObject interactUI; // <- UI à activer dans le trigger

    [Header("Références")]
    [Tooltip("Script de caméra orbitale")]
    public RomainCameraOrbit cameraOrbit;

    [Tooltip("Contrôleur du joueur (RomainPlayerController)")]
    public RomainPlayerController playerController;

    [Header("Contrôles voiture")]
    public float acceleration = 10f;
    public float maxSpeed = 15f;
    public float turnSpeed = 70f;

    [Header("Roues qui tournent (axe X)")]
    [Tooltip("Toutes les roues qui doivent tourner en avançant/reculant")]
    public Transform[] rollingWheels;
    public float wheelRadius = 0.35f;

    [Header("Roues avant qui braquent (axe Y)")]
    [Tooltip("Les roues avant qui braquent (elles vont aussi rouler maintenant)")]
    public Transform[] frontSteerWheels;
    public float maxSteerAngle = 30f;
    public float steerLerpSpeed = 10f;

    [Header("Terrain")]
    [Tooltip("Layer du sol (ex: Ground)")]
    public LayerMask groundLayer;
    [Tooltip("Distance max du raycast vers le bas")]
    public float groundRayLength = 5f;
    [Tooltip("Offset de la voiture au-dessus du sol")]
    public float groundOffset = 0.1f;
    [Tooltip("Vitesse d'alignement à la pente")]
    public float slopeAlignSpeed = 10f;

    [Header("Direction / Réalisme")]
    [Tooltip("Vitesse minimale avant de pouvoir vraiment tourner")]
    public float minSteerSpeed = 0.5f;

    [Header("Collisions")]
    [Tooltip("Layers des murs/obstacles (ex: Default, Environment)")]
    public LayerMask obstacleLayer;
    [Tooltip("Taille demi-étendue du box de collision de la voiture")]
    public Vector3 colliderHalfExtents = new Vector3(0.8f, 0.5f, 1.5f);

    // État
    private GameObject player;
    private bool playerInRange = false;
    private bool isPlayerInside = false;

    private Transform previousCameraTarget;

    private SpriteRenderer[] playerSprites;
    private Renderer[] playerRenderers;

    private float currentSpeed = 0f;
    private float currentSteerAngle = 0f;

    // Input pour le mouvement (via le player)
    private InputAction moveAction;

    // Rotation de base des pivots de roues avant (pour éviter les glitchs)
    private Quaternion[] frontSteerBaseRotations;
    // Angle de roulage (X) pour chaque roue avant
    private float[] frontSteerRollAngles;

    private void Start()
    {
        // On mémorise les rotations de base des pivots de roues avant
        if (frontSteerWheels != null && frontSteerWheels.Length > 0)
        {
            frontSteerBaseRotations = new Quaternion[frontSteerWheels.Length];
            frontSteerRollAngles = new float[frontSteerWheels.Length];

            for (int i = 0; i < frontSteerWheels.Length; i++)
            {
                if (frontSteerWheels[i] != null)
                {
                    frontSteerBaseRotations[i] = frontSteerWheels[i].localRotation;
                    frontSteerRollAngles[i] = 0f;
                }
            }
        }

        // On s'assure que le texte d'interaction est caché au début
        if (interactUI != null)
            interactUI.SetActive(false);

        AlignToGround(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        var controller = other.GetComponentInParent<RomainPlayerController>();
        if (controller == null)
            return;

        if (!string.IsNullOrEmpty(playerTag) && !controller.CompareTag(playerTag))
            return;

        playerInRange = true;
        playerController = controller;
        player = controller.gameObject;

        moveAction = playerController.GetMoveAction();

        // Afficher le texte "Interagir" si le joueur n'est pas déjà dans la voiture
        if (!isPlayerInside && interactUI != null)
            interactUI.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        var controller = other.GetComponentInParent<RomainPlayerController>();
        if (controller == null || controller != playerController)
            return;

        playerInRange = false;

        // Masquer le texte quand le joueur quitte la zone
        if (interactUI != null)
            interactUI.SetActive(false);
    }

    private void Update()
    {
        // ====== INTERACTION CLAVIER + MANETTE ======
        bool interactPressed = false;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            interactPressed = true;

        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
            interactPressed = true;

        if (interactPressed)
        {
            if (!isPlayerInside && playerInRange)
                EnterCar();
            else if (isPlayerInside)
                ExitCar();
        }

        if (!isPlayerInside || moveAction == null)
        {
            AlignToGround(false);
            return;
        }

        Vector2 input = moveAction.ReadValue<Vector2>();
        float moveInput = input.y;
        float turnInput = input.x;

        float targetSpeed = moveInput * maxSpeed;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

        Vector3 groundNormal = GetGroundNormal();

        float speedAbs = Mathf.Abs(currentSpeed);
        bool hasTurnInput = Mathf.Abs(turnInput) > 0.01f;
        bool canSteer = speedAbs > minSteerSpeed && hasTurnInput;

        if (canSteer)
        {
            float directionSign = Mathf.Sign(currentSpeed == 0 ? 1f : currentSpeed);
            float turnAmount = turnInput * turnSpeed * Time.deltaTime * directionSign;

            if (groundNormal != Vector3.zero)
                transform.Rotate(groundNormal, turnAmount, Space.World);
            else
                transform.Rotate(0f, turnAmount, 0f, Space.World);
        }

        Vector3 forward = transform.forward;
        if (groundNormal != Vector3.zero)
        {
            forward = Vector3.ProjectOnPlane(forward, groundNormal).normalized;
        }
        else
        {
            forward.y = 0f;
            forward = forward.sqrMagnitude < 0.0001f ? Vector3.forward : forward.normalized;
        }

        Vector3 delta = forward * currentSpeed * Time.deltaTime;

        // collisions avant seulement
        if (delta.sqrMagnitude > 0.000001f && obstacleLayer != 0)
        {
            float forwardDot = 0f;
            if (delta.sqrMagnitude > 0.000001f)
                forwardDot = Vector3.Dot(delta.normalized, forward);

            bool isMovingForward = forwardDot > 0.1f;

            if (isMovingForward)
            {
                Vector3 newPosition = transform.position + delta;
                Vector3 boxCenter = newPosition + Vector3.up * colliderHalfExtents.y;

                Collider[] hits = Physics.OverlapBox(
                    boxCenter,
                    colliderHalfExtents,
                    transform.rotation,
                    obstacleLayer,
                    QueryTriggerInteraction.Ignore
                );

                bool hitRealObstacle = false;
                if (hits != null && hits.Length > 0)
                {
                    foreach (var h in hits)
                    {
                        if (h == null) continue;
                        if (!h.transform.IsChildOf(transform))
                        {
                            hitRealObstacle = true;
                            break;
                        }
                    }
                }

                if (hitRealObstacle)
                {
                    delta = Vector3.zero;
                    currentSpeed = 0f;
                }
            }
        }

        transform.position += delta;

        UpdateWheels(currentSpeed, turnInput);
        AlignToGround(false);
    }

    private void UpdateWheels(float speed, float turnInput)
    {
        float angleDelta = 0f;

        // Roulement des roues
        if (Mathf.Abs(speed) > 0.01f)
        {
            float distance = speed * Time.deltaTime;
            angleDelta = (distance / wheelRadius) * Mathf.Rad2Deg;

            // Roues "rolling" (souvent l'arrière)
            if (rollingWheels != null && rollingWheels.Length > 0)
            {
                foreach (Transform wheel in rollingWheels)
                {
                    if (wheel == null) continue;
                    wheel.Rotate(Vector3.right * angleDelta, Space.Self);
                }
            }

            // Roues avant : on stocke juste l'angle de roulage
            if (frontSteerWheels != null && frontSteerWheels.Length > 0 && frontSteerRollAngles != null)
            {
                for (int i = 0; i < frontSteerWheels.Length; i++)
                {
                    if (frontSteerWheels[i] == null) continue;
                    frontSteerRollAngles[i] += angleDelta;
                }
            }
        }

        // Braquage Y des roues avant via les pivots
        float targetSteerAngle = maxSteerAngle * turnInput;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, steerLerpSpeed * Time.deltaTime);

        if (frontSteerWheels != null && frontSteerWheels.Length > 0 && frontSteerBaseRotations != null)
        {
            for (int i = 0; i < frontSteerWheels.Length; i++)
            {
                Transform wheel = frontSteerWheels[i];
                if (wheel == null) continue;

                float roll = (frontSteerRollAngles != null && i < frontSteerRollAngles.Length)
                    ? frontSteerRollAngles[i]
                    : 0f;

                // On combine : rotation de base * roulage X * braquage Y
                wheel.localRotation =
                    frontSteerBaseRotations[i] *
                    Quaternion.Euler(roll, currentSteerAngle, 0f);
            }
        }
    }

    private Vector3 GetGroundNormal()
    {
        Vector3 origin = transform.position + Vector3.up * 2f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundRayLength, groundLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.transform.IsChildOf(transform))
                return Vector3.zero;

            return hit.normal;
        }
        return Vector3.zero;
    }

    private void AlignToGround(bool instant)
    {
        Vector3 origin = transform.position + Vector3.up * 2f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundRayLength, groundLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.transform.IsChildOf(transform))
                return;

            Vector3 targetPos = hit.point + hit.normal * groundOffset;
            transform.position = instant
                ? targetPos
                : Vector3.Lerp(transform.position, targetPos, Time.deltaTime * slopeAlignSpeed);

            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.ProjectOnPlane(transform.right, hit.normal).normalized;

            Quaternion targetRot = Quaternion.LookRotation(forward, hit.normal);
            transform.rotation = instant
                ? targetRot
                : Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * slopeAlignSpeed);
        }
    }

    private void EnterCar()
    {
        if (player == null || playerController == null)
        {
            Debug.LogWarning("RomainCarDriver : EnterCar appelé sans playerController ou player.");
            return;
        }

        isPlayerInside = true;

        // On cache le texte d'interaction quand on monte dans la voiture
        if (interactUI != null)
            interactUI.SetActive(false);

        playerController.SetCanMove(false);

        playerSprites = player.GetComponentsInChildren<SpriteRenderer>(true);
        playerRenderers = player.GetComponentsInChildren<Renderer>(true);

        if (playerSprites != null)
        {
            foreach (var sr in playerSprites)
                sr.enabled = false;
        }

        if (playerRenderers != null)
        {
            foreach (var r in playerRenderers)
                r.enabled = false;
        }

        if (cameraOrbit != null)
        {
            previousCameraTarget = cameraOrbit.target;
            cameraOrbit.target = transform;
        }

        AlignToGround(true);
    }

    private void ExitCar()
    {
        if (!isPlayerInside)
            return;

        isPlayerInside = false;
        currentSpeed = 0f;

        if (player != null && exitPoint != null)
            player.transform.position = exitPoint.position;

        if (playerSprites != null)
        {
            foreach (var sr in playerSprites)
                sr.enabled = true;
        }

        if (playerRenderers != null)
        {
            foreach (var r in playerRenderers)
                r.enabled = true;
        }

        if (playerController != null)
            playerController.SetCanMove(true);

        if (cameraOrbit != null)
        {
            if (previousCameraTarget != null)
                cameraOrbit.target = previousCameraTarget;
            else if (player != null)
                cameraOrbit.target = player.transform;
        }

        AlignToGround(true);
    }
}
