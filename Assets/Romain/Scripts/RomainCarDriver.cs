using UnityEngine;
using UnityEngine.InputSystem;

public class RomainCarDriver : MonoBehaviour
{
    [Header("Interaction")]
    [Tooltip("Tag du joueur (optionnel, sécurité en plus)")]
    public string playerTag = "Player";

    [Tooltip("Point où le joueur réapparaît en sortant de la voiture")]
    public Transform exitPoint;

    [Header("UI d'interaction")]
    [Tooltip("Texte TMP à afficher quand le joueur peut interagir (ex: 'Interagir')")]
    [SerializeField] private GameObject interactUI;

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
    public Transform[] rollingWheels;
    public float wheelRadius = 0.35f;

    [Header("Roues avant qui braquent (axe Y)")]
    public Transform[] frontSteerWheels;
    public float maxSteerAngle = 30f;
    public float steerLerpSpeed = 10f;

    [Header("Terrain")]
    public LayerMask groundLayer;
    public float groundRayLength = 5f;
    public float groundOffset = 0.1f;
    public float slopeAlignSpeed = 10f;

    [Header("Direction / Réalisme")]
    public float minSteerSpeed = 0.5f;

    [Header("Collisions")]
    public LayerMask obstacleLayer;
    public Vector3 colliderHalfExtents = new Vector3(0.8f, 0.5f, 1.5f);

    [Tooltip("Si activé, la marche arrière peut traverser les obstacles (comme avant). " +
             "Si désactivé, les collisions sont prises en compte aussi en marche arrière.")]
    public bool allowReverseThroughObstacles = true;

    [Header("Interaction - Sécurité")]
    [SerializeField] private float interactCooldown = 0.25f;
    private float lastInteractTime = -999f;

    [Header("Sortie voiture - Sécurité")]
    [Tooltip("Décalage latéral (porte) appliqué à la sortie par rapport à l'exitPoint")]
    [SerializeField] private float exitSideOffset = 0.25f;

    [Tooltip("Si coché, on 'snap' la sortie au sol avec un raycast")]
    [SerializeField] private bool snapExitToGround = true;

    [Tooltip("Distance max du raycast pour snap la sortie au sol")]
    [SerializeField] private float exitGroundRay = 4f;

    // État
    private GameObject player;
    private bool playerInRange = false;
    private bool isPlayerInside = false;

    private Transform previousCameraTarget;

    private SpriteRenderer[] playerSprites;
    private Renderer[] playerRenderers;

    private float currentSpeed = 0f;
    private float currentSteerAngle = 0f;

    // Input mouvement voiture
    private InputAction moveAction;

    private Quaternion[] frontSteerBaseRotations;
    private float[] frontSteerRollAngles;

    // Pour gérer IsPressed proprement
    private bool interactHeldLastFrame = false;

    private void Start()
    {
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

        if (interactUI != null)
            interactUI.SetActive(false);

        if (exitPoint != null && !exitPoint.IsChildOf(transform))
            Debug.LogWarning("RomainCarDriver : ExitPoint n'est PAS enfant de la voiture. Risque de sortie incohérente.");

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

        if (!isPlayerInside && interactUI != null)
            interactUI.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        var controller = other.GetComponentInParent<RomainPlayerController>();
        if (controller == null || controller != playerController)
            return;

        playerInRange = false;

        if (interactUI != null)
            interactUI.SetActive(false);
    }

