using UnityEngine;
using UnityEngine.UI; // Pour Image, Button
using TMPro; // Pour TMP_Text (TextMeshPro)
using System.Collections;
using DG.Tweening;
public class QuizManager : MonoBehaviour
{
    [Header("Data")]
    public QuestionData questionData;

    [Header("UI References")]
    public TMP_Text questionText;
    public Image questionImage;
    public Button[] answerButtons;
    public TMP_Text feedbackText; // Message dans le endGamePanel
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public GameObject goodFeedbackPanel; // Petit panel de feedback
    public GameObject badFeedbackPanel; // Petit panel de feedback
    public GameObject endGamePanel; // Le panneau de fin de quiz


    [Header("Feedback Assets")]
    public AudioSource audioSource;
    public AudioClip correctSFX;
    public AudioClip wrongSFX;
    public Color goodColor = Color.green;
    public Color badColor = Color.red;

    [Header("Settings")]
    public float timePerQuestion = 15f;
    public Item recompenseQuiz; // L'objet à ajouter à l'inventaire en récompense
    public int scoreMinimumRequis = 3; // Score minimum pour obtenir la récompense

    [Header("Victory Effect")]
    [SerializeField] private VictoryEffect victoryEffect;

    private int currentQuestionIndex = 0;
    private int score = 0;
    private float timer = 15f;
    private bool timerRunning = true;
    private bool quizFinished = false;
    private Coroutine feedbackCoroutine;
    private Coroutine timerRunCoroutine;

    
    public Button replayButton;
    public static event System.Action<int> OnQuizFinished;


    // Start est appelée une seule fois
    void Start()
    {
        // Au début du jeu, on s'assure que le panneau est MASQUE au début du jeu
        if (goodFeedbackPanel != null) goodFeedbackPanel.SetActive(false);
        if (badFeedbackPanel != null) badFeedbackPanel.SetActive(false);

        if (endGamePanel != null) endGamePanel.SetActive(false);

        // Les écouteurs de boutons sont attachés UNE SEULE FOIS
        SetupAnswerButtons();

        // UpdateScoreUI();
        // timerRunCoroutine = StartCoroutine(TimerCoroutine());
        // DisplayQuestion();
        StartQuiz();
    }

    void StartQuiz()
    {
        currentQuestionIndex = 0;
        score = 0;
        quizFinished = false;

        UpdateScoreUI();

        // On s'assure que les éléments du quiz sont visibles
        questionText.gameObject.SetActive(true);
        foreach (var btn in answerButtons) btn.gameObject.SetActive(true);

        if (timerRunCoroutine != null) StopCoroutine(timerRunCoroutine);
        timerRunCoroutine = StartCoroutine(TimerCoroutine());
        DisplayQuestion();
    }

