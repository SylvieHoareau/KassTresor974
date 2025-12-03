using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // --- Références Obligatoires (à assigner dans l'Inspector) ---
    
    [Header("Références du Manager")]
    [Tooltip("Le cerveau de la logique du jeu (CryptogramManager).")]
    [SerializeField] private CryptogramManager cryptogramManager;

    [Header("Références UI de la Scène")]
    [Tooltip("Le GameObject parent où les symboles de cryptogramme seront instanciés.")]
    [SerializeField] private Transform cryptogramGridContainer;
    
    [Tooltip("Le Prefab de l'élément UI représentant un seul symbole chiffré.")]
    [SerializeField] private CipherSymbolUI cipherSymbolPrefab;

    [Header("Panneau de Saisie (Letter Selection)")]
    [Tooltip("Référence au panneau UI qui contient les boutons de sélection de lettres (A, B, C...).")]
    [SerializeField] private GameObject letterSelectionPanel;

    // Référence au symbole sur lequel le joueur a cliqué pour l'instant
    private string _currentSymbolToGuess; 
    
    // Liste des instances UI créées pour rafraîchir rapidement
    private List<CipherSymbolUI> _instantiatedSymbols = new List<CipherSymbolUI>();

    // ====================================================================
    // 1. INITIALISATION (Startup)
    // ====================================================================
    
    private void Start()
    {
        if (cryptogramManager == null || cipherSymbolPrefab == null || cryptogramGridContainer == null)
        {
            Debug.LogError("UIManager non configuré ! Veuillez assigner le Manager, le Conteneur et le Prefab.");
            return;
        }

        // 1. Abonnement à l'événement de mise à jour du jeu
        cryptogramManager.OnLetterUpdated += RefreshAllSymbols;
        
        // 2. Initialisation de l'affichage du cryptogramme
        InstantiateCryptogramSymbols();
        
        // 3. Assurez-vous que le panneau de saisie est désactivé au démarrage
        if (letterSelectionPanel != null)
        {
            letterSelectionPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Nettoyage de l'abonnement pour éviter les fuites de mémoire (très important !)
        if (cryptogramManager != null)
        {
            cryptogramManager.OnLetterUpdated -= RefreshAllSymbols;
        }
    }

    // Crée les éléments visuels basés sur la liste de données du manager
    private void InstantiateCryptogramSymbols()
    {
        // Vider l'ancien contenu si la méthode était appelée plusieurs fois
        foreach (Transform child in cryptogramGridContainer)
        {
            Destroy(child.gameObject);
        }
        _instantiatedSymbols.Clear();

        // Créer un élément UI pour chaque 'CipherLetter'
        foreach (var letterData in cryptogramManager.GetGameLetters())
        {
            // Instanciation
            CipherSymbolUI newSymbolUI = Instantiate(cipherSymbolPrefab, cryptogramGridContainer);
            
            // Configuration : l'UI a besoin de la donnée et de la référence du Manager UI pour les actions
            // L'UI va maintenant appeler OpenLetterSelection dans ce UIManager, et non le CryptogramManager directement.
            newSymbolUI.Setup(letterData, this); 
            
            _instantiatedSymbols.Add(newSymbolUI);
        }
    }

    // ====================================================================
    // 2. GESTION DE L'AFFICHAGE (View Refresh)
    // ====================================================================

    /// <summary>
    /// Appelé via l'événement OnLetterUpdated du CryptogramManager.
    /// Met à jour l'état visuel de tous les symboles affichés.
    /// </summary>
    private void RefreshAllSymbols()
    {
        foreach (var symbol in _instantiatedSymbols)
        {
            symbol.UpdateVisuals();
        }
        
        // Optionnel : Mettre à jour la barre de progression ou le texte de victoire
        // Debug.Log($"Progression : {cryptogramManager.GetCompletionProgress() * 100}%");
    }

    // ====================================================================
    // 3. GESTION DE L'INTERACTION (Selection Panel)
    // ====================================================================

    /// <summary>
    /// Appelé par CipherSymbolUI lorsque le joueur clique sur un symbole.
    /// Ouvre le panneau de sélection de lettres.
    /// </summary>
    /// <param name="symbol">Le symbole chiffré sur lequel on a cliqué (ex: "F1").</param>
    public void OpenLetterSelection(string symbol)
    {
        if (letterSelectionPanel == null)
        {
            // Si vous n'avez pas de panneau de saisie, vous devez le gérer dans CipherSymbolUI.
            Debug.LogError("LetterSelectionPanel non assigné. Impossible d'ouvrir la sélection.");
            return;
        }
        
        _currentSymbolToGuess = symbol;
        letterSelectionPanel.SetActive(true);
        
        // Optionnel : Centrer le panneau près du symbole cliqué
    }

    /// <summary>
    /// Appelé par un des boutons de sélection de lettres (A, B, C...) sur le panneau.
    /// C'est l'action finale qui soumet la devinette.
    /// </summary>
    /// <param name="guessChar">La lettre choisie par le joueur (ex: 'A').</param>
    public void SubmitLetterGuess(string guessStr)
    {
        if (string.IsNullOrEmpty(_currentSymbolToGuess)) return;

        // EXTRAIRE LE CARACTERE
        // On prend le premier caractère qui est de type char
        char guessChar = guessStr[0];

        // ENVOI AU MANAGER
        // Soumettre au CryptogramManager qui gère la logique globale de substitution.
        cryptogramManager.PlayerAssignLetter(_currentSymbolToGuess, guessChar);
        
        // Cacher le panneau après la sélection
        letterSelectionPanel.SetActive(false);
        _currentSymbolToGuess = null; 
    }
}
