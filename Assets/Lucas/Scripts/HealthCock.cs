using UnityEngine;
using UnityEngine.UI;

public class HealthCock : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public int minHealth = 0;
    public Slider healthBar;
    public bool isAlive = true;
    public GameObject endPanel;
    
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        healthBar.minValue = minHealth;
       
    }

    public void UpdateHealthBar()
    {
        healthBar.value = currentHealth;
    }

    public void UpdateDamage(int damage)
    {
        currentHealth += damage;
        UpdateHealthBar();
        if (currentHealth <= 0)
        {
            isAlive = false;
            endPanel.SetActive(true);
            Time.timeScale = 0.05f;

            GetComponent<DamageCockSystem>().enabled = false;
            GetComponent<CockAttack>().enabled = false;

            CameraShake.Instance.StopShake();
            CockGameManager. Instance.EndGame();

            Destroy(gameObject);
        }


    }
}
