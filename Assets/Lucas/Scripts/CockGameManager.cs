using UnityEngine;
using UnityEngine.SceneManagement;

public class CockGameManager : MonoBehaviour
{
    public static CockGameManager Instance;
    public string sceneName;

    void Awake()
    {
        Instance = this;
    }

    public void EndGame()
    {
        Debug.Log("END GAME !!");

        // 1️⃣ Récupérer les stats de la partie
        int finalScore = CockScore.score;
        //int finalCombo = CockComboSystem.Instance.greatCombo;
        int finalKills = EnemyKillCounter.currentSessionKills;

        // 2️⃣ Les sauvegarder
        CockStatsManager.SaveHighScore(finalScore);
        //CockStatsManager.SaveBestCombo(finalCombo);
        CockStatsManager.SaveMostKills(finalKills);

        Debug.Log($"Saved stats → score={finalScore} kills={finalKills}");
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}