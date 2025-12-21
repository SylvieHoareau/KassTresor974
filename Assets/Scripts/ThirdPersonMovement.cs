using UnityEngine;

[RequireComponent(typeof(CharacterController))]
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

    private CharacterController controller;

    private float verticalVelocity;
    private float coyoteTimer;

    private float lastGroundedTime;
    private bool  jumpConsumed;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // === INPUT CLAVIER (ZQSD / WASD) ===
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

        // === Déplacement relatif caméra ===
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
                // À l'arrêt : on joue juste l'animation, sans saut physique
                if (animator != null)
                {
                    animator.ResetTrigger("Jump");
                    animator.SetTrigger("Jump");
                }

                // Important : ne pas consommer le saut, ne pas modifier verticalVelocity
                // Comme ça, dès que tu bouges + espace, le saut normal marche.
            }
            else
            {
                // En mouvement : saut physique normal (avec coyote)
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

        // Vitesse horizontale
        float currentSpeed = 0f;
        if (hasInput)
            currentSpeed = runKeyPressed ? runSpeed : walkSpeed;

        Vector3 horizontalVelocity = moveDir * currentSpeed;

        // Mouvement final
        Vector3 finalMove = horizontalVelocity;
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);

        // Animator locomotion
        if (animator != null)
        {
            Vector3 planarVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
            float speed = planarVelocity.magnitude;

            animator.SetFloat("Speed", speed);
            animator.SetBool("IsRunning", hasInput && runKeyPressed);
        }
    }
}
