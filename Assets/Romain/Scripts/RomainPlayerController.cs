using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class RomainPlayerController : MonoBehaviour
{
    [Header("Déplacement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float rotationLerp = 0.15f;

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
    [SerializeField] private float jumpDelay = 0.1f; // délai avant l'impulsion (pour caler avec l'anim)
    private bool jumpQueued = false;
    private float jumpDelayTimer = 0f;

    [Header("Double saut")]
    [SerializeField] private int maxJumps = 2;
    private int jumpCount = 0;

    [Header("Dash aérien")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.2f;
    private bool isDashing = false;
    private bool hasAirDashed = false;
    private float dashTimer = 0f;
    private Vector3 dashDirection = Vector3.zero;
    private bool dashRequested = false;

    [Header("Roulade (au sol)")]
    [SerializeField] private float rollSpeed = 8f;
    [SerializeField] private float rollDuration = 0.4f;
    private bool isRolling = false;
    private float rollTimer = 0f;
    private Vector3 rollDirection = Vector3.zero;
    private bool rollRequested = false;

    [Header("Input (New Input System)")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference dashAction;
    [SerializeField] private InputActionReference sprintAction;  // Sprint toggle
    [SerializeField] private InputActionReference rollAction;    // Roulade

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private Rigidbody rb;
    private Camera cam;

    // Input brut
    private Vector2 moveInputRaw = Vector2.zero;

    // Direction de déplacement (relative caméra)
    private Vector3 moveDirection = Vector3.zero;

    private bool canMove = true;

    private bool isGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool jumpPressedThisFrame;
    private bool isJumpHeld;

    // Sprint
    private bool isSprinting = false;      // utilisé pour la vitesse & l'anim
    private bool sprintLatched = false;    // états du toggle

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        cam = Camera.main;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.Enable();
            moveAction.action.performed += OnMovePerformed;
            moveAction.action.canceled  += OnMoveCanceled;
        }

        if (jumpAction != null)
        {
            jumpAction.action.Enable();
            jumpAction.action.performed += OnJumpPerformed;
            jumpAction.action.canceled  += OnJumpCanceled;
        }

        if (dashAction != null)
        {
            dashAction.action.Enable();
            dashAction.action.performed += OnDashPerformed;
        }

        if (sprintAction != null)
        {
            sprintAction.action.Enable();
            sprintAction.action.performed += OnSprintPerformed; // toggle
        }

        if (rollAction != null)
        {
            rollAction.action.Enable();
            rollAction.action.performed += OnRollPerformed;
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.performed -= OnMovePerformed;
            moveAction.action.canceled  -= OnMoveCanceled;
            moveAction.action.Disable();
        }

        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJumpPerformed;
            jumpAction.action.canceled  -= OnJumpCanceled;
            jumpAction.action.Disable();
        }

        if (dashAction != null)
        {
            dashAction.action.performed -= OnDashPerformed;
            dashAction.action.Disable();
        }

        if (sprintAction != null)
        {
            sprintAction.action.performed -= OnSprintPerformed;
            sprintAction.action.Disable();
        }

        if (rollAction != null)
        {
            rollAction.action.performed -= OnRollPerformed;
            rollAction.action.Disable();
        }
    }

    // ===== INPUT CALLBACKS =====

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        moveInputRaw = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInputRaw = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        jumpPressedThisFrame = true;
        isJumpHeld = true;
    }

    private void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        isJumpHeld = false;
    }

    private void OnDashPerformed(InputAction.CallbackContext ctx)
    {
        dashRequested = true;
    }

    private void OnRollPerformed(InputAction.CallbackContext ctx)
    {
        rollRequested = true;
    }

    private void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        // Toggle du sprint
        sprintLatched = !sprintLatched;

        // On applique immédiatement si on bouge déjà
        if (moveInputRaw.sqrMagnitude > 0.01f)
            isSprinting = sprintLatched;
    }

    // ===== UPDATE =====

    private void Update()
    {
        if (cam == null)
            cam = Camera.main;

        UpdateGrounded();

        // Jump buffer
        if (jumpPressedThisFrame)
            jumpBufferTimer = jumpBufferTime;

        jumpBufferTimer -= Time.deltaTime;

        // Coyote + reset sauts & dash
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            jumpCount = 0;
            hasAirDashed = false;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        bool canFirstJump  = (jumpBufferTimer > 0 && coyoteTimer > 0);
        bool canDoubleJump = (!isGrounded && jumpCount < maxJumps && jumpBufferTimer > 0);

        // On ne déclenche le saut que si pas déjà en attente
        if (canMove && !jumpQueued && (canFirstJump || canDoubleJump))
        {
            jumpQueued = true;
            jumpDelayTimer = jumpDelay;
            jumpBufferTimer = 0;
            coyoteTimer = 0;

            // On lance l'animation de "pré-saut" pour le premier saut seulement
            if (animator != null && jumpCount == 0)
            {
                animator.ResetTrigger("JumpStart");
                animator.SetTrigger("JumpStart");
            }
        }

        // Gestion du délai de saut
        if (jumpQueued)
        {
            jumpDelayTimer -= Time.deltaTime;

            // Si on ne peut plus bouger (dialogue, cinématique) → on annule
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

        // ROULADE AU SOL
        if (canMove && rollRequested && isGrounded && !isRolling && !isDashing)
        {
            StartRoll();
        }
        rollRequested = false;

        // DASH AÉRIEN
        if (canMove && dashRequested && !isGrounded && !isDashing && !hasAirDashed && !isRolling)
        {
            StartDash();
        }
        dashRequested = false;

        // Gestion du sprint : 
        // - si plus de mouvement → on désactive tout
        // - si on bouge → on suit le latch
        if (moveInputRaw.sqrMagnitude < 0.01f)
        {
            isSprinting = false;
            sprintLatched = false; // il faudra rappuyer pour resprinter
        }
        else
        {
            isSprinting = sprintLatched;
        }

        UpdateAnimatorParameters();
    }

    // ===== FIXEDUPDATE =====

    private void FixedUpdate()
    {
        // 1) ROULADE
        if (isRolling)
        {
            rollTimer -= Time.fixedDeltaTime;

            rb.linearVelocity = new Vector3(
                rollDirection.x * rollSpeed,
                rb.linearVelocity.y,
                rollDirection.z * rollSpeed
            );

            if (rollDirection.sqrMagnitude > 0.001f)
            {
                Quaternion rollRot = Quaternion.LookRotation(rollDirection, Vector3.up);
                Quaternion smoothRollRot = Quaternion.Slerp(rb.rotation, rollRot, rotationLerp);
                rb.MoveRotation(smoothRollRot);
            }

            if (rollTimer <= 0f)
            {
                isRolling = false;
            }

            return;
        }

        // 2) DASH
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;

            rb.linearVelocity = new Vector3(
                dashDirection.x * dashSpeed,
                0f,
                dashDirection.z * dashSpeed
            );

            if (dashDirection.sqrMagnitude > 0.001f)
            {
                Quaternion dashRot = Quaternion.LookRotation(dashDirection, Vector3.up);
                Quaternion smoothDashRot = Quaternion.Slerp(rb.rotation, dashRot, rotationLerp);
                rb.MoveRotation(smoothDashRot);
            }

            if (dashTimer <= 0f)
            {
                isDashing = false;
            }

            return;
        }

        if (cam == null)
            cam = Camera.main;

        // Projection input dans l'espace caméra
        Vector3 camForward = cam.transform.forward;
        Vector3 camRight   = cam.transform.right;
        camForward.y = 0f;
        camRight.y   = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 targetDir = camForward * moveInputRaw.y + camRight * moveInputRaw.x;

        if (targetDir.sqrMagnitude > 1f)
            targetDir.Normalize();

        moveDirection = targetDir;

        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontal = (canMove && moveDirection.sqrMagnitude > 0.001f)
            ? moveDirection.normalized * currentSpeed
            : Vector3.zero;

        rb.linearVelocity = new Vector3(
            horizontal.x,
            velocity.y,
            horizontal.z
        );

        // Gravité "better jump"
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !isJumpHeld)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }

        // Rotation automatique vers la direction de déplacement
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection, Vector3.up);
            Quaternion smoothRot = Quaternion.Slerp(rb.rotation, targetRot, rotationLerp);
            rb.MoveRotation(smoothRot);
        }
    }

    // ===== SAUT / SOL =====

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

        Vector3 vel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(vel.x, jumpForce, vel.z);
        isGrounded = false;

        // Si tu veux une anim spécifique pour le double saut, tu peux la gérer ici :
        // if (animator != null && jumpCount > 1) { ... }
    }

    private void StartDash()
    {
        hasAirDashed = true;
        isDashing = true;
        dashTimer = dashDuration;

        if (moveDirection.sqrMagnitude > 0.1f)
            dashDirection = moveDirection.normalized;
        else
            dashDirection = transform.forward;

        rb.linearVelocity = new Vector3(dashDirection.x * dashSpeed, 0f, dashDirection.z * dashSpeed);
    }

    private void StartRoll()
    {
        isRolling = true;
        rollTimer = rollDuration;

        if (moveDirection.sqrMagnitude > 0.1f)
            rollDirection = moveDirection.normalized;
        else
            rollDirection = transform.forward;

        rb.linearVelocity = new Vector3(
            rollDirection.x * rollSpeed,
            rb.linearVelocity.y,
            rollDirection.z * rollSpeed
        );

        if (animator != null)
        {
            animator.ResetTrigger("Roll");
            animator.SetTrigger("Roll");
        }
    }

    // ===== ANIMATION =====

    private void UpdateAnimatorParameters()
    {
        if (animator == null) return;

        Vector3 horizontalVel = rb.linearVelocity;
        horizontalVel.y = 0f;

        float speed = horizontalVel.magnitude;

        // Vitesse dans l'espace local du perso
        Vector3 localVel = Vector3.zero;
        if (speed > 0.1f)
            localVel = transform.InverseTransformDirection(horizontalVel).normalized;

        animator.SetFloat("Hor",  localVel.x, 0.1f, Time.deltaTime);
        animator.SetFloat("Vert", localVel.z, 0.1f, Time.deltaTime);

        float state = 0f;
        if (speed > 0.1f)
        {
            if (isSprinting || isDashing)
                state = 1f;      // Run
            else
                state = 0.5f;    // Walk
        }

        animator.SetFloat("State", state, 0.1f, Time.deltaTime);
        animator.SetBool("IsJump", !isGrounded);
        animator.SetBool("IsRolling", isRolling);
    }

    // ===== UTILITIES =====

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!value)
        {
            moveInputRaw = Vector2.zero;
            moveDirection = Vector3.zero;
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

            isDashing = false;
            isRolling = false;
            isSprinting = false;
            sprintLatched = false;
            jumpQueued = false;
        }
    }

    public void TeleportTo(Vector3 pos)
    {
        rb.linearVelocity = Vector3.zero;
        transform.position = pos;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
