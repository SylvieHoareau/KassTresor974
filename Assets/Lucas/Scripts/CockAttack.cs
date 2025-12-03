using UnityEngine;
using UnityEngine.InputSystem;

public class CockAttack : MonoBehaviour
{
    public float attackRange = 1.5f;  
    public PlayerInput playerInput;

    private DamageCockSystem damageSystem;

    void Start()
    {
        damageSystem = GetComponent<DamageCockSystem>();
    }

    public void AttackRight(InputAction.CallbackContext context)
    {
        if (context.performed)
            damageSystem.PlayerAttack(1);
    }

    public void AttackLeft(InputAction.CallbackContext context)
    {
        if (context.performed)
            damageSystem.PlayerAttack(-1);
    }
}

