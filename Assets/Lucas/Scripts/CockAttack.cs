using UnityEngine;
using UnityEngine.InputSystem;

public class CockAttack : MonoBehaviour
{
       public float attackRange = 1.5f;  
    public LayerMask enemyLayer;
    public PlayerInput playerInput;

    public void AttackRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Attaque Droite");
            Attack(1);
        }
    }
    
    public void AttackLeft(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Attaque Gauche");
            Attack(-1);
        }
    }


    void Attack(int direction)
    {
        Vector2 origin = transform.position;
        Vector2 dir = new Vector2(direction, 0);

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, attackRange, enemyLayer);

        if (hit.collider != null)
        {
            Debug.Log("Ennemi touché : " + hit.collider.name);
            CockScore.AddScore();
            Destroy(hit.collider.gameObject);
        }
    }
}

