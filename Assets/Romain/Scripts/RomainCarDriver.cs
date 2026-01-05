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

    [Header("Chemin (anti hors-piste)")]
    [Tooltip("Active le mode 'la voiture suit un chemin' : le joueur avance, et la direction est contrainte/assistée.")]
    [SerializeField] private bool usePath = true;

    [Tooltip("Points du chemin (dans l'ordre). Place des empties le long de la route.")]
    [SerializeField] private Transform[] pathPoints;

    [Tooltip("Distance en mètres visée devant la voiture sur le chemin (plus grand = plus doux, moins précis).")]
    [SerializeField] private float pathLookAhead = 6f;

    [Tooltip("Largeur du corridor: si la voiture s'éloigne plus que ça du chemin, on la ramène.")]
    [SerializeField] private float pathMaxLateralDistance = 3f;

    [Tooltip("Force de retour vers le chemin (0 = pas de retour).")]
    [SerializeField] private float pathSnapStrength = 6f;

    [Tooltip("Aide directionnelle. 1 = normal, >1 = plus agressif.")]
    [SerializeField] private float autoSteerStrength = 1.2f;

    [Tooltip("Si vrai, interdit de reculer (souvent utile en mode 'ride on rails').")]
    [SerializeField] private bool forbidReverse = false;

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

    // ===================== AUDIO AJOUTÉ =====================
    [Header("Audio voiture")]
    [Tooltip("AudioSource pour le bruit de démarrage (Loop OFF, Play On Awake OFF)")]
    [SerializeField] private AudioSource engineStartSource;

    [Tooltip("AudioSource pour le bruit moteur léger (Loop ON, Play On Awake OFF)")]
    [SerializeField] private AudioSource engineLoopSource;

    [Tooltip("Délai avant de lancer le loop moteur après le start")]
    [SerializeField] private float engineLoopDelay = 0.8f;

    [Tooltip("Pitch moteur au ralenti")]
    [SerializeField] private float engineMinPitch = 0.9f;

    [Tooltip("Pitch moteur à vitesse max")]
    [SerializeField] private float engineMaxPitch = 1.2f;

    [Tooltip("Active la variation de pitch selon la vitesse")]
    [SerializeField] private bool enginePitchWithSpeed = true;
    // ========================================================

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

    // Path cache (pour éviter de scanner tous les segments inutilement)
    private int lastClosestSegment = 0;

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

        // Sécurité audio: on démarre éteint
        if (engineLoopSource != null)
            engineLoopSource.Stop();

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

        if (forbidReverse)
            moveInput = Mathf.Max(0f, moveInput);

        float targetSpeed = moveInput * maxSpeed;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

        Vector3 groundNormal = GetGroundNormal();

        // --------- PATH: calcule un turnInput automatique ----------
        if (usePath && pathPoints != null && pathPoints.Length >= 2)
        {
            float autoTurn = ComputeAutoSteerOnPath(groundNormal);
            turnInput = Mathf.Clamp(autoTurn * autoSteerStrength, -1f, 1f);

            // Optionnel: si tu veux autoriser un peu le joueur à corriger, décommente:
            // turnInput = Mathf.Clamp(turnInput + input.x * 0.25f, -1f, 1f);

            // Optionnel: ramener dans le corridor
            ApplyPathSnapIfNeeded();
        }
        // ----------------------------------------------------------

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

        // =================== AUDIO: pitch moteur ====================
        UpdateEngineAudio();
    }

    // ===================== PATH HELPERS =====================

    private float ComputeAutoSteerOnPath(Vector3 groundNormal)
    {
        // On cherche le point le plus proche sur la polyline + un point look-ahead
        Vector3 closest, tangent;
        float lateralDist;
        GetClosestPointAndTangentOnPath(transform.position, out closest, out tangent, out lateralDist);

        Vector3 lookTarget = closest + tangent * pathLookAhead;

        // Direction désirée sur le plan du sol (évite les angles bizarres en pente)
        Vector3 desiredDir = (lookTarget - transform.position);
        if (groundNormal != Vector3.zero)
            desiredDir = Vector3.ProjectOnPlane(desiredDir, groundNormal);
        desiredDir.y = 0f;

        if (desiredDir.sqrMagnitude < 0.0001f)
            return 0f;

        desiredDir.Normalize();

        Vector3 fwd = transform.forward;
        if (groundNormal != Vector3.zero)
            fwd = Vector3.ProjectOnPlane(fwd, groundNormal).normalized;
        else
        {
            fwd.y = 0f;
            fwd = fwd.sqrMagnitude < 0.0001f ? Vector3.forward : fwd.normalized;
        }

        // Angle signé autour de la normale (ou Y si pas de normale)
        Vector3 axis = (groundNormal != Vector3.zero) ? groundNormal : Vector3.up;
        float signedAngle = Vector3.SignedAngle(fwd, desiredDir, axis);

        // Map angle -> [-1,1]
        // "35° = braquage complet" est un bon départ.
        const float fullSteerAngle = 35f;
        float turn = Mathf.Clamp(signedAngle / fullSteerAngle, -1f, 1f);

        return turn;
    }

    private void ApplyPathSnapIfNeeded()
    {
        if (pathSnapStrength <= 0f) return;

        Vector3 closest, tangent;
        float lateralDist;
        GetClosestPointAndTangentOnPath(transform.position, out closest, out tangent, out lateralDist);

        if (lateralDist <= pathMaxLateralDistance) return;

        // Ramène progressivement vers le chemin (pas un teleport sec)
        Vector3 toPath = (closest - transform.position);
        // On évite de changer la hauteur brutalement, AlignToGround gère déjà ça
        toPath.y = 0f;

        transform.position += toPath * Mathf.Clamp01(Time.deltaTime * pathSnapStrength);
    }

    private void GetClosestPointAndTangentOnPath(
        Vector3 worldPos,
        out Vector3 closestPoint,
        out Vector3 tangent,
        out float lateralDistance)
    {
        closestPoint = worldPos;
        tangent = transform.forward;
        lateralDistance = 0f;

        if (pathPoints == null || pathPoints.Length < 2)
            return;

        int segCount = pathPoints.Length - 1;

        // On cherche autour du dernier segment trouvé pour optimiser,
        // mais on reste safe si le joueur arrive n'importe où.
        int start = Mathf.Clamp(lastClosestSegment - 2, 0, segCount - 1);
        int end = Mathf.Clamp(lastClosestSegment + 2, 0, segCount - 1);

        float bestSqr = float.MaxValue;
        int bestSeg = lastClosestSegment;
        Vector3 bestPoint = worldPos;
        Vector3 bestTangent = transform.forward;

        // Petit fallback: si path est tordu et qu’on s’est perdu, on scanne tout.
        // (segCount est souvent petit, donc c’est ok.)
        bool didFullScan = false;

        for (int pass = 0; pass < 2; pass++)
        {
            int a = (pass == 0) ? start : 0;
            int b = (pass == 0) ? end : segCount - 1;

            for (int i = a; i <= b; i++)
            {
                Transform p0t = pathPoints[i];
                Transform p1t = pathPoints[i + 1];
                if (p0t == null || p1t == null) continue;

                Vector3 p0 = p0t.position;
                Vector3 p1 = p1t.position;

                Vector3 pointOnSeg = ClosestPointOnSegment(worldPos, p0, p1);
                float sqr = (worldPos - pointOnSeg).sqrMagnitude;

                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    bestSeg = i;
                    bestPoint = pointOnSeg;
                    Vector3 segDir = (p1 - p0);
                    segDir.y = 0f;
                    bestTangent = segDir.sqrMagnitude < 0.0001f ? transform.forward : segDir.normalized;
                }
            }

            if (bestSqr < float.MaxValue * 0.5f) break; // trouvé un truc correct
            if (pass == 0 && !didFullScan)
            {
                didFullScan = true;
                // deuxième pass = full scan
            }
        }

        lastClosestSegment = bestSeg;
        closestPoint = bestPoint;
        tangent = bestTangent;
        lateralDistance = Mathf.Sqrt(bestSqr);
    }

    private static Vector3 ClosestPointOnSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float abSqr = Vector3.Dot(ab, ab);
        if (abSqr <= 0.000001f) return a;

        float t = Vector3.Dot(p - a, ab) / abSqr;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }

    // ===================== END PATH HELPERS =====================

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

        // ===== AUDIO: start + loop =====
        if (engineStartSource != null)
            engineStartSource.Play();

        if (engineLoopSource != null)
        {
            engineLoopSource.Stop();
            engineLoopSource.pitch = engineMinPitch;
            engineLoopSource.PlayDelayed(Mathf.Max(0f, engineLoopDelay));
        }

        AlignToGround(true);
    }

    private void ExitCar()
    {
        if (!isPlayerInside) return;

        isPlayerInside = false;
        currentSpeed = 0f;

        // ===== AUDIO: stop loop =====
        if (engineLoopSource != null)
            engineLoopSource.Stop();

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

        lastInteractTime = Time.time;

        AlignToGround(true);
    }

    private void UpdateEngineAudio()
    {
        if (!isPlayerInside) return;
        if (engineLoopSource == null) return;
        if (!enginePitchWithSpeed) return;

        float t = (maxSpeed <= 0.0001f) ? 0f : Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
        engineLoopSource.pitch = Mathf.Lerp(engineMinPitch, engineMaxPitch, t);
    }

    private void TeleportPlayerSafely(GameObject p, Vector3 pos, Quaternion rot)
    {
        var cc = p.GetComponent<CharacterController>();
        bool ccWasEnabled = false;
        if (cc != null)
        {
            ccWasEnabled = cc.enabled;
            cc.enabled = false;
        }

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
        // OverlapBox collisions debug
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.up * colliderHalfExtents.y, colliderHalfExtents * 2f);

        // Path debug
        if (pathPoints != null && pathPoints.Length >= 2)
        {
            Gizmos.matrix = Matrix4x4.identity;
            for (int i = 0; i < pathPoints.Length - 1; i++)
            {
                if (pathPoints[i] == null || pathPoints[i + 1] == null) continue;
                Gizmos.DrawLine(pathPoints[i].position, pathPoints[i + 1].position);
            }
        }
    }
#endif
}
