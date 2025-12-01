using UnityEngine;

public class CockSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;

    public float baseSpawnInterval = 1.5f;
    public float minSpawnInterval = 0.25f;

    [Header("Difficulty Settings")]
    public float difficultyRamp = 0.01f;

    private float timer;

    void Update()
    {
        float score = CockScore.score;

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

        // 🔥 Object pooling ici !
        GameObject enemy = EnemyPool.Instance.GetEnemy();
        enemy.transform.position = spawnPoint.position;
        enemy.transform.rotation = Quaternion.identity;

        // Ajustement vitesse
        CockEnemyMove enemyScript = enemy.GetComponent<CockEnemyMove>();
        float baseSpeed = enemyScript.speed;
        enemyScript.speed = baseSpeed + score * 0.05f;

        // Flip si spawn à droite
        if (!spawnLeft)
        {
            Vector3 scale = enemy.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * -1f;
            enemy.transform.localScale = scale;
        }
        else
        {
            Vector3 scale = enemy.transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            enemy.transform.localScale = scale;
        }
    }
}