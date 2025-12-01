using UnityEngine;

public class DamageCockSystem : MonoBehaviour
{
    [Header("Références")]
    public HealthCock health;
    public CockAttack attack;
    public EnemyProgressUI progressUI;

    [Header("Paramètres des Dégâts")]
    public int damageTooClose = 20;
    public int damageWrongDirection = 15;
    public int damageAttackTooEarly = 10;

    [Header("Paramètres Détection Ennemi")]
    public float dangerRange = 0.8f;
    public LayerMask enemyLayer;

    void Update()
    {
        CheckIfEnemyTooClose();
    }

    void CheckIfEnemyTooClose()
    {
        Collider2D closeEnemy = Physics2D.OverlapCircle(transform.position, dangerRange, enemyLayer);

        if (closeEnemy != null)
        {
            health.UpdateDamage(-damageTooClose);

            // ---- AJOUT UI ----
            progressUI.RemoveEnemy(closeEnemy.transform);

            Destroy(closeEnemy.gameObject);
            Debug.Log("❌ Ennemi trop proche → dégâts !");
        }
    }

    public void PlayerAttack(int direction)
    {
        Vector2 origin = transform.position;
        Vector2 dir = new Vector2(direction, 0);

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, attack.attackRange, enemyLayer);

        if (hit.collider == null)
        {
            health.UpdateDamage(-damageAttackTooEarly);
            Debug.Log("❌ Attaque trop tôt → dégâts !");
            return;
        }

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

        Debug.Log("✔ Ennemi touché : " + hit.collider.name);
        CockScore.AddScore();

        Destroy(hit.collider.gameObject);
    }
}
