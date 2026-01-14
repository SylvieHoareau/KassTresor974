using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCipherKey", menuName = "Game/Cryptogram/Cipher Key")]
public class CipherMapping : ScriptableObject
{
    [System.Serializable]
    public class SymbolToLetterPair
    {
        // Le symbole chiffré tel qu'il apparaît dans le message
        public string CipherSymbol;

        // La lettre claire correspondante (la solution)
        public char SolutionLetter;
    }

    [Tooltip("Liste des paires symbole chiffré - lettre claire")]
    public List<SymbolToLetterPair> KeyPairs = new List<SymbolToLetterPair>();

    // Dictionnaire pour un accès rapide en runtime
    private Dictionary<string, char> _solutionMap;

    public Dictionary<string, char> GetSolutionMap()
    {
        if (_solutionMap == null)
        {
            _solutionMap = new Dictionary<string, char>();
            foreach (var pair in KeyPairs)
            {
                // Utiliser la version normalisée/majuscule pour la solution
                _solutionMap[pair.CipherSymbol] = char.ToUpper(pair.SolutionLetter);
            }
        }
        return _solutionMap;
    }
}
