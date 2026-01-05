using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class RomainPlayerController3D : MonoBehaviour
{
    [Header("Déplacement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float rotationLerp = 0.15f;

    [Header("Sprint (Toggle)")]
    [Tooltip("Si vrai: tu appuies 1 fois sur Shift = sprint activé, même si tu t'arrêtes puis repars (pas besoin de rappuyer).")]
    [SerializeField] private bool sprintTogglePersistsWhenStopping = true;

    [Header("Saut")]
    [SerializeField] private float jumpForce = 7f;

    [Header("Détection du sol")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Jump Feel")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float fallGravityMultiplier = 2f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("Timing du saut")]
    [SerializeField] private float jumpDelay = 0.1f;
    private bool jumpQueued = false;
    private float jumpDelayTimer = 0f;

    [Header("Double saut")]
    [SerializeField] private int maxJumps = 2;
    private int jumpCount = 0;

    [Header("Input (New Input System)")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction; // Sprint toggle

    [Header("Animation (Animator 3D du collègue)")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animSpeedParam = "Speed";
    [SerializeField] private string animRunParam = "IsRunning";
    [SerializeField] private string animJumpTrigger = "Jump";

    [Tooltip("Optionnel: bool sol (ex: IsGrounded). Laisse vide si tu n'en as pas.")]
    [SerializeField] private string animGroundedBool = "";

    [Header("Sons de pas")]
    [Tooltip("Clips joués aléatoirement quand le joueur marche/court.")]
    [SerializeField] private AudioClip[] footstepClips;

    [Tooltip("Intervalle entre 2 pas en marche.")]
    [SerializeField] private float walkStepInterval = 0.5f;

    [Tooltip("Intervalle entre 2 pas en sprint.")]
    [SerializeField] private float runStepInterval = 0.32f;

    [Tooltip("Random léger sur le pitch pour éviter l'effet mitraillette.")]
    [Range(0f, 0.3f)]
    [SerializeField] private float pitchRandom = 0.1f;

    [Tooltip("Optionnel: si vide, prend l'AudioSource sur ce GameObject.")]
    [SerializeField] private AudioSource footstepSource;

    private Rigidbody rb;
    private Camera cam;

    private Vector2 moveInputRaw = Vector2.zero;
    private Vector3 moveDirection = Vector3.zero;

    private bool canMove = true;

    private bool isGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool jumpPressedThisFrame;
    private bool isJumpHeld;

    private bool isSprinting = false;
    private bool sprintLatched = false;

    private float stepTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        cam = Camera.main;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (footstepSource == null)
            footstepSource = GetComponent<AudioSource>(); // pas obligatoire, mais pratique

        // Si tu n'as pas d'AudioSource, tu peux en ajouter automatiquement (optionnel)
        if (footstepSource == null)
            footstepSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.Enable();
            moveAction.action.performed += OnMovePerformed;
            moveAction.action.canceled += OnMoveCanceled;
        }

        if (jumpAction != null)
        {
            jumpAction.action.Enable();
            jumpAction.action.performed += OnJumpPerformed;
            jumpAction.action.canceled += OnJumpCanceled;
        }

        if (sprintAction != null)
        {
            sprintAction.action.Enable();
            sprintAction.action.performed += OnSprintPerformed;
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.performed -= OnMovePerformed;
            moveAction.action.canceled -= OnMoveCanceled;
            moveAction.action.Disable();
        }

        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJumpPerformed;
            jumpAction.action.canceled -= OnJumpCanceled;
            jumpAction.action.Disable();
        }

        if (sprintAction != null)
        {
            sprintAction.action.performed -= OnSprintPerformed;
            sprintAction.action.Disable();
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx) => moveInputRaw = ctx.ReadValue<Vector2>();
    private void OnMoveCanceled(InputAction.CallbackContext ctx) => moveInputRaw = Vector2.zero;

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        jumpPressedThisFrame = true;
        isJumpHeld = true;
    }

    private void OnJumpCanceled(InputAction.CallbackContext ctx) => isJumpHeld = false;

    private void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        // Toggle du sprint (1 pression = ON, 1 autre = OFF)
        sprintLatched = !sprintLatched;

        // Si on bouge déjà, on applique tout de suite
        if (moveInputRaw.sqrMagnitude > 0.01f)
            isSprinting = sprintLatched;
    }

    private void Update()
    {
        if (cam == null) cam = Camera.main;

        UpdateGrounded();

        if (jumpPressedThisFrame)
            jumpBufferTimer = jumpBufferTime;

        jumpBufferTimer -= Time.deltaTime;

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            jumpCount = 0;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        bool canFirstJump = (jumpBufferTimer > 0 && coyoteTimer > 0);
        bool canDoubleJump = (!isGrounded && jumpCount < maxJumps && jumpBufferTimer > 0);

        if (canMove && !jumpQueued && (canFirstJump || canDoubleJump))
        {
            jumpQueued = true;
            jumpDelayTimer = jumpDelay;
            jumpBufferTimer = 0;
            coyoteTimer = 0;
        }

        if (jumpQueued)
        {
            jumpDelayTimer -= Time.deltaTime;

            if (!canMove)
            {
                jumpQueued = false;
            }
            else if (jumpDelayTimer <= 0f)
            {
                DoJump();
                jumpQueued = false;
            }
        }

        jumpPressedThisFrame = false;

        // Sprint toggle logique (avec option de persistance)
        bool hasMove = moveInputRaw.sqrMagnitude >= 0.01f;

        if (!hasMove)
        {
            isSprinting = false;

            // Option: si faux, on reset le toggle quand on s'arrête (comportement "faut rappuyer")
            if (!sprintTogglePersistsWhenStopping)
                sprintLatched = false;
        }
        else
        {
            // Si on bouge, on suit l'état du toggle
            isSprinting = sprintLatched;
        }

        // Sons de pas (repris du 1er script, adapté au Rigidbody)
        HandleFootsteps(hasMove, isGrounded, isSprinting);

        UpdateAnimatorParameters();
    }

    private void FixedUpdate()
    {
        if (cam == null) cam = Camera.main;

        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 targetDir = camForward * moveInputRaw.y + camRight * moveInputRaw.x;
        if (targetDir.sqrMagnitude > 1f) targetDir.Normalize();
        moveDirection = targetDir;

        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        Vector3 vel = GetLinearVelocity();
        Vector3 horizontal = (canMove && moveDirection.sqrMagnitude > 0.001f)
            ? moveDirection.normalized * currentSpeed
            : Vector3.zero;

        SetLinearVelocity(new Vector3(horizontal.x, vel.y, horizontal.z));

        vel = GetLinearVelocity();
        if (vel.y < 0f)
            SetLinearVelocity(vel + Vector3.up * Physics.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime);
        else if (vel.y > 0f && !isJumpHeld)
            SetLinearVelocity(vel + Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime);

        if (moveDirection.sqrMagnitude > 0.001f && canMove)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection, Vector3.up);
            Quaternion smoothRot = Quaternion.Slerp(rb.rotation, targetRot, rotationLerp);
            rb.MoveRotation(smoothRot);
        }
    }

    private void HandleFootsteps(bool hasInput, bool grounded, bool running)
    {
        if (!hasInput || !grounded || footstepClips == null || footstepClips.Length == 0 || footstepSource == null)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;
        float interval = running ? runStepInterval : walkStepInterval;

        if (stepTimer <= 0f)
        {
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            footstepSource.pitch = 1f + Random.Range(-pitchRandom, pitchRandom);
            footstepSource.PlayOneShot(clip);
            stepTimer = interval;
        }
    }

    private void UpdateGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckRadius * 2f, groundLayer);
            return;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void DoJump()
    {
        jumpCount++;

        Vector3 vel = GetLinearVelocity();
        SetLinearVelocity(new Vector3(vel.x, jumpForce, vel.z));
        isGrounded = false;

        SafeSetTrigger(animator, animJumpTrigger);
    }

    private void UpdateAnimatorParameters()
    {
        if (animator == null) return;

        Vector3 v = GetLinearVelocity();
        Vector3 planar = new Vector3(v.x, 0f, v.z);
        float speed = planar.magnitude;

        SafeSetFloat(animator, animSpeedParam, speed);
        SafeSetBool(animator, animRunParam, speed > 0.1f && isSprinting);

        if (!string.IsNullOrEmpty(animGroundedBool))
            SafeSetBool(animator, animGroundedBool, isGrounded);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!value)
        {
            moveInputRaw = Vector2.zero;
            moveDirection = Vector3.zero;

            var v = GetLinearVelocity();
            SetLinearVelocity(new Vector3(0f, v.y, 0f));

            isSprinting = false;
            sprintLatched = false;

            jumpQueued = false;

            // Bonus: coupe les pas instant si tu disable le move
            stepTimer = 0f;
        }
    }

    public void TeleportTo(Vector3 pos)
    {
        SetLinearVelocity(Vector3.zero);
        transform.position = pos;
    }

    public InputAction GetMoveAction()
    {
        return moveAction != null ? moveAction.action : null;
    }

    private Vector3 GetLinearVelocity()
    {
#if UNITY_6000_0_OR_NEWER
        return rb.linearVelocity;
#else
        return rb.velocity;
#endif
    }

    private void SetLinearVelocity(Vector3 v)
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = v;
#else
        rb.velocity = v;
#endif
    }

    private static void SafeSetFloat(Animator a, string param, float value)
    {
        if (a == null || string.IsNullOrEmpty(param)) return;
        if (HasParam(a, param)) a.SetFloat(param, value);
    }

    private static void SafeSetBool(Animator a, string param, bool value)
    {
        if (a == null || string.IsNullOrEmpty(param)) return;
        if (HasParam(a, param)) a.SetBool(param, value);
    }

    private static void SafeSetTrigger(Animator a, string param)
    {
        if (a == null || string.IsNullOrEmpty(param)) return;
        if (HasParam(a, param)) a.SetTrigger(param);
    }

    private static bool HasParam(Animator a, string paramName)
    {
        foreach (var p in a.parameters)
            if (p.name == paramName) return true;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
