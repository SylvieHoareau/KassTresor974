using UnityEngine;

public class DamageCockSystem : MonoBehaviour
{
    [Header("Références")]
    public HealthCock health;
    public CockAttack attack;

    [Header("Paramètres des Dégâts")]
    public int damageTooClose = 20;
    public int damageWrongDirection = 15;
    public int damageAttackTooEarly = 10;

    [Header("Paramètres Détection Ennemi")]
    public float dangerRange = 0.8f;  // distance à laquelle l’ennemi blesse le joueur
    public LayerMask enemyLayer;

    void Update()
    {
        CheckIfEnemyTooClose();
    }

    // ------------------------------
    // DÉGÂTS SI ENNEMI TROP PROCHE
    // ------------------------------
    void CheckIfEnemyTooClose()
    {
        Collider2D closeEnemy = Physics2D.OverlapCircle(transform.position, dangerRange, enemyLayer);

        if (closeEnemy != null)
        {
            health.UpdateDamage(-damageTooClose);
            Destroy(closeEnemy.gameObject);
            Debug.Log("❌ Ennemi trop proche → dégâts !");
        }
    }

    // ------------------------------------
    // FONCTIONS APPELÉES PAR CockAttack.cs
    // ------------------------------------
    public void PlayerAttack(int direction)
    {
        Vector2 origin = transform.position;
        Vector2 dir = new Vector2(direction, 0);

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, attack.attackRange, enemyLayer);

        // AUCUN ennemi dans la direction → attaque trop tôt
        if (hit.collider == null)
        {
            health.UpdateDamage(-damageAttackTooEarly);
            Debug.Log("❌ Attaque trop tôt → dégâts !");
            return;
        }

        // Ennemi détecté mais vérifier s'il est au bon côté
        float enemyX = hit.collider.transform.position.x;
        float playerX = transform.position.x;

        bool enemyOnRight = enemyX > playerX;
        bool attackRight = direction == 1;

        if (enemyOnRight != attackRight)
        {
            health.UpdateDamage(-damageWrongDirection);
            Debug.Log("❌ Mauvaise direction → dégâts !");
            return;
        }

        // Sinon → attaque correcte, tuer l’ennemi
        Debug.Log("✔ Ennemi touché : " + hit.collider.name);
        CockScore.AddScore();
        Destroy(hit.collider.gameObject);
    }

    // Gizmo pour voir la zone dangereuse
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dangerRange);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector3 rightDir = transform.position + Vector3.right * attack.attackRange;
        Vector3 leftDir = transform.position + Vector3.left * attack.attackRange;
        Gizmos.DrawLine(transform.position, rightDir);
        Gizmos.DrawLine(transform.position, leftDir);
    }
}
