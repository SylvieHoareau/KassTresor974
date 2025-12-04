using UnityEngine;
using System.Collections; // Pour le délai

public class DialogueTrigger : MonoBehaviour
{
    public DialogueSpeaker speakerProfile; 
    [TextArea] 
    public string message = "Bienvenue dans le jeu !";

    [Header("Options")]
    public bool autoStart = false; // Si coché, le dialogue part tout seul
    public float autoStartDelay = 1f; // Petit temps d'attente avant de parler

    private bool hasTriggered = false; // Pour éviter qu'il se répète

    // --- NOUVEAU : GESTION DU DÉMARRAGE AUTO ---
    void Start()
    {
        if (autoStart)
        {
            StartCoroutine(StartDialogueRoutine());
        }
    }

    IEnumerator StartDialogueRoutine()
    {
        yield return new WaitForSeconds(autoStartDelay);
        TriggerDialogue();
    }
    // -------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        // On vérifie que c'est le joueur ET qu'on n'a pas déjà déclenché le truc
        if (other.CompareTag("Player") && !hasTriggered)
        {
            TriggerDialogue();
        }
    }

    // On crée une fonction commune pour éviter de copier-coller le code
    void TriggerDialogue()
    {
        hasTriggered = true; // On note que c'est fait
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowMessage(speakerProfile, message);
            
            // Si c'est un monologue, on peut ajouter un bouton "Continuer" par défaut
            DialogueManager.Instance.AddChoice("...", () => 
            { 
                Debug.Log("Fin du monologue"); 
                // Ici on pourrait lancer une animation ou autre
            });
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