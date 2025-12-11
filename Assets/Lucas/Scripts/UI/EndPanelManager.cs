using UnityEngine;
using TMPro;

public class EndPanelManager : MonoBehaviour
{
    public TextMeshProUGUI gameScore;
    public TextMeshProUGUI highScore;
    public TextMeshProUGUI bestCombo;
    public TextMeshProUGUI mostKills;

    void Start()
    {
        gameScore.text = "Ton Toute ?! : " + CockScore.score;
        highScore.text = "Lo pli gro cok : " + CockStatsManager.GetHighScore();
        bestCombo.text = "Pli gro sac lo kou : " + CockStatsManager.GetBestCombo();
        mostKills.text = "Lo plis' cok dan' marmite : " + CockStatsManager.GetMostKills();
    }

}
