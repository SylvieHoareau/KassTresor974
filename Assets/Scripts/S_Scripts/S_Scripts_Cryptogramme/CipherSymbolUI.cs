using UnityEngine;
using UnityEngine.UI;

public class CipherSymbolUI : MonoBehaviour
{
    // Références UI
    [SerializeField] private Text cipherText; // Affiche le symbole F1, U1, P1
    [SerializeField] private Text guessText; // Affiche la devinette du joueur
    [SerializeField] private Button symbolButton; // Bouton pour interagir avec le symbole

    // Référence à l'objet de données du symbole chiffré
    private CipherLetter _data;

    // Référence au manager pour soumettre la devinette
    private UIManager _uiManager;

    public void Setup(CipherLetter data, UIManager uiManager)
    {
        _data = data;
        _uiManager = uiManager;

        // Initialiser l'affichage UI
        cipherText.text = _data.CipherSymbol;

        // Ajouter le listener au bouton
        symbolButton.onClick.AddListener(OnSymbolClicked);

        UpdateVisuals(); // Mise à jour initiale des visuels
    }

    // Appelé par le Manager via l'événement OnLetterUpdated
    public void UpdateVisuals()
    {
        // Met à jour le texte de la devinette
        guessText.text = _data.PlayerGuess != ' ' ? _data.PlayerGuess.ToString() : "_";
        // Feedback visuel si la devinette est correcte
        guessText.color = _data.IsCorrect ? Color.green : Color.white;
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
