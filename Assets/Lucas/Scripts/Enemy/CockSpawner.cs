using UnityEngine;

public class CockSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject enemyPrefabNormal;
    public GameObject enemyPrefabTank;

    [Header("Spawn Points")]
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;

    [Header("Spawn Timing")]
    public float baseSpawnInterval = 1.5f;
    public float minSpawnInterval = 0.25f;

    [Header("Difficulty Over Time")]
    public float difficultyMin = 0f;       // difficulté à t=0
    public float difficultyMax = 1f;       // difficulté max
    public float difficultyRampSpeed = 0.02f;  // vitesse d’augmentation de la difficulté

    private float timer;
    private float elapsedTime;

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // 🔥 Calcul de la difficulté (0 → 1)
        float difficulty = Mathf.Clamp01(
            difficultyMin + elapsedTime * difficultyRampSpeed
        );

        difficulty = Mathf.Clamp(difficulty, difficultyMin, difficultyMax);

        // Le spawn se raccourcit en fonction de la difficulté
        float currentSpawnInterval = Mathf.Lerp(baseSpawnInterval, minSpawnInterval, difficulty);

        timer += Time.deltaTime;

        if (timer >= currentSpawnInterval)
        {
            SpawnEnemy(difficulty);
            timer = 0f;
        }
    }

    void SpawnEnemy(float difficulty)
    {
        bool spawnLeft = Random.value < 0.5f;
        Transform spawnPoint = spawnLeft ? leftSpawnPoint : rightSpawnPoint;

        GameObject enemy = EnemyPool.Instance.GetEnemy();
        enemy.transform.position = spawnPoint.position;
        enemy.transform.rotation = Quaternion.identity;

        // 🔥 Augmentation de la vitesse des ennemis selon la difficulté
        CockEnemyMove move = enemy.GetComponent<CockEnemyMove>();
        float baseSpeed = move.speed;

        move.speed = Mathf.Lerp(baseSpeed, 9f, difficulty);

        // Flip si spawn à droite
        Vector3 scale = enemy.transform.localScale;
        scale.x = spawnLeft ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        enemy.transform.localScale = scale;
    }
}