    private void Update()
    {
        HandleInteractionInput();

        if (!isPlayerInside || moveAction == null)
        {
            AlignToGround(false);
            return;
        }

        // =================== MOUVEMENT VOITURE ====================
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
            forward = Vector3.ProjectOnPlane(forward, groundNormal).normalized;
        else
        {
            forward.y = 0f;
            forward = forward.sqrMagnitude < 0.0001f ? Vector3.forward : forward.normalized;
        }

        Vector3 delta = forward * currentSpeed * Time.deltaTime;

        // ====== COLLISIONS AVEC OBSTACLES (avant / arrière suivant l’option) ======
        if (delta.sqrMagnitude > 0.000001f && obstacleLayer != 0)
        {
            bool shouldCheckCollision;

            if (allowReverseThroughObstacles)
            {
                float forwardDot = Vector3.Dot(delta.normalized, forward);
                bool isMovingForward = forwardDot > 0.1f;
                shouldCheckCollision = isMovingForward;
            }
            else
            {
                shouldCheckCollision = true;
            }

            if (shouldCheckCollision)
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

    private void HandleInteractionInput()
    {
        if (!playerInRange && !isPlayerInside)
            return;

        bool interactHeld =
            (Keyboard.current != null && Keyboard.current.eKey.IsPressed()) ||
            (Gamepad.current != null && Gamepad.current.buttonWest.IsPressed());

        // Si la touche était déjà maintenue à la frame précédente, on ignore pour éviter le spam
        if (interactHeld && interactHeldLastFrame)
        {
            interactHeldLastFrame = interactHeld;
            return;
        }

        // Transition "pas appuyée" -> "appuyée" = press unique
        bool interactPressed = interactHeld && !interactHeldLastFrame;

        interactHeldLastFrame = interactHeld;

        if (!interactPressed)
            return;

        if (Time.time < lastInteractTime + interactCooldown)
            return;

        lastInteractTime = Time.time;

        if (!isPlayerInside && playerInRange)
        {
            EnterCar();
        }
        else if (isPlayerInside)
        {
            ExitCar();
        }
    }

    private void UpdateWheels(float speed, float turnInput)
    {
        float angleDelta = 0f;

        if (Mathf.Abs(speed) > 0.01f)
        {
            float distance = speed * Time.deltaTime;
            angleDelta = (distance / wheelRadius) * Mathf.Rad2Deg;

            if (rollingWheels != null && rollingWheels.Length > 0)
            {
                foreach (Transform wheel in rollingWheels)
                {
                    if (wheel == null) continue;
                    wheel.Rotate(Vector3.right * angleDelta, Space.Self);
                }
            }

            if (frontSteerWheels != null && frontSteerWheels.Length > 0)
            {
                for (int i = 0; i < frontSteerWheels.Length; i++)
                {
                    if (frontSteerWheels[i] == null) continue;
                    frontSteerRollAngles[i] += angleDelta;
                }
            }
        }

        float targetSteerAngle = maxSteerAngle * turnInput;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, steerLerpSpeed * Time.deltaTime);

        if (frontSteerWheels != null && frontSteerWheels.Length > 0)
        {
            for (int i = 0; i < frontSteerWheels.Length; i++)
            {
                Transform wheel = frontSteerWheels[i];
                if (wheel == null) continue;

                float roll = frontSteerRollAngles[i];

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
            if (!hit.collider.transform.IsChildOf(transform))
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
        playerInRange = false;

        if (interactUI != null)
            interactUI.SetActive(false);

        playerController.SetCanMove(false);

        playerSprites = player.GetComponentsInChildren<SpriteRenderer>(true);
        playerRenderers = player.GetComponentsInChildren<Renderer>(true);

        if (playerSprites != null)
            foreach (var sr in playerSprites) if (sr) sr.enabled = false;

        if (playerRenderers != null)
            foreach (var r in playerRenderers) if (r) r.enabled = false;

        if (cameraOrbit != null)
        {
            previousCameraTarget = cameraOrbit.target;
            cameraOrbit.target = transform;
        }

        AlignToGround(true);
    }

    private void ExitCar()
    {
        if (!isPlayerInside) return;

        isPlayerInside = false;
        currentSpeed = 0f;

        // IMPORTANT : on aligne la voiture AVANT de calculer la sortie,
        // sinon exitPoint (enfant) peut "bouger" après et créer des incohérences.
        AlignToGround(true);

        if (player != null && exitPoint != null)
        {
            Vector3 exitPos = exitPoint.position + (exitPoint.right * exitSideOffset);
            if (snapExitToGround)
                exitPos = SnapToGround(exitPos);

            TeleportPlayerSafely(player, exitPos, exitPoint.rotation);
        }
        else if (player != null && exitPoint == null)
        {
            Debug.LogWarning("RomainCarDriver : exitPoint manquant, sortie à la position actuelle du joueur.");
            TeleportPlayerSafely(player, player.transform.position, player.transform.rotation);
        }

        if (playerSprites != null)
            foreach (var sr in playerSprites) if (sr) sr.enabled = true;

        if (playerRenderers != null)
            foreach (var r in playerRenderers) if (r) r.enabled = true;

        if (playerController != null)
            playerController.SetCanMove(true);

        if (cameraOrbit != null)
        {
            if (previousCameraTarget != null)
                cameraOrbit.target = previousCameraTarget;
            else if (player != null)
                cameraOrbit.target = player.transform;
        }

        // Anti double toggle instantané
        lastInteractTime = Time.time;

        AlignToGround(true);
    }

    private void TeleportPlayerSafely(GameObject p, Vector3 pos, Quaternion rot)
    {
        // Le CharacterController peut annuler/corriger un teleport si tu le bouges activé
        var cc = p.GetComponent<CharacterController>();
        bool ccWasEnabled = false;
        if (cc != null)
        {
            ccWasEnabled = cc.enabled;
            cc.enabled = false;
        }

        // Si Rigidbody, on passe par rb.position/rotation + reset vitesses
        var rb = p.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = pos;
            rb.rotation = rot;
        }
        else
        {
            p.transform.SetPositionAndRotation(pos, rot);
        }

        Physics.SyncTransforms();

        if (cc != null && ccWasEnabled)
            cc.enabled = true;
    }

    private Vector3 SnapToGround(Vector3 pos)
    {
        Vector3 origin = pos + Vector3.up * 1.5f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, exitGroundRay, groundLayer, QueryTriggerInteraction.Ignore))
        {
            return hit.point + hit.normal * 0.05f;
        }
        return pos;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Visualiser le volume d'OverlapBox pour debug collisions
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.up * colliderHalfExtents.y, colliderHalfExtents * 2f);
    }
#endif
}
