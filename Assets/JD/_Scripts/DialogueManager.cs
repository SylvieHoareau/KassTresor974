using UnityEngine;
using TMPro;
using UnityEngine.UI; // INDISPENSABLE pour l'Image du portrait
using System.Collections.Generic;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Elements")]
    public GameObject dialogueBox;
    public Image portraitImage;       // Le fameux champ pour l'image
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI contentText;
    public Transform choicesContainer;
    public GameObject choiceButtonPrefab;

    private void Awake()
    {
        // On force la mise à jour : "Moi, le DialogueManager de CETTE scène, je prends le relais"
        Instance = this; 
        
        if (dialogueBox != null) dialogueBox.SetActive(false); 
    }

    // Nouvelle version qui accepte la Fiche (Speaker)
    public void ShowMessage(DialogueSpeaker speaker, string content)
    {
        dialogueBox.SetActive(true);
        
        // 1. On met le nom et la couleur
        nameText.text = speaker.characterName;
        nameText.color = speaker.nameColor;
        contentText.text = content;

        // 2. On gère le portrait
        if (speaker.portrait != null)
        {
            portraitImage.gameObject.SetActive(true);
            portraitImage.sprite = speaker.portrait;
        }
        else
        {
            portraitImage.gameObject.SetActive(false); // On cache si pas d'image
        }
        
        DeleteOldChoices();
    }

    // On ajoute un paramètre "autoClose" qui est VRAI par défaut
    public void AddChoice(string textChoice, UnityAction onClickAction, bool autoClose = true)
    {
        GameObject newButton = Instantiate(choiceButtonPrefab, choicesContainer);
        newButton.GetComponentInChildren<TextMeshProUGUI>().text = textChoice;
        
        newButton.GetComponent<Button>().onClick.AddListener(() => 
        {
            onClickAction.Invoke(); // Fais l'action
            
            // On ne ferme que si on l'a demandé
            if (autoClose)
            {
                CloseDialogue();
            }
        });
    }

    public void CloseDialogue()
    {
        dialogueBox.SetActive(false);
        DeleteOldChoices();
    }

    void DeleteOldChoices()
    {
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }
    }
}