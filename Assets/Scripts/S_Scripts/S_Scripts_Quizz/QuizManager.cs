using UnityEngine;
using UnityEngine.UI; // Pour Image, Button
using TMPro; // Pour TMP_Text (TextMeshPro)
using System.Collections; // Pour IEnumerator et coroutines

public class QuizManager : MonoBehaviour
{
    [Header("Data")]
    public QuestionData questionData;

    [Header("UI References")]
    public TMP_Text questionText;
    public Image questionImage;
    public Button[] answerButtons;
    public TMP_Text feedbackText;
    public TMP_Text scoreText;
    public TMP_Text timerText;

    [Header("Feedback")]
    public AudioSource audioSource;
    public AudioClip correctSFX;
    public AudioClip wrongSFX;
    public Color goodColor = Color.green;
    public Color badColor = Color.red;

    private int currentQuestionIndex = 0;
    private int score = 0;
    private float timer = 15f;
    private bool timerRunning = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateScoreUI();
        StartCoroutine(TimerRoutine());
        DisplayQuestion();
    }

    void DisplayQuestion()
    {
        timer = 15f;
        timerRunning = true;

        Question q = questionData.questions[currentQuestionIndex];

        // Texte de la question
        questionText.text = q.question;

        // Image (peut être null)
        if (q.questionImage != null) {
            questionImage.gameObject.SetActive(true);
            questionImage.sprite = q.questionImage;
        }
        else
        {
           questionImage.gameObject.SetActive(false);
        }

        // Réponses
        for (int i = 0; i < answerButtons.Length; i++)
        {
            TMP_Text btnText = answerButtons[i].GetComponentInChildren<TMP_Text>();
            btnText.text = q.replies[i];

            int buttonIndex = i; // Capture pour événement
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerClicked(buttonIndex));

            answerButtons[i].interactable = true;
        }

        feedbackText.text = "";
    }

    void OnAnswerClicked(int index)
    {

        timerRunning = false;

        Question q = questionData.questions[currentQuestionIndex];

        foreach (var btn in answerButtons)
        {
            btn.interactable = false;
        }

        if (index == q.correctOptionIndex)
        {
            score++;
            UpdateScoreUI();
            StartCoroutine(PlayFeedback("Bon réponse !", goodColor, correctSFX));
            // NextQuestion();
        }
        else
        {
            // TODO : Afficher un panneau de feedback
            StartCoroutine(PlayFeedback("Ayo... la pas sa ! Réessaye après !", badColor, wrongSFX));
        }
    }

    IEnumerator PlayFeedback(string message, Color color, AudioClip sfx)
    {
        // Audio
        audioSource.PlayOneShot(sfx);

        // Texte feedback
        feedbackText.text = message;
        feedbackText.color = color;

        // Mini amination scale
        Vector3 originalScale = feedbackText.transform.localScale;
        feedbackText.transform.localScale = Vector3.zero;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 3;
            feedbackText.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            yield return null;
        }

        yield return new WaitForSeconds(1.2f);

        NextQuestion();
    }

    void NextQuestion()
    {
        currentQuestionIndex++;

        if (currentQuestionIndex >= questionData.questions.Length)
        {
            Debug.Log("Quiz terminé !");
            // Afficher un écran de fin
            feedbackText.text = "Quiz terminé ! Score : " + score;
            return;
        }

        DisplayQuestion();
    }

    IEnumerator TimerRoutine()
    {
        while(true)
        {
            if (timerRunning)
            {
                timer -= Time.deltaTime;
                timerText.text = "Temps : " + Mathf.Ceil(timer);

                if (timer <= 0)
                {
                    timerRunning = false;
                    StartCoroutine(PlayFeedback("Temps écoulé !", badColor, wrongSFX));

                    foreach (var btn in answerButtons)
                    {
                        btn.interactable = false;
                    }
                }

                yield return null;
            }
        }
    }

    void UpdateScoreUI()
    {
        scoreText.text = "Score : " + score;
    }

}
