using UnityEngine;
using UnityEngine.InputSystem;

public class RomainCarDriver : MonoBehaviour
{
    [Header("Interaction")]
    [Tooltip("Tag du joueur (optionnel, sécurité en plus)")]
    public string playerTag = "Player";

    [Tooltip("Point où le joueur réapparaît en sortant de la voiture")]
    public Transform exitPoint;

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
    [Tooltip("Uniquement les PIVOTS des roues avant (pas les meshes + parent en même temps)")]
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

    private void Start()
    {
        // On mémorise les rotations de base des pivots de roues avant
        if (frontSteerWheels != null && frontSteerWheels.Length > 0)
        {
            frontSteerBaseRotations = new Quaternion[frontSteerWheels.Length];
            for (int i = 0; i < frontSteerWheels.Length; i++)
            {
                if (frontSteerWheels[i] != null)
                    frontSteerBaseRotations[i] = frontSteerWheels[i].localRotation;
            }
        }

        // On colle la voiture au terrain au départ
        AlignToGround(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Cherche RomainPlayerController dans les parents du collider
        var controller = other.GetComponentInParent<RomainPlayerController>();
        if (controller == null)
            return;

        if (!string.IsNullOrEmpty(playerTag) && !controller.CompareTag(playerTag))
            return;

        playerInRange = true;
        playerController = controller;
        player = controller.gameObject;

        // Récupère Move via ton getter dans RomainPlayerController
        moveAction = playerController.GetMoveAction();
    }

    private void OnTriggerExit(Collider other)
    {
        var controller = other.GetComponentInParent<RomainPlayerController>();
        if (controller == null || controller != playerController)
            return;

        playerInRange = false;
    }

    private void Update()
    {
        // Interaction avec E (New Input System)
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!isPlayerInside && playerInRange)
            {
                EnterCar();
            }
            else if (isPlayerInside)
            {
                ExitCar();
            }
        }

        if (!isPlayerInside || moveAction == null)
        {
            AlignToGround(false);
            return;
        }

        // ===== MOUVEMENT MANUEL SUR LE TERRAIN =====
        Vector2 input = moveAction.ReadValue<Vector2>();
        float moveInput = input.y;
        float turnInput = input.x;

        // Accélération / frein
        float targetSpeed = moveInput * maxSpeed;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

        // On récupère la normale du sol sous la voiture
        Vector3 groundNormal = GetGroundNormal();

        // ===== ROTATION : seulement si on roule déjà un minimum =====
        float speedAbs = Mathf.Abs(currentSpeed);
        bool hasTurnInput = Mathf.Abs(turnInput) > 0.01f;
        bool canSteer = speedAbs > minSteerSpeed && hasTurnInput;

        if (canSteer)
        {
            // sens de braquage selon direction (marche avant / arrière)
            float directionSign = Mathf.Sign(currentSpeed == 0 ? 1f : currentSpeed);
            float turnAmount = turnInput * turnSpeed * Time.deltaTime * directionSign;

            if (groundNormal != Vector3.zero)
                transform.Rotate(groundNormal, turnAmount, Space.World);
            else
                transform.Rotate(0f, turnAmount, 0f, Space.World);
        }

        // Forward projeté sur le plan du sol pour se déplacer sur la pente
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

        // Avancer le long du sol
        Vector3 delta = forward * currentSpeed * Time.deltaTime;
        transform.position += delta;

        // Roues
        UpdateWheels(currentSpeed, turnInput);

        // On recolle au terrain (position & rotation fine)
        AlignToGround(false);
    }

    private void UpdateWheels(float speed, float turnInput)
    {
        // Rotation X des roues (roulement)
        if (rollingWheels != null && rollingWheels.Length > 0 && Mathf.Abs(speed) > 0.01f)
        {
            float distance = speed * Time.deltaTime;
            float angleDelta = (distance / wheelRadius) * Mathf.Rad2Deg;

            foreach (Transform wheel in rollingWheels)
            {
                if (wheel == null) continue;
                wheel.Rotate(Vector3.right * angleDelta, Space.Self);
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

                // Rotation = rotation de base * rotation de braquage, pour éviter de "tasser" les roues
                wheel.localRotation = frontSteerBaseRotations[i] * Quaternion.Euler(0f, currentSteerAngle, 0f);
            }
        }
    }

    /// <summary>
    /// Retourne la normale du terrain sous la voiture (ou Vector3.zero si rien).
    /// </summary>
    private Vector3 GetGroundNormal()
    {
        Vector3 origin = transform.position + Vector3.up * 2f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundRayLength, groundLayer, QueryTriggerInteraction.Ignore))
        {
            // Si jamais le raycast touche un collider de la voiture → ignore
            if (hit.collider.transform.IsChildOf(transform))
                return Vector3.zero;

            return hit.normal;
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Aligne la voiture au terrain : position + rotation alignée à la pente.
    /// </summary>
    private void AlignToGround(bool instant)
    {
        Vector3 origin = transform.position + Vector3.up * 2f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundRayLength, groundLayer, QueryTriggerInteraction.Ignore))
        {
            // Ignore si on tape la voiture elle-même
            if (hit.collider.transform.IsChildOf(transform))
                return;

            // Position plaquée au sol + offset
            Vector3 targetPos = hit.point + hit.normal * groundOffset;
            transform.position = instant
                ? targetPos
                : Vector3.Lerp(transform.position, targetPos, Time.deltaTime * slopeAlignSpeed);

            // Rotation alignée à la pente
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

        // On coupe les mouvements du player proprement
        playerController.SetCanMove(false);

        // On cache le joueur (sprites / mesh)
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

        // Caméra → voiture
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

        // Replace le joueur
        if (player != null && exitPoint != null)
        {
            player.transform.position = exitPoint.position;
        }

        // Affiche de nouveau le joueur
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

        // Rends le contrôle au joueur
        if (playerController != null)
            playerController.SetCanMove(true);

        // Caméra → retour sur ce qu’elle suivait avant (ou le joueur)
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
