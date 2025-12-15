using UnityEngine;
using TMPro;

public class EndGamePanel : MonoBehaviour
{
    public TMP_Text finalScoreText;

    void OnEnable() 
    {
        // ABONNEMENT à l'événement du QuizManager
        QuizManager.OnQuizFinished += ShowPanel;
    }

    void OnDisable() 
    {
        // DÉSABONNEMENT pour éviter les fuites de mémoire !
        QuizManager.OnQuizFinished -= ShowPanel;
    }

    void ShowPanel(int finalScore) 
    {
        gameObject.SetActive(true);
        finalScoreText.text = $"Bravo ! Score final : {finalScore}!";
    }
}