    // Méthode pour configurer les événements onClick des boutons
    void SetupAnswerButtons()
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int buttonIndex = i; // Capture pour événement
            // On s'assure qu'il n'y a pas d'écouteur existant avant d'ajouter
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerClicked(buttonIndex));
        }
    }

    void DisplayQuestion()
    {
        // Vérification de sécurité au cas où questionData serait null ou vide
       if (questionData == null || questionData.questions.Length == 0) return;

        Question q = questionData.questions[currentQuestionIndex];
        // Texte de la question
        questionText.text = q.question;

        // Image (peut être null)
        if (q.questionImage != null) 
        {
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
            answerButtons[i].interactable = true; // Rendre cliquable   
        }

        // Reset timer pour cette question
        timer = timePerQuestion;
        timerRunning = true;
    }

    void OnAnswerClicked(int index)
    {
        if (!timerRunning || quizFinished) return;
        // Arrêter le chronomètre immédiatement
        timerRunning = false;

        // Récupérer la question actuelle
        Question q = questionData.questions[currentQuestionIndex];
        // Récupérer le composant AwnserButtonAnimator du bouton cliqué
        AnswerButtonAnimator animator = answerButtons[index].GetComponent<AnswerButtonAnimator>();

        if (index == q.correctOptionIndex)
        {
            score++;
            UpdateScoreUI();

            if (animator != null) animator.AnimateCorrectAnswer();
            feedbackCoroutine = StartCoroutine(PlayFeedback("Bonne réponse !", goodColor, correctSFX, true));
        } 
        else
        {
            // Appel DOTWEEN pour l'animation
            if (animator != null)
            {
                animator.AnimateIncorrectAnswer();
            }

            // Afficher également la bonne réponse
            Button correctButton = answerButtons[q.correctOptionIndex];
            if (animator != null) animator.AnimateIncorrectAnswer();
            AnswerButtonAnimator correctAnimator = correctButton.GetComponent<AnswerButtonAnimator>();
            if (correctAnimator != null)
            {
                // Anime la bonne réponse pour la montrer (une version plus légère que la victoire)
                // Vous devrez ajouter une méthode 'AnimateHint' dans AnswerButtonAnimator.
                // Par exemple : correctAnimator.AnimateHint(); 
                correctAnimator.AnimateHint();
            }

            feedbackCoroutine = StartCoroutine(PlayFeedback("Ayo... la pas sa ! Réessaye après !", badColor, wrongSFX, false));
        }
    }

    IEnumerator PlayFeedback(string message, Color color, AudioClip sfx, bool isCorrect)
    {
        // Audio
        audioSource.PlayOneShot(sfx);

        // Texte feedback avec animation
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackText.transform.localScale = Vector3.zero;
        feedbackText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);

        // Gestion du panneau de feedback
        GameObject panelToUse = isCorrect ? goodFeedbackPanel : badFeedbackPanel;

        // Feedback visuel rapide
        if (panelToUse != null)
        {
            panelToUse.SetActive(true);
            CanvasGroup cg = panelToUse.GetComponent<CanvasGroup>();
            if (cg == null) cg = panelToUse.AddComponent<CanvasGroup>();
            cg.alpha = 0;
            cg.DOFade(1f, 0.2f);
        }

        // Délai
        yield return new WaitForSeconds(1.2f);

        // On ne masque le panneau QUE si ce n'est pas la dernière question
        if (currentQuestionIndex < questionData.questions.Length - 1)
        {
             // Masquer le panneau si nécessaire après le délai
            if (panelToUse != null)
            {
                panelToUse.GetComponent<CanvasGroup>()?.DOFade(0f, 0.2f)
                    .OnComplete(() => panelToUse.SetActive(false));
            }
        }
        else
        {
            // Sécurité : masquer les deux si on arrive au dernier index
            if (goodFeedbackPanel != null) goodFeedbackPanel.SetActive(false);
            if (badFeedbackPanel != null) badFeedbackPanel.SetActive(false);
        }

        // Passer à la question suivante
        NextQuestion();
    }

    void NextQuestion()
    {
        currentQuestionIndex++;

        if (currentQuestionIndex >= questionData.questions.Length)
        {
            FinishQuiz();
        }
        else
        {
            DisplayQuestion();
        }

    }

    // Méthode pour gérer la fin du quizz 
    void FinishQuiz()
    {
        // Arrêter la logique du quiz
        timerRunning = false;
        quizFinished = true;
        timerText.text = "";

        // On cache le HUD de jeu
        questionText.gameObject.SetActive(false);
        questionImage.gameObject.SetActive(false);
        foreach (var btn in answerButtons) btn.gameObject.SetActive(false);

        // --- ACTIVATION DU PANNEAU FINAL ---
        if (endGamePanel != null)
        {
            endGamePanel.SetActive(true);
            // On peut aussi ajouter un petit effet de fade avec DOTween si tu as un CanvasGroup
            CanvasGroup cg = endGamePanel.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = 0; cg.DOFade(1f, 0.5f); }
        }

        // Logique de score et récompense
        int total = questionData.questions.Length;
        if (score >= scoreMinimumRequis)
        {
            if (recompenseQuiz != null) InventoryManager.Instance?.AjouterObjet(recompenseQuiz);
            feedbackText.text = $"Bravo ! {score}/{total}\nTu as gagné : {recompenseQuiz?.nom}";
            NotificationManager.Instance?.AfficherNotification("Récompense obtenue !");
        }
        else
        {
            feedbackText.text = $"Terminé ! {score}/{total}\nRéessaye pour la récompense !";
        }

       // Victoire parfaite
        if (score == total && victoryEffect != null) victoryEffect.PlayVictory();
        // ---------------------------------------------------

        OnQuizFinished?.Invoke(score);
    }

    public void RejouerQuiz()
    {
        // On cache les panneaux avant de relancer
        if (endGamePanel != null) endGamePanel.SetActive(false);
        if (goodFeedbackPanel != null) goodFeedbackPanel.SetActive(false);
        if (badFeedbackPanel != null) badFeedbackPanel.SetActive(false);

        StartQuiz();
    }

    IEnumerator TimerCoroutine()
    {
        while (!quizFinished)
        {
            if (timerRunning)
            {
                timer -= Time.deltaTime;
                timerText.text = "Temps : " + Mathf.CeilToInt(timer).ToString();

                if (timer <= 0)
                {
                    timerRunning = false;
                    foreach (var btn in answerButtons) btn.interactable = false;
                    feedbackCoroutine = StartCoroutine(PlayFeedback("Temps écoulé !", badColor, wrongSFX, false));
                }
            }
            yield return null;
        }
    }

    void UpdateScoreUI()
    {
        scoreText.text = "Score : " + score;
    }

}
