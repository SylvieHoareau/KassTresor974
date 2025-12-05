using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueSequence : MonoBehaviour
{
    // Une structure pour définir une "Ligne de dialogue" dans l'inspecteur
    [System.Serializable]
    public struct DialogueLine
    {
        public DialogueSpeaker speaker; // Qui parle ?
        [TextArea] public string text;  // Quoi ?
    }

    [Header("Configuration")]
    public List<DialogueLine> sequence; // La liste des dialogues à enchaîner
    public float startDelay = 1f;       // Délai avant de commencer

    private int currentIndex = 0;

    void Start()
    {
        StartCoroutine(StartRoutine());
    }

    IEnumerator StartRoutine()
    {
        yield return new WaitForSeconds(startDelay);
        PlayNextLine();
    }

    void PlayNextLine()
    {
        // Si on a fini la liste
        if (currentIndex >= sequence.Count)
        {
            DialogueManager.Instance.CloseDialogue();
            EnableExit(); // On débloque la sortie
            return;
        }

        // On récupère la ligne actuelle
        DialogueLine line = sequence[currentIndex];

        // On affiche le message
        DialogueManager.Instance.ShowMessage(line.speaker, line.text);

        // MODIFICATION ICI : On ajoute ", false" à la fin
        DialogueManager.Instance.AddChoice("Continuer...", () => 
        {
            currentIndex++; 
            PlayNextLine(); 
        }, false); // <--- False veut dire : "Ne ferme pas la boite, j'ai encore des trucs à dire"
    }

    void EnableExit()
    {
        Debug.Log("Séquence finie ! La sortie est ouverte.");
        // Ici tu pourras activer le trigger de fin si tu veux le bloquer avant
    }
}