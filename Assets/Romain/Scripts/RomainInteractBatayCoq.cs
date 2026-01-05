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

    [Header("Contrôles clavier")]
    public KeyCode interactKey = KeyCode.E;
    public KeyCode leftKey = KeyCode.Q;   // Q = gauche (AZERTY)
    public KeyCode rightKey = KeyCode.D;  // D = droite (AZERTY)

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
        // --- Ouvrir le choix ---
        if (!isChoosing)
        {
            if (!playerInRange)
                return;

            bool interactPressed =
                Input.GetKeyDown(interactKey) ||
                Input.GetKeyDown(KeyCode.JoystickButton0);

            if (interactPressed)
                OpenChoice();

            return;
        }

        // --- Navigation Oui/Non (quand le panel est ouvert) ---
        if (Input.GetKeyDown(leftKey))
            SelectButton(yesButton); // à gauche = Oui (choix classique)

        if (Input.GetKeyDown(rightKey))
            SelectButton(noButton);

        // --- Valider avec E (ou bouton manette) ---
        bool validatePressed =
            Input.GetKeyDown(interactKey) ||
            Input.GetKeyDown(KeyCode.JoystickButton0);

        if (validatePressed)
        {
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;

            if (selected == null)
            {
                // Si rien n’est sélectionné, on force Oui
                SelectButton(yesButton);
                selected = yesButton != null ? yesButton.gameObject : null;
            }

            if (selected == yesButton.gameObject) OnClickYes();
            else if (selected == noButton.gameObject) OnClickNo();
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

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        SelectButton(yesButton);
    }

    private void SelectButton(Button btn)
    {
        if (EventSystem.current == null || btn == null) return;
        EventSystem.current.SetSelectedGameObject(btn.gameObject);
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
