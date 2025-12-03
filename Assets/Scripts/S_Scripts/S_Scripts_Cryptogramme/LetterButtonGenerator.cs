using UnityEngine;
using UnityEngine.UI; // Pour la classe Button
using TMPro;

public class LetterButtonGenerator : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Référence au UIManager pour soumettre la devinette.")]
    [SerializeField] private UIManager uiManager;

    [Tooltip("Le Prefab du bouton de sélection (doit contenir un Button et un TextMeshPro)")]
    [SerializeField] private Button buttonPrefab;

    [Tooltip("Le conteneur parent où les boutons seront instanciés.")]
    [SerializeField] private Transform buttonsContainer;

    [Header("Configuration des Lettres")]
    [Tooltip("Inclure un bouton pour retirer la lettre (' ').")]
    [SerializeField] private bool includeClearButton = true;

    private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (uiManager == null || buttonPrefab == null || buttonsContainer == null)
        {
            Debug.LogError("LetterButtonGenerator non configuré ! Veuillez assigner le UIManager, le Prefab et le Conteneur.");
            return;
        }

        // Générer les lettres de l'alphabet (A à Z)
        foreach (char letter in ALPHABET)
        {
            CreateButton(letter.ToString());
        }

        // Ajouter le bouton pour effacer la devinette si nécessaire
        if (includeClearButton)
        {
            CreateButton("Retirer");
        }
    }

    private void CreateButton(string buttonText)
    {
        // Instancier le Prefab du bouton
        Button newButton = Instantiate(buttonPrefab, buttonsContainer);
        newButton.name = "Button_" + buttonText;

        // Mise à jour du texte du bouton
        TextMeshProUGUI buttonTextComponent = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonTextComponent != null)
        {
            buttonTextComponent.text = buttonText;
        }

        // Configuration du Listener OnClick
        if (buttonText == "Retirer")
        {
            // Pour le bouton Retirer, on soumet un espace (' ') ou une autre valeur spéciale.
            newButton.onClick.AddListener(() => uiManager.SubmitLetterGuess(" ")); 
            // NOTE: Vous pourriez aussi créer une méthode PlayerUnassignLetter spécifique dans UIManager.
        }
        else
        {
            // Pour les lettres, on soumet la lettre elle-même (ex: "A", "B")
            // On capture la variable 'buttonText' pour qu'elle soit utilisée dans la lambda (closure)
            string letterToSubmit = buttonText;
            newButton.onClick.AddListener(() => uiManager.SubmitLetterGuess(letterToSubmit));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
