using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class RomainPlayerController : MonoBehaviour
{
    [Header("Déplacement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Saut")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool jumpPressed;
    private bool canMove = true;

    private InputSystem_Actions input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.freezeRotation = true;

        input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        input.Enable();

        input.Player.Move.performed += ctx =>
        {
            Vector2 v = ctx.ReadValue<Vector2>();
            moveInput = new Vector3(v.x, 0f, v.y);
        };

        input.Player.Move.canceled += ctx =>
        {
            moveInput = Vector3.zero;
        };

        input.Player.Jump.performed += ctx =>
        {
            jumpPressed = true;
        };
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void Update()
    {
        if (!canMove) return;

        if (jumpPressed && IsGrounded())
        {
            Jump();
        }

        jumpPressed = false;
    }

    private void FixedUpdate()
{
    if (!canMove)
    {
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        return;
    }

    Vector3 velocity = rb.linearVelocity;
    Vector3 horizontal = moveInput.normalized * moveSpeed;

    rb.linearVelocity = new Vector3(
        horizontal.x,
        velocity.y,
        horizontal.z
    );

    // >>> ROTATION automatique selon le mouvement <<<
    if (moveInput.sqrMagnitude > 0.01f)
    {
        Quaternion targetRot = Quaternion.LookRotation(moveInput, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 0.15f);
    }
}


    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!value)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    public void TeleportTo(Vector3 pos)
    {
        rb.linearVelocity = Vector3.zero;
        transform.position = pos;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}
