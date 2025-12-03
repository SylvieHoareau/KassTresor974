using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemyProgressUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform progressBar;
    public RectTransform attackZone;
    public GameObject enemyIconPrefab;

    [Header("Gameplay References")]
    public Transform player;
    public float maxDistance = 10f;

    private Dictionary<Transform, GameObject> enemyIcons = new Dictionary<Transform, GameObject>();

    void Update()
    {
        AutoCleanList();
        UpdateEnemyIcons();
    }

    // --- Ajout automatique d'un ennemi ---
    public void AddEnemy(Transform enemy)
    {
        if (enemyIcons.ContainsKey(enemy)) return;

        GameObject icon = Instantiate(enemyIconPrefab, progressBar);
        enemyIcons.Add(enemy, icon);
    }

    // --- Suppression automatisée ---
    public void RemoveEnemy(Transform enemy)
    {
        if (enemyIcons.ContainsKey(enemy))
        {
            Destroy(enemyIcons[enemy]);
            enemyIcons.Remove(enemy);
        }
    }

    // --- Retire automatiquement les ennemis détruits ---
    void AutoCleanList()
    {
        List<Transform> toRemove = new List<Transform>();

        foreach (var enemy in enemyIcons.Keys)
        {
            if (enemy == null)
                toRemove.Add(enemy);
        }

        foreach (var dead in toRemove)
        {
            RemoveEnemy(dead);
        }
    }

    void UpdateEnemyIcons()
    {
        foreach (var pair in enemyIcons)
        {
            Transform enemy = pair.Key;
            GameObject iconObj = pair.Value;

            if (enemy == null) continue;

            float distance = Mathf.Abs(enemy.position.x - player.position.x);
            float normalized = Mathf.Clamp01(distance / maxDistance);

            RectTransform iconRT = iconObj.GetComponent<RectTransform>();
            float x = Mathf.Lerp(-progressBar.rect.width / 2,
                                 progressBar.rect.width / 2,
                                 1 - normalized);

            iconRT.anchoredPosition = new Vector2(x, iconRT.anchoredPosition.y);
        }
    }
}
