using UnityEngine;
using TMPro;

public class CockScore : MonoBehaviour
{
    public static int score = 0;
    public TextMeshProUGUI scoreText;

    public static void AddScore(int amount = 100)
    {
        score += amount * CockComboSystem.Instance.GetMultiplier();
    }
    void Update()
    {
        scoreText.text = "Score: " + score;
    }
}
