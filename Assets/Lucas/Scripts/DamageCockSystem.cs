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
    public int comboHeal = 3;

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
            CockComboSystem.Instance.ResetCombo();

            CameraShake.Instance.Shake(0.15f, 0.25f);
            ImpactFlash.Instance.FlashRed();


             EnemyPool.Instance.ReturnEnemy(closeEnemy.gameObject);
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
            CockComboSystem.Instance.ResetCombo();
            ImpactFlash.Instance.FlashRed();
            CameraShake.Instance.Shake(0.07f, 0.12f);
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
            CockComboSystem.Instance.ResetCombo();
            ImpactFlash.Instance.FlashRed();
            CameraShake.Instance.Shake(0.12f, 0.2f);
            Debug.Log("❌ Mauvaise direction → dégâts !");
            return;
        }

        Debug.Log("✔ Ennemi touché : " + hit.collider.name);
        CockComboSystem.Instance.AddCombo();
        CockScore.AddScore();
        EnemyKillCounter.AddKill();

        CameraShake.Instance.Shake(0.08f, 0.15f);
        ImpactFlash.Instance.FlashWhite();
        HitSlowMotion.Instance.DoSlowMotion();

        EnemyPool.Instance.ReturnEnemy(hit.collider.gameObject);
        
        if (health.currentHealth < 100)
        {
            health.UpdateDamage(comboHeal);
        }

    

       
    }
     
}
