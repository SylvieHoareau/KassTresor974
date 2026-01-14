using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Pour LINQ (compte les éléments)

public class CryptogramManager_Old : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Référence au ScriptableObject contenant la clé de chiffrement du Forban")]
    public CipherMapping CipherKey;

    [Tooltip("Le message chiffré à décoder, ligne par ligne")]
    // Il faut saisir ici le message exact en utilisant les symboles de l'image 
    // et en séparant les "mots" par un espace.

    // Le début du message de La Buse
    [TextArea(5, 10)]
    public string CipherText = 
        "F1 U1 P1 L1 D1 I1 L1 A1 T1 L1 A1 N1 J1 O1 F1 L1 U1 I1 E1 N1 D1 O1 R1 T1 V1 O1 R1 L1 Y1 C1 E1 L1 V1 Z1 U1 R1 L1 T1 V1 F1 V1 E1 L1 J1 O1 V1 L1 V1 L1 U1 I1 E1 J1 E1 I1 L1 A1 T1 L1 E1 N1 T1 V1 L1 O1 C1 V1 V1 B1 V1 J1 D1 B1 L1 T1 V1 L1 U1 I1 E1 N1 F1 I1 E1 I1 J1 L1 A1 T1 L1 A1 C1 C1 B1 I1 R1 L1 U1 L1 T1 O1 B1 I1 E1 C1 C1 L1 L1 A1 R1 V1 I1 L1 L1 A1 V1 L1 T1 I1 J1 L1 O1 V1 I1 E1 V1 T1 E1 R1 A1 T1 J1 V1 R1 E1 V1 V1 E1 A1 Y1 R1 E1 I1 N1 J1 A1 N1 L1 U1 I1 E1 C1 J1 N1 L1 F1 V1 R1 V1 O1 B1 V1 R1 A1 V1 I1 <1 N1 A1 C1 L1 A1 N1 C1 E1 L1 V1 N1 F1 I1 R1 L1 T1 E1 A1 S1 Z1 C1 L1 V1 J1 V1 V1 Y1 L1 V1 E1 A1 R1 C1 L1 U1 J1 L1 I1 T1 O1 B1 I1 C1 L1 A1 Y1 F1 A1 L1 V1 V1 N1 A1 V1 Y1 J1 T1 R1 O1 V1 O1 L1 U1 N1 A1 C1 C1 L1 N1 A1 R1 <1 L1 I1 N1 J1 L1 U1 B1 L1 A1 J1 T1 I1 L1 L1 I1 T1 L1 U1 I1 C1 J1 T1 L1 U1 J1 V1 E1 Y1 <1 N1 E1 V1 T1 J1 C1 L1 F1 A1 I1 <1 R1 A1 V1 V1 L1 G1 I1 R1 L1 E1 C1 J1 U1 R1 J1 J1 A1 U1 M1 L1 J1 V1 R1 N1 N1 R1 <1 L1 T1 J1 J1 F1 B1 V1 V1 N1 A1 F1 N1 L1 B1 T1 J1 C1 E1 N1 A1 L1 O1 A1 <1 B1 C1 L1 V1 J1 V1 F1 E1 A1 T1 L1 B1 C1 O1 T1 A1 R1 <1 R1 E1 J1 O1 B1 N1 O1 V1 L1 R1 T1 Z1 H1 J1 B1 A1 T1 V1 A1 N1 F1 E1 <1 T1 C1 L1 T1 B1 J1 U1 E1 C1 J1 D1 F1 I1 G1 U1 B1 L1 J1 O1 V1 T1 I1 N1 O1 L1 L1 V1 V1 A1 R1 R1 I1 A1 I1 R1 <1 E1 L1 T1 F1 <1 A1 E1 C1 T1 O1 V1 B1 D1 L1 E1 A1 U1 L1 E1 <1 L1 E1 E1 L1 G1 R1 F1 F1 B1 <1 L1 E1 V1 V1 E1 L1 L1 J1 B1 F1 L1 U1 I1 A1 T1 I1 V1 L1 V1 V1 L1 U1 B1 L1 V1 L1 K1 W1 C1 L1 U1 J1 T1 V1 U1 A1 <1 <1 B1 *1 N1 R1 F1 T1 U1 A1 R1 T1 O1 F1 R1 T1 E1 R1 I1 A1 T1 U1 J1 R1 P1 P1 L1 V1 V1 U1 L1 <1 <1 I1 /1 M1 E1 I1 L1 E1 L1 C1 F1 V1 T1 O1 R1 L1 U1 E1 F1 <1 E1 J1 U1 D1 L1 L1 A1 F1 F1 L1 Y1 B1 E1 L1 L1 <1 R1 J1 V1 E1 L1 V1 C1 L1 V1 E1 L1 V1 C1";

    // La liste de tous les objets 'CipherLetter' représentant le jeu
    private List<CipherLetter> _gameLetters;

    // Evénement déclenché quand le joueur change une lettre (pour rafraîchir l'UI)
    public event System.Action OnLetterUpdated; 

    // Pour un accès plus rapide aux lettres uniques (utile pour la désaffectation)
    private Dictionary<string, char> _currentGuesses = new Dictionary<string, char>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        InitializeCryptogram();
    }

    private void InitializeCryptogram()
    {
        if (CipherKey == null)
        {
            Debug.LogError("CipherKey is not assigned in CryptogramManager.");
            return;
        }

        // Obtient la carte de solution (symbole -> lettre claire)
        Dictionary<string, char> solutionMap = CipherKey.GetSolutionMap();

        _gameLetters = new List<CipherLetter>();
        // Divise le texte chiffré en symboles individuels
        string[] symbols = CipherText.Split(new char[] { ' ', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string symbol in symbols)
        {
            // Trouver la solution pour ce symbole
            char solution = solutionMap.ContainsKey(symbol) ? solutionMap[symbol] : '?';

            _gameLetters.Add(new CipherLetter(symbol, solution));
        }

        Debug.Log($"Cryptogramme initialisé. Nombre de symboles à déchiffrer : {_gameLetters.Count}");
    }

    // --- LOGIQUE D'INTERACTION DU JOUEUR ---

    /// <summary>
    /// Met à jour la supposition du joueur pour un symbole donné.
    /// </summary>
    /// <param name="targetSymbol">Le symbole chiffré à mettre à jour.</param>
    /// <param name="guess">La lettre que le joueur a devinée.</param>
    public void PlayerAssignLetter(string targetSymbol, char guess)
    {
        char upperGuess = char.ToUpper(guess);

        // Appliquer le changement à TOUTES les instance de ce symbole
        foreach (var letter in _gameLetters.Where(l => l.CipherSymbol == targetSymbol))
        {
            letter.PlayerGuess = upperGuess;
        }

        // Informer l'UI qu'elle doit se rafraîchir
        OnLetterUpdated?.Invoke();

        // Vérifier si le jeu est terminé
        if (IsGameComplete())
        {
            Debug.Log("Félicitations ! Vous avez déchiffré le message !");  
            // Fin du jeu et révélation du trésor
        }
    }

    /// <summary>
    /// Retire la supposition du joueur pour un symbole, le remettant à l'état non deviné (' ').
    /// </summary>
    /// <param name="targetSymbol">Le symbole chiffré à réinitialiser.</param>
    public void PlayerUnassignLetter(string targetSymbol)
    {
        // 1. Retirer du mapping des devinettes
        if (_currentGuesses.ContainsKey(targetSymbol))
        {
            _currentGuesses.Remove(targetSymbol);
        }

        // 2. Appliquer la devinette vide à TOUTES les instances de ce symbole
        foreach (var letter in _gameLetters.Where(l => l.CipherSymbol == targetSymbol))
        {
            letter.PlayerGuess = ' ';
        }

        // 3. Informer l'UI
        OnLetterUpdated?.Invoke();
    }

    // --- LOGIQUE DE JEU ET D'ETAT ---

    // Permet à l'UI d'accéder à la liste pour laffichage
    public List<CipherLetter> GetGameLetters()
    {
        return _gameLetters;
    }

    /// <summary>
    /// Calcule le pourcentage de symboles correctement déchiffrés par le joueur.
    /// </summary>
    /// <returns>Pourcentage de progression (0.0f à 1.0f).</returns>
    public float GetCompletionProgress()
    {
        if (_gameLetters == null || _gameLetters.Count == 0) return 0f;

        int correctCount = _gameLetters.Count(letter => letter.IsCorrect);
        return (float)correctCount / _gameLetters.Count;
    }

    /// <summary>
    /// Vérifie si toutes les lettres ont été correctement devinées.
    /// </summary>
    public bool IsGameComplete()
    {
        // Le jeu est complet si toutes les lettres sont correctes
        return _gameLetters.All(letter => letter.IsCorrect);
    }
}
