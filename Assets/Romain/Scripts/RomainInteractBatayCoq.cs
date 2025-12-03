using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class RomainInteractBatayCoq : MonoBehaviour
{
    [Header("UI")]
    public GameObject interactMessage;   // Texte "Interagir"
    public GameObject choicePanel;       // Panel avec Oui / Non
    public TMP_Text titleText;           // Titre : "Voulez-vous jouer à Batay Coq ?"

    [Header("Boutons")]
    public Button yesButton;             // Bouton Oui
    public Button noButton;              // Bouton Non

    [Header("Scène")]
    public string sceneName = "BatayCoqScene";

    private bool playerInRange = false;
    private bool isChoosing = false;

    void Start()
    {
        if (interactMessage != null)
            interactMessage.SetActive(false);

        if (choicePanel != null)
            choicePanel.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange || isChoosing) 
            return;

        // INTERACTION = E clavier + bouton OUEST manette (JoystickButton2)
        bool interactPressed =
            Input.GetKeyDown(KeyCode.E) ||
            Input.GetKeyDown(KeyCode.JoystickButton0);

        if (interactPressed)
        {
            OpenChoice();
        }
    }

    private void OpenChoice()
    {
        isChoosing = true;

        if (interactMessage != null)
            interactMessage.SetActive(false);

        if (choicePanel != null)
            choicePanel.SetActive(true);

        if (titleText != null)
            titleText.text = "Voulez-vous jouer à Batay Coq ?";

        // Évite que Unity valide un bouton automatiquement
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // Sélection automatique du bouton Oui pour navigation manette
        if (EventSystem.current != null && yesButton != null)
            EventSystem.current.SetSelectedGameObject(yesButton.gameObject);
    }

    public void OnClickYes()
    {
        SceneManager.LoadScene(sceneName);
    }

    public void OnClickNo()
    {
        if (choicePanel != null)
            choicePanel.SetActive(false);

        isChoosing = false;

        // Nettoyage de la sélection UI
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) 
            return;

        playerInRange = true;

        if (interactMessage != null)
            interactMessage.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) 
            return;

        playerInRange = false;

        if (interactMessage != null)
            interactMessage.SetActive(false);

        if (choicePanel != null)
            choicePanel.SetActive(false);

        isChoosing = false;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
