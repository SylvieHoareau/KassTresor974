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
    public UnityEvent onWin;
    public UnityEvent onVictoryUIDisappear;

    [Header("UI de victoire (optionnel)")]
    public GameObject victoryUI;

    [Header("Contrôles clavier")]
    public KeyCode validateKey = KeyCode.E;
    public KeyCode upKey = KeyCode.Z;
    public KeyCode downKey = KeyCode.S;

    private bool hasWon = false;
    private bool playerInRange = false;

    private enum QuizState { None, Greeting, Question, Victory, Defeat }
    private QuizState state = QuizState.None;

    private int selectedIndex = 0;

    private void Start()
    {
        if (victoryUI != null)
            victoryUI.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (state == QuizState.None) return;

        if (Input.GetKeyDown(upKey))
            MoveSelection(-1);

        if (Input.GetKeyDown(downKey))
            MoveSelection(+1);

        if (Input.GetKeyDown(validateKey))
            ActivateSelection();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasWon)
        {
            playerInRange = true;
            StartQuiz();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            state = QuizState.None;
            DialogueManager.Instance.CloseDialogue();
        }
    }

    void StartQuiz()
    {
        state = QuizState.Greeting;
        selectedIndex = 0;

        DialogueManager.Instance.ShowMessage(speakerProfile, greeting);
        RedrawChoices();
    }

    void AskQuestion()
    {
        state = QuizState.Question;
        selectedIndex = 0;

        DialogueManager.Instance.ShowMessage(speakerProfile, question);
        RedrawChoices();
    }

    void CheckAnswer(int index)
    {
        if (index == correctAnswerIndex)
        {
            hasWon = true;
            state = QuizState.Victory;
            selectedIndex = 0;

            DialogueManager.Instance.ShowMessage(speakerProfile, victoryText);

            if (victoryUI != null)
                victoryUI.SetActive(true);

            RedrawChoices();
        }
        else
        {
            state = QuizState.Defeat;
            selectedIndex = 0;

            DialogueManager.Instance.ShowMessage(speakerProfile, defeatText);
            RedrawChoices();
        }
    }

    public void HideVictoryUI()
    {
        if (victoryUI != null)
            victoryUI.SetActive(false);

        onVictoryUIDisappear.Invoke();
    }

    private int GetChoiceCount()
    {
        switch (state)
        {
            case QuizState.Greeting: return 1; // ">"
            case QuizState.Question: return answers != null ? answers.Length : 0;
            case QuizState.Victory:  return 1; // ">"
            case QuizState.Defeat:   return 2; // ">" + "Partir"
            default: return 0;
        }
    }

    private void MoveSelection(int delta)
    {
        int count = GetChoiceCount();
        if (count <= 0) return;

        selectedIndex += delta;
        if (selectedIndex < 0) selectedIndex = count - 1;
        if (selectedIndex >= count) selectedIndex = 0;

        RedrawChoices(); // ✅ redraw propre (avec ClearChoices)
    }

    private void ActivateSelection()
    {
        switch (state)
        {
            case QuizState.Greeting:
                AskQuestion();
                break;

            case QuizState.Question:
                if (answers == null || answers.Length == 0) return;
                CheckAnswer(selectedIndex);
                break;

            case QuizState.Victory:
                onWin.Invoke();
                DialogueManager.Instance.CloseDialogue();
                state = QuizState.None;
                break;

            case QuizState.Defeat:
                if (selectedIndex == 0) AskQuestion();
                else
                {
                    DialogueManager.Instance.CloseDialogue();
                    state = QuizState.None;
                }
                break;
        }
    }

    private void RedrawChoices()
    {
        // ✅ LE FIX ANTI-NARUTO
        DialogueManager.Instance.ClearChoices();

        switch (state)
        {
            case QuizState.Greeting:
                DialogueManager.Instance.AddChoice(FormatChoice(">", 0), AskQuestion, false);
                break;

            case QuizState.Question:
                for (int i = 0; i < answers.Length; i++)
                {
                    int index = i;
                    DialogueManager.Instance.AddChoice(
                        FormatChoice(answers[i], i),
                        () => CheckAnswer(index),
                        false
                    );
                }
                break;

            case QuizState.Victory:
                DialogueManager.Instance.AddChoice(FormatChoice(">", 0), () =>
                {
                    onWin.Invoke();
                    DialogueManager.Instance.CloseDialogue();
                    state = QuizState.None;
                });
                break;

            case QuizState.Defeat:
                DialogueManager.Instance.AddChoice(FormatChoice(">", 0), AskQuestion, false);
                DialogueManager.Instance.AddChoice(FormatChoice("Partir", 1), () =>
                {
                    DialogueManager.Instance.CloseDialogue();
                    state = QuizState.None;
                }, false);
                break;
        }
    }

    private string FormatChoice(string label, int index)
    {
        return (index == selectedIndex ? "> " : "  ") + label;
    }
}
