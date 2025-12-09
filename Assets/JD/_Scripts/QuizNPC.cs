using UnityEngine;
using UnityEngine.Events;

public class QuizNPC : MonoBehaviour
{
    [Header("Identité")]
    public DialogueSpeaker speakerProfile;

    [Header("Le Dialogue")]
    [TextArea] public string greeting = "Holà voyageur !";
    [TextArea] public string question = "Combien font 2 + 2 ?";

    [Header("Les Réponses")]
    public string[] answers;
    public int correctAnswerIndex = 0;

    [Header("Réactions")]
    [TextArea] public string victoryText = "Bravo !";
    [TextArea] public string defeatText = "Faux !";

    [Header("Evénements")]
    public UnityEvent onWin;                     // Quand le joueur valide la bonne réponse
    public UnityEvent onVictoryUIDisappear;      // Quand le texte disparaît (pour ton timer)

    [Header("UI de victoire (optionnel)")]
    public GameObject victoryUI;

    private bool hasWon = false;

    private void Start()
    {
        if (victoryUI != null)
            victoryUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasWon)
        {
            StartQuiz();
        }
    }

    void StartQuiz()
    {
        DialogueManager.Instance.ShowMessage(speakerProfile, greeting);
        DialogueManager.Instance.AddChoice(">", AskQuestion, false);
    }

    void AskQuestion()
    {
        DialogueManager.Instance.ShowMessage(speakerProfile, question);

        for (int i = 0; i < answers.Length; i++)
        {
            int index = i;
            DialogueManager.Instance.AddChoice(answers[i], () => CheckAnswer(index), false);
        }
    }

    void CheckAnswer(int index)
    {
        if (index == correctAnswerIndex)
        {
            hasWon = true;

            DialogueManager.Instance.ShowMessage(speakerProfile, victoryText);

            // Affiche l'UI
            if (victoryUI != null)
                victoryUI.SetActive(true);

            // Quand le joueur continue → changement de scène ou autre
            DialogueManager.Instance.AddChoice(">", () =>
            {
                onWin.Invoke();
                DialogueManager.Instance.CloseDialogue();
            });
        }
        else
        {
            DialogueManager.Instance.ShowMessage(speakerProfile, defeatText);
            DialogueManager.Instance.AddChoice(">", AskQuestion, false);
            DialogueManager.Instance.AddChoice("Partir", () => DialogueManager.Instance.CloseDialogue());
        }
    }

    // --- Fonction que TON TIMER appellera pour cacher l'UI ---
    public void HideVictoryUI()
    {
        if (victoryUI != null)
            victoryUI.SetActive(false);

        onVictoryUIDisappear.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            DialogueManager.Instance.CloseDialogue();
    }
}
