using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class CryptogramManager : MonoBehaviour
{
    public static CryptogramManager Instance;

    [Header("Configuration")]
    [SerializeField] private CipherMapping cipherMapping; // Ta clé de substitution
    [SerializeField, TextArea] private string secretPhrase = "LE TRESOR EST ICI";

    [Header("Données du Jeu")]
    [SerializeField] private List<CipherLetter> _gameLetters = new List<CipherLetter>();

    // Événement pour prévenir l'UI qu'une lettre a changé
    public Action OnLetterUpdated;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        PrepareGame(); // Généère les lettres au démarrage
    }

    // Prépare les données du jeu en chiffrant la phrase secrète
    private void PrepareGame()
    {
        _gameLetters.Clear();
        // On récupère le dictionnaire ultra-rapide
        var solutionMap = cipherMapping.GetSolutionMap();

        // On parcourt chaque caractère de la phrase secrète
        foreach (char c in secretPhrase.ToUpper())
        {
            if (c == ' ')
            {
                // Ajouter un espace sans chiffrement
                _gameLetters.Add(new CipherLetter(" ", ' '));
                continue; // Pour arrêter la boucle ici
            }
           
           // On cherche quel symbole correspond à cette lettre dans ton Mapping
            string associatedSymbol = "";
            foreach (var pair in cipherMapping.KeyPairs)
            {
                if (char.ToUpper(pair.SolutionLetter) == c)
                {
                    associatedSymbol = pair.CipherSymbol;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(associatedSymbol))
            {
                // On crée l'objet de donnée pour cette lettre précise
                _gameLetters.Add(new CipherLetter(associatedSymbol, c));
            }
        }
    }

    public List<CipherLetter> GetGameLetters() => _gameLetters;

    // --- LA MÉTHODE DE VÉRIFICATION ---
    public bool CheckIfLetterIsCorrect(string symbol, char guess)
    {
        foreach (var letter in _gameLetters) 
        {
            if (letter.CipherSymbol == symbol)
            {
                // On utilise SolutionLetter (le vrai nom dans ton script CipherLetter)
                return char.ToUpper(guess) == letter.SolutionLetter;
            }
        }
        return false;
    }

    // Assigne la lettre et déclenche la mise à jour visuelle
    public void PlayerAssignLetter(string symbol, char guess)
    {
        foreach (var letter in _gameLetters)
        {
            if (letter.CipherSymbol == symbol)
            {
               // IsCorrect se mettra à jour tout seul dans CipherLetter !
                letter.PlayerGuess = char.ToUpper(guess);
            }
        }
        // On prévient l'UI qu'il faut se redessiner
        OnLetterUpdated?.Invoke();
    }

    // Calcule le pourcentage de progression
    public float GetCompletionProgress()
    {
       if (_gameLetters == null || _gameLetters.Count == 0) return 0f;

        int correctCount = _gameLetters.Count(l => l.IsCorrect);
        return (float)correctCount / _gameLetters.Count;
    }
}