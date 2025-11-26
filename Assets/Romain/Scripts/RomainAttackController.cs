using UnityEngine;
using UnityEngine.InputSystem;

public class RomainAttackController : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Animator animator;

    [Header("Cooldowns")]
    [SerializeField] private float punchCooldown = 0.5f;
    [SerializeField] private float kickCooldown = 0.8f;

    private InputSystem_Actions inputActions;

    private bool canPunch = true;
    private bool canKick = true;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Attack.performed += OnAttack;
    }

    private void OnDisable()
    {
        inputActions.Player.Attack.performed -= OnAttack;
        inputActions.Disable();
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (Mouse.current == null) return;

        // Clic gauche = Punch
        if (Mouse.current.leftButton.isPressed && canPunch)
        {
            animator.Play("Attack_Punch");
            canPunch = false;
            Invoke(nameof(ResetPunch), punchCooldown);
        }

        // Clic droit = Kick
        if (Mouse.current.rightButton.isPressed && canKick)
        {
            animator.Play("Attack_Kick");
            canKick = false;
            Invoke(nameof(ResetKick), kickCooldown);
        }
    }

    private void ResetPunch()
    {
        canPunch = true;
    }

    private void ResetKick()
    {
        canKick = true;
    }
}
