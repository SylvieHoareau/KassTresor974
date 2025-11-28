using UnityEngine;

public class CockSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;

    public float baseSpawnInterval = 1.5f;  // temps de spawn au début
    public float minSpawnInterval = 0.25f;  // limite de vitesse max

    [Header("Difficulty Settings")]
    public float difficultyRamp = 0.01f;  // vitesse de montée en difficulté

    private float timer;

    void Update()
    {
        float score = CockScore.score;

        // calcul dynamique du temps entre spawns
        float currentSpawnInterval = Mathf.Max(
            baseSpawnInterval - score * difficultyRamp,
            minSpawnInterval
        );

        timer += Time.deltaTime;

        if (timer >= currentSpawnInterval)
        {
            SpawnEnemy(score);
            timer = 0f;
        }
    }

    void SpawnEnemy(float score)
    {
        bool spawnLeft = Random.value < 0.5f;
        Transform spawnPoint = spawnLeft ? leftSpawnPoint : rightSpawnPoint;

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        // Ajuste la vitesse de l’ennemi selon le score
        CockEnemyMove enemyScript = enemy.GetComponent<CockEnemyMove>();

        float baseSpeed = enemyScript.speed;
        enemyScript.speed = baseSpeed + score * 0.05f; // vitesse dynamique

        // Inversion du scale si l’ennemi vient de la droite
        if (!spawnLeft)
        {
            Vector3 scale = enemy.transform.localScale;
            scale.x *= -1;
            enemy.transform.localScale = scale;
        }
    }
}


