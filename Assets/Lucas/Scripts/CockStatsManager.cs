using UnityEngine;

public class CockStatsManager : MonoBehaviour
{
    // --- KEYS ---
    private const string HIGH_SCORE_KEY = "HighScore";
    private const string BEST_COMBO_KEY = "BestCombo";
    private const string MOST_KILLS_KEY = "MostKills";

    // --------------------------------------
    //              HIGH SCORE
    // --------------------------------------
    public static void SaveHighScore(int score)
    {
        int currentHigh = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);

        if (score > currentHigh)
        {
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, score);
            PlayerPrefs.Save();
        }
    }

    public static int GetHighScore()
    {
        return PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    // --------------------------------------
    //             BEST COMBO
    // --------------------------------------
    public static void SaveBestCombo(int combo)
    {
        int bestCombo = PlayerPrefs.GetInt(BEST_COMBO_KEY, 0);

        if (combo > bestCombo)
        {
            PlayerPrefs.SetInt(BEST_COMBO_KEY, combo);
            PlayerPrefs.Save();
        }
    }

    public static int GetBestCombo()
    {
        return PlayerPrefs.GetInt(BEST_COMBO_KEY, 0);
    }

    // --------------------------------------
    //              MOST KILLS
    // --------------------------------------
    public static void SaveMostKills(int kills)
    {
        int bestKills = PlayerPrefs.GetInt(MOST_KILLS_KEY, 0);

        if (kills > bestKills)
        {
            PlayerPrefs.SetInt(MOST_KILLS_KEY, kills);
            PlayerPrefs.Save();
        }
    }

    public static int GetMostKills()
    {
        return PlayerPrefs.GetInt(MOST_KILLS_KEY, 0);
    }

    // --------------------------------------
    //            RESET (optionnel)
    // --------------------------------------
    public static void ResetStats()
    {
        PlayerPrefs.DeleteKey(HIGH_SCORE_KEY);
        PlayerPrefs.DeleteKey(BEST_COMBO_KEY);
        PlayerPrefs.DeleteKey(MOST_KILLS_KEY);
    }
}