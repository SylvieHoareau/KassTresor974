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
    public TMP_Text feedbackText;
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public GameObject goodFeedbackPanel;

    [Header("Feedback")]
    public AudioSource audioSource;
    public AudioClip correctSFX;
    public AudioClip wrongSFX;
    public Color goodColor = Color.green;
    public Color badColor = Color.red;

    [Header("Settings")]
    public float timePerQuestion = 15f;

    private int currentQuestionIndex = 0;
    private int score = 0;
    private float timer = 15f;
    private bool timerRunning = true;
    private Coroutine feedbackCoroutine;
    private Coroutine timerRunCoroutine;

    public static event System.Action<int> OnQuizFinished;

    [Header("Victory Effect")]
    [SerializeField] private VictoryEffect victoryEffect;

    [Header("Récompense")]
    public Item recompenseQuiz; // L'objet à ajouter à l'inventaire en récompense
    public int scoreMinimumRequis = 3; // Score minimum pour obtenir la récompense

    private bool quizFinished = false;

    public Button replayButton;
    // Start est appelée une seule fois
    void Start()
    {
        // Au début du jeu, on s'assure que le panneau est MASQUE au début du jeu
        if (goodFeedbackPanel != null)
        {
            goodFeedbackPanel.SetActive(false);
        }
        // Les écouteurs de boutons sont attachés UNE SEULE FOIS
        SetupAnswerButtons();

        UpdateScoreUI();
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
        if (questionData == null || questionData.questions == null || questionData.questions.Length == 0)
        {
            Debug.LogError("Question Data est manquant ou vide !");
            return;
        }

        // On vérifie la fin du quiz au cas où cet appel serait mal placé
        if (currentQuestionIndex >= questionData.questions.Length)
        {
            FinishQuiz();
            return;
        }

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

        feedbackText.text = "";
        feedbackText.transform.localScale = Vector3.one;

        // Reset timer pour cette question
        timer = timePerQuestion;
        timerRunning = true;
    }

    void OnAnswerClicked(int index)
    {
        if (!timerRunning) return;

        // Arrêter le chronomètre immédiatement
        timerRunning = false;

        // Récupérer le composant AwnserButtonAnimator du bouton cliqué
        AnswerButtonAnimator clickedButtonAnimator = answerButtons[index].GetComponent<AnswerButtonAnimator>();

        foreach (var btn in answerButtons)
        {
            btn.interactable = false;
        }

        // Arrete le feedback précédent
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);

        // Récupérer la question actuelle
        Question q = questionData.questions[currentQuestionIndex];

        if (index == q.correctOptionIndex)
        {
            score++;
            UpdateScoreUI();

            // Appel DOTween pour l'animation locale du bouton
            if (clickedButtonAnimator != null)
            {
                clickedButtonAnimator.AnimateCorrectAnswer();
            }

            // On démarre la coroutine de feedback
            feedbackCoroutine = StartCoroutine(PlayFeedback("Bonne réponse !", goodColor, correctSFX, true));
        }
        else
        {
            // Appel DOTWEEN pour l'animation
            if (clickedButtonAnimator != null)
            {
                clickedButtonAnimator.AnimateIncorrectAnswer();
            }

            // Afficher également la bonne réponse
            Button correctButton = answerButtons[q.correctOptionIndex];
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

        // Texte feedback
        feedbackText.text = message;
        feedbackText.color = color;

        // Mini amination scale
        Vector3 originalScale = feedbackText.transform.localScale;
        feedbackText.transform.localScale = Vector3.zero;

        // Tuer tout tween en cours sur le texte pour éviter les interférences.
        DOTween.Kill(feedbackText.transform);

        // Création de la séquence de Tween (Animation Pop)
        feedbackText.transform.DOScale(1.1f, 0.2f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                feedbackText.transform.DOScale(1f, 0.1f);
            });

        // Animation spécifique pour le panneau de bon feedback
        if (isCorrect && goodFeedbackPanel != null)
        {
            // On rend le panneau visible
            goodFeedbackPanel.SetActive(true);
            CanvasGroup cg = goodFeedbackPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = goodFeedbackPanel.AddComponent<CanvasGroup>();
            
            cg.alpha = 0f;
            cg.DOFade(1f, 0.3f);
        }

        // float t = 0f;
        // while (t < 1f)
        // {
        //     t += Time.deltaTime * 3;
        //     feedbackText.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
        //     yield return null;
        // }

        // Délai
        yield return new WaitForSeconds(1.2f);

        // On ne masque le panneau QUE si ce n'est pas la dernière question
        if (currentQuestionIndex < questionData.questions.Length - 1)
        {
             // Masquer le panneau si nécessaire après le délai
            if (goodFeedbackPanel != null)
            {
                // On peut utiliser un DOFade Out si on veut
                CanvasGroup cg = goodFeedbackPanel.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.DOFade(0f, 0.2f).OnComplete(() => goodFeedbackPanel.SetActive(false));
                }
                else
                {
                    goodFeedbackPanel.SetActive(false);
                }
            }
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
            return;
        }

        DisplayQuestion();
    }

    // Méthode pour gérer la fin du quizz 
    void FinishQuiz()
    {
        // Arrêter la logique du quiz
        timerRunning = false;

        quizFinished = true;

        // Arrêter explicitement la coroutine
        if (timerRunCoroutine != null)
        {
            StopCoroutine(timerRunCoroutine);
            timerRunCoroutine = null;
        }

        // Afficher le panneau de récompense 
        if (goodFeedbackPanel != null)
        {
            goodFeedbackPanel.SetActive(true);
            CanvasGroup cg = goodFeedbackPanel.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f; // On force la visibilité
        }

        // On calcule le nombre total de questions
        int nombreTotalQuestions = questionData.questions.Length;

        // --- LOGIQUE D'INVENTAIRE ------------------
        if (score >= scoreMinimumRequis && recompenseQuiz != null)
        {
            // On demande à l'InventoryManager d'ajouter l'objet
            InventoryManager.Instance.AjouterObjet(recompenseQuiz);
            feedbackText.text = $"Bravo ! Tu as gagné : {recompenseQuiz.nom}";

            // APPEL DE LA NOTIFICATION ICI
            NotificationManager.Instance.AfficherNotification($"Nouvel objet : {recompenseQuiz.nom} !");
        }
        else
        {
            // Mise à jour de l'UI
            feedbackText.text = $"Quiz terminé ! Score : {score} / {nombreTotalQuestions}. Réessaie pour gagner la récompense !";
        }
        // --------------------------------------------

        timerText.text = "";

        // Désactiver les éléments du jeu (questions, boutons, etc.)
        questionText.gameObject.SetActive(false);
        questionImage.gameObject.SetActive(false);

        foreach (var btn in answerButtons)
        {
            btn.gameObject.SetActive(false);
        }

        replayButton.gameObject.SetActive(true);

        // Déclenche l'évenement pour les abonnées, en passant le score
        OnQuizFinished?.Invoke(score);

        // --- NOUVELLE LOGIQUE : SCORE PARFAIT UNIQUEMENT ---
        // On vérifie si le score est égal au nombre total de questions.
        // On ajoute aussi une sécurité (> 0) pour éviter de lancer l'effet si le quiz était vide.
        if (score == nombreTotalQuestions && nombreTotalQuestions > 0)
        {
            Debug.Log("Score parfait ! Lancement de l'effet de victoire.");
            if (victoryEffect != null)
            {
                victoryEffect.PlayVictory();
                // Optionnel : Tu peux ajouter un message spécial pour le score parfait
                // feedbackText.text += "\nSCORE PARFAIT ! INCROYABLE !";
            }
            else
            {
                Debug.LogWarning("Attention : Un score parfait a été atteint, mais le VictoryEffect n'est pas assigné dans l'inspecteur !");
            }
        }
        // ---------------------------------------------------
    }

    public void RejouerQuiz()
    {
        // 1. On remet les compteurs à zéro
        currentQuestionIndex = 0;
        score = 0;
        quizFinished = false; // Très important pour débloquer ton Timer !

        // 2. On prépare l'UI
        UpdateScoreUI();
        replayButton.gameObject.SetActive(false); // On cache le bouton rejouer
        
        // On réactive les éléments que FinishQuiz avait cachés
        questionText.gameObject.SetActive(true);
        // questionImage.gameObject.SetActive(true); // Optionnel selon ton setup
        foreach (var btn in answerButtons)
        {
            btn.gameObject.SetActive(true);
        }

        // 3. On relance la machine
        if (timerRunCoroutine != null) StopCoroutine(timerRunCoroutine);
        timerRunCoroutine = StartCoroutine(TimerCoroutine());
        DisplayQuestion();
    }

    IEnumerator TimerCoroutine()
    {
        while(currentQuestionIndex < questionData.questions.Length && !quizFinished)
        {
            if (timerRunning)
            {
                timer -= Time.deltaTime;
                timerText.text = "Temps : " + Mathf.Ceil(timer);

                if (timer <= 0f)
                {
                    timerRunning = false;

                    foreach (var btn in answerButtons)
                    {
                        btn.interactable = false;
                    }

                    // Lancer le feedback une seule fois
                    if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
                    feedbackCoroutine = StartCoroutine(PlayFeedback("Temps écoulé !", badColor, wrongSFX, false));
                }

                yield return null; // Toujours céder le contrôle
            }
            else
            {
                // Si le minuteur est arrêté (une réponse a été donnée), on attend simplement
                yield return null;
            }
        }
    }

    void UpdateScoreUI()
    {
        scoreText.text = "Score : " + score;
    }

}
