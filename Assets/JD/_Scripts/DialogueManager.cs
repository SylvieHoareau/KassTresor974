using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Elements")]
    public GameObject dialogueBox;
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI contentText;
    public Transform choicesContainer;
    public GameObject choiceButtonPrefab;

    private void Awake()
    {
        Instance = this;

        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }

    public void ShowMessage(DialogueSpeaker speaker, string content)
    {
        dialogueBox.SetActive(true);

        nameText.text = speaker.characterName;
        nameText.color = speaker.nameColor;
        contentText.text = content;

        if (speaker.portrait != null)
        {
            portraitImage.gameObject.SetActive(true);
            portraitImage.sprite = speaker.portrait;
        }
        else
        {
            portraitImage.gameObject.SetActive(false);
        }

        DeleteOldChoices();
    }

    public void AddChoice(string textChoice, UnityAction onClickAction, bool autoClose = true)
    {
        GameObject newButton = Instantiate(choiceButtonPrefab, choicesContainer);
        newButton.GetComponentInChildren<TextMeshProUGUI>().text = textChoice;

        newButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            onClickAction.Invoke();

            if (autoClose)
                CloseDialogue();
        });
    }

    public void CloseDialogue()
    {
        dialogueBox.SetActive(false);
        DeleteOldChoices();
    }

    // ✅ NOUVEAU : méthode publique pour vider les choix depuis d'autres scripts
    public void ClearChoices()
    {
        DeleteOldChoices();
    }

    // (resté privé, mais accessible via ClearChoices)
    void DeleteOldChoices()
    {
        if (choicesContainer == null) return;

        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
