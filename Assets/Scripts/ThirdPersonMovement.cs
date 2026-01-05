using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Déplacement")]
    public float walkSpeed = 4f;
    public float runSpeed  = 7f;
    public float rotationSpeed = 10f;

    [Header("Saut & Gravité")]
    public float jumpHeight = 2f;
    public float gravity    = -20f;
    public float coyoteTime = 0.15f;

    [Header("Sol (stabilité)")]
    public float groundedGraceTime = 0.08f;
    public float groundedStickVelocity = -2f;

    [Header("Références")]
    public Transform cameraTransform;
    public Animator animator;

    [Header("Sons de pas")]
    public AudioClip[] footstepClips;
    public float walkStepInterval = 0.5f;
    public float runStepInterval  = 0.32f;
    [Range(0f, 0.3f)] public float pitchRandom = 0.1f;

    private CharacterController controller;
    private AudioSource footstepSource;

    private float verticalVelocity;
    private float coyoteTimer;
    private float lastGroundedTime;
    private bool  jumpConsumed;

    private float stepTimer;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        footstepSource = GetComponent<AudioSource>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // === INPUT CLAVIER ===
        float horizontal = 0f;
        float vertical   = 0f;

        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D))                           horizontal += 1f;
        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.W)) vertical   += 1f;
        if (Input.GetKey(KeyCode.S))                            vertical   -= 1f;

        bool runKeyPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jumpPressed   = Input.GetKeyDown(KeyCode.Space);

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;
        bool hasInput = inputDir.sqrMagnitude > 0.01f;

        // === DIRECTION CAMERA ===
        Vector3 moveDir = Vector3.zero;

        if (hasInput)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight   = cameraTransform.right;

            camForward.y = 0f;
            camRight.y   = 0f;
            camForward.Normalize();
            camRight.Normalize();

            moveDir = (camForward * inputDir.z + camRight * inputDir.x).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // === SOL / COYOTE ===
        bool rawGrounded = controller.isGrounded;

        if (rawGrounded)
            lastGroundedTime = Time.time;

        bool isGroundedStable = rawGrounded || (Time.time - lastGroundedTime) <= groundedGraceTime;

        if (isGroundedStable)
        {
            coyoteTimer = coyoteTime;
            jumpConsumed = false;

            if (verticalVelocity < 0f)
                verticalVelocity = groundedStickVelocity;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        // === SAUT / ANIM ===
        if (jumpPressed)
        {
            if (!hasInput)
            {
                // À l'arrêt : animation uniquement (pas de saut physique)
                if (animator != null)
                {
                    animator.ResetTrigger("Jump");
                    animator.SetTrigger("Jump");
                }
                // On ne consomme pas le saut, et on ne touche pas verticalVelocity.
            }
            else
            {
                // En mouvement : saut physique normal (coyote)
                if (!jumpConsumed && coyoteTimer > 0f)
                {
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                    coyoteTimer = 0f;
                    jumpConsumed = true;

                    if (animator != null)
                    {
                        animator.ResetTrigger("Jump");
                        animator.SetTrigger("Jump");
                    }
                }
            }
        }

        // Gravité
        verticalVelocity += gravity * Time.deltaTime;

        // === MOUVEMENT ===
        float currentSpeed = hasInput ? (runKeyPressed ? runSpeed : walkSpeed) : 0f;
        Vector3 horizontalVelocity = moveDir * currentSpeed;

        Vector3 finalMove = horizontalVelocity;
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);

        // === SONS DE PAS ===
        HandleFootsteps(hasInput, isGroundedStable, runKeyPressed);

        // === ANIMATOR ===
        if (animator != null)
        {
            Vector3 planarVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
            animator.SetFloat("Speed", planarVelocity.magnitude);
            animator.SetBool("IsRunning", hasInput && runKeyPressed);
        }
    }

    private void HandleFootsteps(bool hasInput, bool isGrounded, bool isRunning)
    {
        if (!hasInput || !isGrounded || footstepClips == null || footstepClips.Length == 0)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;
        float interval = isRunning ? runStepInterval : walkStepInterval;

        if (stepTimer <= 0f)
        {
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            footstepSource.pitch = 1f + Random.Range(-pitchRandom, pitchRandom);
            footstepSource.PlayOneShot(clip);
            stepTimer = interval;
        }
    }
}
