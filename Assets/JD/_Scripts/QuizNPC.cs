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

    [Header("Evénement de Victoire")]
    public UnityEvent onWin; 

    private bool hasWon = false;

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
        // ", false" pour ne pas fermer
        DialogueManager.Instance.AddChoice("Je suis prêt", AskQuestion, false);
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

    // --- C'EST ICI QUE ÇA CHANGE ---
    void CheckAnswer(int index)
    {
        if (index == correctAnswerIndex)
        {
            // --- VICTOIRE ---
            hasWon = true;
            DialogueManager.Instance.ShowMessage(speakerProfile, victoryText);
            
            // AVANT : onWin.Invoke() était ici, donc c'était immédiat.
            
            // MAINTENANT : On crée un bouton "Continuer"
            // Et c'est QUAND on clique dessus que la scène change (onWin)
            DialogueManager.Instance.AddChoice("Continuer", () => 
            {
                onWin.Invoke(); // <-- L'action se lance maintenant
                DialogueManager.Instance.CloseDialogue();
            });
        }
        else
        {
            // --- DÉFAITE ---
            DialogueManager.Instance.ShowMessage(speakerProfile, defeatText);
            DialogueManager.Instance.AddChoice("Réessayer", AskQuestion, false);
            DialogueManager.Instance.AddChoice("Partir", () => DialogueManager.Instance.CloseDialogue());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueManager.Instance.CloseDialogue();
        }
    }
}