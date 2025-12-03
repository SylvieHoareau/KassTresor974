using UnityEngine;
using System.Collections.Generic;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance;

    [Header("Pool Settings")]
    public GameObject enemyPrefab;
    public int initialPoolSize = 30;


    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;

        // Pré-creation des ennemis
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);
            enemy.SetActive(false);
            pool.Enqueue(enemy);
        }
    }

    public GameObject GetEnemy()
    {
        if (pool.Count > 0)
        {
            GameObject e = pool.Dequeue();
            e.SetActive(true);
            return e;
        }

        // Si le pool est vide → on crée un nouvel ennemi (optionnel)
        GameObject extra = Instantiate(enemyPrefab);
        return extra;
    }

    public void ReturnEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
        pool.Enqueue(enemy);
    }
}
