using UnityEngine;
using TMPro;

public class CockScore : MonoBehaviour
{
    public static int score = 0;
    public TextMeshProUGUI scoreText;
    public int currentScore;

    public static CockScore Instance;

    public static void AddScore(int amount = 100)
    {
        score += amount * CockComboSystem.Instance.GetMultiplier();
    }
    void FixedUpdate()
    {
        scoreText.text = "Score: " + score;
        currentScore = score;
    }
}
