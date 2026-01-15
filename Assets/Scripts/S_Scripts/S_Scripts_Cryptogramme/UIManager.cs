using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Références du Manager")]
    [SerializeField] private CryptogramManager cryptogramManager;

    [Header("Références UI de la Scène")]
    [SerializeField] private Transform cryptogramGridContainer;
    [SerializeField] private CipherSymbolUI cipherSymbolPrefab;
    [SerializeField] private GameObject letterSelectionPanel;

    [Header("Audio Feedbacks")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip soundCorrect;
    [SerializeField] private AudioClip soundWrong;
    [SerializeField] private AudioClip soundClick;

    [Header("Barre de Progression")]
    [SerializeField] private Slider progressSlider;

    private string _currentSymbolToGuess; 
    private List<CipherSymbolUI> _instantiatedSymbols = new List<CipherSymbolUI>();

    private void Start()
    {
        if (cryptogramManager == null) return;

        cryptogramManager.OnLetterUpdated += RefreshAllSymbols;
        InstantiateCryptogramSymbols();
        
        if (letterSelectionPanel != null) letterSelectionPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (cryptogramManager != null)
            cryptogramManager.OnLetterUpdated -= RefreshAllSymbols;
    }

    private void InstantiateCryptogramSymbols()
    {
        foreach (Transform child in cryptogramGridContainer) Destroy(child.gameObject);
        _instantiatedSymbols.Clear();

        foreach (var letterData in cryptogramManager.GetGameLetters())
        {
            CipherSymbolUI newSymbolUI = Instantiate(cipherSymbolPrefab, cryptogramGridContainer);
            newSymbolUI.Setup(letterData, this); 
            _instantiatedSymbols.Add(newSymbolUI);
        }
    }

    private void RefreshAllSymbols()
    {
        foreach (var symbol in _instantiatedSymbols) symbol.UpdateVisuals();

        if (progressSlider != null)
            progressSlider.value = cryptogramManager.GetCompletionProgress();
    }

    public void OpenLetterSelection(string symbol)
    {
        _currentSymbolToGuess = symbol;
        if (letterSelectionPanel != null) letterSelectionPanel.SetActive(true);
    }

    public void SubmitLetterGuess(string guessStr)
    {
        if (string.IsNullOrEmpty(_currentSymbolToGuess) || string.IsNullOrEmpty(guessStr)) return;

        char guessChar = char.ToUpper(guessStr[0]); // Mise en majuscule par sécurité
        bool isCorrect = false; // On initialise par défaut à faux

       // on récupère la liste des lettres pour trouver la solution
        var targetLetter = cryptogramManager.GetGameLetters()
            .Find(l => l.CipherSymbol == _currentSymbolToGuess);

        if (targetLetter != null)
        {
            isCorrect = (guessChar == targetLetter.SolutionLetter);

            // Feedback sonore
            if (audioSource != null)
            {
                if (guessChar != ' ') 
                    audioSource.PlayOneShot(isCorrect ? soundCorrect : soundWrong);
                else 
                    audioSource.PlayOneShot(soundClick);
            }
        }

        // On envoie la décision finale au manager
        cryptogramManager.PlayerAssignLetter(_currentSymbolToGuess, guessChar);
        
        // Nettoyage de l'interface
        if (letterSelectionPanel != null) letterSelectionPanel.SetActive(false);
        _currentSymbolToGuess = null; 
    }
}