using UnityEngine;

public class CipherLetter
{
    // Le symbole chiffré orginal (ex: 'Γ', 'V', '<')
    public readonly string CipherSymbol;

    // La lettre que le joueur a assigné à ce symbole (ex: 'T')
    public char PlayerGuess { get; set; }

    // La solution réelle (pour vérification)
    public readonly char SolutionLetter;

    public CipherLetter(string cipherSymbol, char solutionLetter)
    {
        this.CipherSymbol = cipherSymbol;
        this.SolutionLetter = char.ToUpper(solutionLetter);
    }

    // Propriété de vérification pour le feedback immédiat
    public bool IsCorrect => PlayerGuess != ' ' && char.ToUpper(PlayerGuess) == SolutionLetter;

    // Propriété pour vérifier si la lettre a été devinée
    public bool IsDecrypted => PlayerGuess != ' ';
}
