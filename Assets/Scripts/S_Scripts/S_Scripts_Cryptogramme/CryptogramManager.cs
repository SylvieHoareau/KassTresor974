using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class CryptogramManager : MonoBehaviour
{
    public static CryptogramManager Instance;

    [Header("Configuration")]
    [SerializeField] private CipherMapping cipherMapping; 
    [SerializeField, TextArea] private string secretPhrase = "LE TRESOR EST ICI";
    
    [Header("Paramètres de Victoire")]
    public GameObject victoryPanel;

    private List<CipherLetter> _gameLetters = new List<CipherLetter>();
    public Action OnLetterUpdated;
    private bool _isGameFinished = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        PrepareGame();
    }

    private void PrepareGame()
    {
        _gameLetters.Clear();
        _isGameFinished = false;

        // On utilise la solution du CipherMapping
        var solutionMap = cipherMapping.GetSolutionMap();

        foreach (char c in secretPhrase.ToUpper())
        {
            if (c == ' ')
            {
                _gameLetters.Add(new CipherLetter(" ", ' '));
                continue;
            }

            // On cherche le symbole associé à cette lettre dans le ScriptableObject
            string associatedSymbol = solutionMap.FirstOrDefault(x => x.Value == c).Key;
            
            if (!string.IsNullOrEmpty(associatedSymbol))
            {
                _gameLetters.Add(new CipherLetter(associatedSymbol, c));
            }
        }
    }

    public void PlayerAssignLetter(string symbol, char guess)
    {
        if (_isGameFinished) return;

        foreach (var letter in _gameLetters)
        {
            if (letter.CipherSymbol == symbol)
            {
                letter.PlayerGuess = char.ToUpper(guess);
            }
        }

        OnLetterUpdated?.Invoke();
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        // On vérifie si toutes les lettres (hors espaces) sont correctes
        bool allCorrect = _gameLetters
            .Where(l => l.CipherSymbol != " ")
            .All(l => l.IsCorrect);

        if (allCorrect && !_isGameFinished)
        {
            _isGameFinished = true;
            if (victoryPanel != null) victoryPanel.SetActive(true);
            Debug.Log("Félicitations ! Message déchiffré.");
        }
    }

    public List<CipherLetter> GetGameLetters() => _gameLetters;
    
    public float GetCompletionProgress()
    {
        var lettersOnly = _gameLetters.Where(l => l.CipherSymbol != " ").ToList();
        if (lettersOnly.Count == 0) return 0f;
        float correct = lettersOnly.Count(l => l.IsCorrect);
        return correct / lettersOnly.Count;
    }
}