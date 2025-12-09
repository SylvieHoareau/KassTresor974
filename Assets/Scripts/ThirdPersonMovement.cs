using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Déplacement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;

    [Header("Saut & Gravité")]
    public float jumpHeight = 2f;
    public float gravity = -20f;     // un peu plus fort pour un saut plus nerveux
    public float coyoteTime = 0.15f; // temps toléré pour pouvoir sauter après avoir quitté le sol

    [Header("Références")]
    public Transform cameraTransform;
    public Animator animator;

    private CharacterController controller;
    private float verticalVelocity;  // vitesse verticale pour le saut
    private float coyoteTimer;       // timer pour le "coyote time"

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        // === INPUT CLAVIER (ZQSD / WASD) ===
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A))
            horizontal -= 1f;
        if (Input.GetKey(KeyCode.D))
            horizontal += 1f;
        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.W))
            vertical += 1f;
        if (Input.GetKey(KeyCode.S))
            vertical -= 1f;

        bool runKeyPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jumpPressed   = Input.GetKeyDown(KeyCode.Space);

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;
        bool hasInput = inputDir.sqrMagnitude > 0.01f;

        Vector3 moveDir = Vector3.zero;

        if (hasInput)
        {
            // Direction relative à la caméra
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            moveDir = camForward * inputDir.z + camRight * inputDir.x;
            moveDir.Normalize();

            // Rotation vers la direction de déplacement
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 10f
            );
        }

        // --- GESTION SOL / COYOTE TIME / GRAVITÉ / SAUT ---

        bool isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            // tant qu'on touche le sol, on reset le coyote timer
            coyoteTimer = coyoteTime;

            if (verticalVelocity < 0f)
            {
                // petit offset pour rester bien plaqué au sol
                verticalVelocity = -2f;
            }
        }
        else
        {
            // en l'air, on décrémente le coyote timer
            coyoteTimer -= Time.deltaTime;
        }

        // Condition de saut : on autorise tant que le coyoteTimer > 0
        if (jumpPressed && coyoteTimer > 0f)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            coyoteTimer = 0f; // on consomme le coyote time

            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }

        // Gravité appliquée en continu
        verticalVelocity += gravity * Time.deltaTime;

        // Vitesse horizontale (walk / run)
        float currentSpeed = 0f;
        if (hasInput)
        {
            currentSpeed = runKeyPressed ? runSpeed : walkSpeed;
        }

        Vector3 horizontalVelocity = moveDir * currentSpeed;

        // Mouvement final (horizontal + vertical)
        Vector3 finalMove = horizontalVelocity;
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);

        // Vitesse plane pour l'Animator
        Vector3 planarVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        float speed = planarVelocity.magnitude;

        if (animator != null)
        {
            animator.SetFloat("Speed", speed);

            // ⚠️ Retour au comportement qui marchait bien avant :
            // Run = il y a un input + Shift enfoncé (on ne rajoute pas isGrounded ici)
            bool isRunning = hasInput && runKeyPressed;
            animator.SetBool("IsRunning", isRunning);
        }
    }
}
