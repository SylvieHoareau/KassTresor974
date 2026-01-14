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

        char guessChar = guessStr[0];

        // ON APPELLE LE MANAGER POUR VÉRIFIER
        bool isCorrect = cryptogramManager.CheckIfLetterIsCorrect(_currentSymbolToGuess, guessChar);

        // Feedback sonore
        if (audioSource != null)
        {
            if (guessChar != ' ') 
                audioSource.PlayOneShot(isCorrect ? soundCorrect : soundWrong);
            else 
                audioSource.PlayOneShot(soundClick);
        }

        // On envoie la décision finale au manager
        cryptogramManager.PlayerAssignLetter(_currentSymbolToGuess, guessChar);
        
        if (letterSelectionPanel != null) letterSelectionPanel.SetActive(false);
        _currentSymbolToGuess = null; 
    }
}