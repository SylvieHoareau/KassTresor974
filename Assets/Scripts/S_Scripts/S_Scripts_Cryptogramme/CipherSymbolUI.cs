using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CipherSymbolUI : MonoBehaviour
{
    // Références UI
    [Header("Références UI")]
    [SerializeField] private TextMeshProUGUI cipherText; // Affiche le symbole F1, U1, P1
    [SerializeField] private TextMeshProUGUI guessText; // Affiche la devinette du joueur
    [SerializeField] private Button symbolButton; // Bouton pour interagir avec le symbole
    [SerializeField] private Image backgroundSquare; // Un fond pour le symbole

    [Header("Paramètres Visuels")]
    [SerializeField] private Color colorEmpty = Color.white; 
    [SerializeField] private Color colorCorrect = new Color(0.2f, 0.8f, 0.2f); // Vert
    [SerializeField] private Color colorWrong = new Color(0.8f, 0.2f, 0.2f); // Rouge
    
    // Référence à l'objet de données du symbole chiffré
    private CipherLetter _data;

    // Référence au manager pour soumettre la devinette
    private UIManager _uiManager;

    public void Setup(CipherLetter data, UIManager uiManager)
    {
        _data = data;
        _uiManager = uiManager;

        // Initialiser l'affichage UI Affichage du symbole chiffré
        cipherText.text = _data.CipherSymbol;

        // Ajouter le listener au bouton
        symbolButton.onClick.AddListener(OnSymbolClicked);

        UpdateVisuals(); // Mise à jour initiale des visuels
    }

    // Appelé par le Manager via l'événement OnLetterUpdated
    public void UpdateVisuals()
    {
        // Met à jour le texte de la devinette
        bool hasGuessed = _data.PlayerGuess != ' ';
        // Feedback visuel si la devinette est correcte
        guessText.text = hasGuessed ? _data.PlayerGuess.ToString() : "_";

        if (!hasGuessed)
        {
            guessText.color = Color.gray;
            backgroundSquare.color = colorEmpty;
        }
        else if (_data.IsCorrect)
        {
            guessText.color = Color.green;
            backgroundSquare.color = colorCorrect;
            // Désactiver le bouton si la devinette est correcte
            symbolButton.interactable = false;
        }
        else
        {
            guessText.color = Color.red;
            backgroundSquare.color = colorWrong;
            TriggerShake(); // Petit feedback de mouvement
        }
    }

    // Animation simple de secousse pour indiquer une erreur
    private void TriggerShake()
    {
        StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0f;
        float duration = 0.3f;
        float magnitude = 5f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    private void OnSymbolClicked()
    {
        // Afficher un panneau de saisie pour que le joueur choisisse une lettre

        // Saisie par l'utilisateur
        // char userGuess = 'A';

        // Ouvre l'interface de sélection de lettre dans le manager
        // _manager.PlayerAssignLetter(_data.CipherSymbol, userGuess);

        _uiManager.OpenLetterSelection(_data.CipherSymbol);
    }
}
