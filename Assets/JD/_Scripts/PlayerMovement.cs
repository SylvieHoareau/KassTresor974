using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float rotationSpeed = 720f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private Vector2 moveInput;

    // On garde notre fichier de secours en mémoire
    private InputSystem_Actions inputsDeSecours; 

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // ---------------------------------------------------------
        // LA LOGIQUE SÉPARÉE (Pour éviter les conflits)
        // ---------------------------------------------------------

        if (GameManager.Instance != null)
        {
            // === SCÉNARIO 1 : JEU COMPLET (Avec les collègues) ===
            // On utilise LEUR système (GameControls et "Gameplay")
            
            var inputsPro = GameManager.Instance.inputs; // Unity devine que c'est GameControls
            
            // On se branche sur LEURS noms (Gameplay)
            inputsPro.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            inputsPro.Gameplay.Move.canceled += ctx => moveInput = Vector2.zero;
            inputsPro.Gameplay.Jump.performed += ctx => Jump();
        }
        else
        {
            // === SCÉNARIO 2 : SOLO (Export de scène) ===
            // On utilise TON système (InputSystem_Actions et "Player")
            
            Debug.LogWarning("⚠️ Mode 'Autonome' : Utilisation des inputs de secours (InputSystem_Actions).");
            
            inputsDeSecours = new InputSystem_Actions();
            inputsDeSecours.Enable(); 

            // On se branche sur TES noms par défaut (Player)
            inputsDeSecours.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            inputsDeSecours.Player.Move.canceled += ctx => moveInput = Vector2.zero;
            inputsDeSecours.Player.Jump.performed += ctx => Jump();
        }
    }

    private void OnDisable()
    {
        // On nettoie seulement si on a utilisé le secours
        if (inputsDeSecours != null)
        {
            inputsDeSecours.Disable();
        }
    }

    void Update()
    {
        // Le reste ne change pas, c'est de la pure logique de mouvement
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        
        if (move != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        controller.Move(move * speed * Time.deltaTime);

        if (animator != null)
        {
            animator.SetFloat("Speed", move.magnitude);
            animator.SetBool("IsGrounded", controller.isGrounded);
        }

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Jump()
    {
        if (controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator != null) animator.SetTrigger("Jump");
        }
    }
}