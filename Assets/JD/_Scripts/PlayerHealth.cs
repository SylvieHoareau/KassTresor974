using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    void Start()
    {
        currentHealth = maxHealth; // On commence full vie
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Aïe ! PV restants : " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("Le joueur est mort.");
        // On prévient le chef que c'est fini
        GameManager.Instance.TriggerGameOver();
        
        // Optionnel : On cache le joueur pour faire croire qu'il a disparu
        gameObject.SetActive(false); 
    }
}