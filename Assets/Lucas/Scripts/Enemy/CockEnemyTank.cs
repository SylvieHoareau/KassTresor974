using UnityEngine;

public class CockEnemyTank : MonoBehaviour
{
    [Header("Tank Settings")]
    public int maxHits = 2;   // Tank = 2 coups
    private int currentHits;

    private CockEnemyMove moveScript;

    void OnEnable()
    {
        currentHits = maxHits;
        moveScript = GetComponent<CockEnemyMove>();

        // Optionnel : rendre le Tank un peu plus lent
        moveScript.speed *= 0.8f; 
    }

    public bool TakeHit()
    {
        currentHits--;

        if (currentHits <= 0)
        {
            return true; // Ennemi mort
        }

        return false; // Survivant → encore 1 coup
    }
}