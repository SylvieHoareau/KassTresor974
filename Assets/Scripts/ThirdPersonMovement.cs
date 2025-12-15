using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Déplacement")]
    public float walkSpeed = 4f;
    public float runSpeed  = 7f;

    [Header("Saut & Gravité")]
    public float jumpHeight = 2f;
    public float gravity    = -20f;
    public float coyoteTime = 0.15f;

    [Header("Références")]
    public Transform cameraTransform;
    public Animator animator;

    private CharacterController controller;
    private float verticalVelocity;
    private float coyoteTimer;

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

        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A))
            horizontal -= 1f;
        if (Input.GetKey(KeyCode.D))
            horizontal += 1f;
        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.W))
            vertical   += 1f;
        if (Input.GetKey(KeyCode.S))
            vertical   -= 1f;

        bool runKeyPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jumpPressed   = Input.GetKeyDown(KeyCode.Space);

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;
        bool hasInput    = inputDir.sqrMagnitude > 0.01f;

        Vector3 moveDir = Vector3.zero;

        if (hasInput)
        {
            // Direction RELATIVE à la caméra
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight   = cameraTransform.right;

            camForward.y = 0f;
            camRight.y   = 0f;

            camForward.Normalize();
            camRight.Normalize();

            // Z = avant / arrière, X = gauche / droite
            moveDir = camForward * inputDir.z + camRight * inputDir.x;
            moveDir.Normalize();

            // Direction de regard = direction de déplacement
            // -> si tu appuies S, moveDir pointe vers "l'arrière" de la caméra
            //    et le perso se tourne pour marcher dans ce sens.
            Vector3 lookDir = moveDir;

            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 10f
            );
        }

        // --- SOL / COYOTE / GRAVITÉ / SAUT ---

        bool isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;

            if (verticalVelocity < 0f)
                verticalVelocity = -2f;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpPressed && coyoteTimer > 0f)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            coyoteTimer      = 0f;

            if (animator != null)
                animator.SetTrigger("Jump");
        }

        verticalVelocity += gravity * Time.deltaTime;

        float currentSpeed = 0f;
        if (hasInput)
            currentSpeed = runKeyPressed ? runSpeed : walkSpeed;

        Vector3 horizontalVelocity = moveDir * currentSpeed;
        Vector3 finalMove          = horizontalVelocity;
        finalMove.y                = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);

        // Animator
        Vector3 planarVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        float speed            = planarVelocity.magnitude;

        if (animator != null)
        {
            animator.SetFloat("Speed", speed);

            bool isRunning = hasInput && runKeyPressed;
            animator.SetBool("IsRunning", isRunning);
        }
    }
}
