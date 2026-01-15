using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems; 

public class CipherSolver : MonoBehaviour
{
    // --- VARIABLES PUBLIQUES ---
    public TextMeshProUGUI cipherTextDisplay; // Référence au composant TextMeshProUGUI pour afficher le texte chiffré

    [Header("Messages de fin")]
    [Tooltip("Message affiché lorsque le joueur réussit à déchiffrer le message")]
    public string winMessage = "Bravo ! Vous avez trouvé le trésor de la Buse !";

    // --- VARIABLES PRIVÉES ---
    private const string SecretPhrase = "LE TRESOR EST SOUS LA ROCHE";
    private string CipherAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    // private string SubstitutionAlphabet = "QWERTYUIOPASDFGHJKLZXCVBNM"; // Exemple de substitution
    private string SubstitutionKey; // La clé de substitution générée
    private Dictionary<char, char> playerSubstitutions = new Dictionary<char, char>();
    private char currentlySelectedCipherLetter = '\0'; // La lettre chiffrée que 
    public GameObject victoryPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateSubstitutionKey(); // Crée la clé de substitution
        InitializePlayerSubstitutions(); // Initialise les substitutions du joueur
        EncryptAndDisplay(); // Chiffre la phrase secrète et l'affiche
    }

    /// <summary>
    /// Génère une clé de substitution aléatoire (un alphabet mélangé)
    /// </summary>
    public void GenerateSubstitutionKey()
    {
        // Ici, on pourrait implémenter une logique pour générer une clé de substitution aléatoire
        // Pour l'instant, on utilise une clé fixe définie dans SubstitutionAlphabet
        List<char> shuffledAlphabet = new List<char>(CipherAlphabet.ToCharArray());

        // Mélanger la liste (algorithme de Fisher-Yates)
        System.Random rng = new System.Random();
        int n = shuffledAlphabet.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            char temp = shuffledAlphabet[n];
            shuffledAlphabet[n] = shuffledAlphabet[k];
            shuffledAlphabet[k] = temp;
        }

        // Convertir la liste mélangée en une chaîne pour la clé
        SubstitutionKey = new string(shuffledAlphabet.ToArray());

        Debug.Log("Clé de Substitution Générée : " + SubstitutionKey);
    }

    /// <summary>
    /// Initialise la grille de substitution du joueur avec des espaces pour commenter
    /// </summary>
    public void InitializePlayerSubstitutions()
    {
        // Pour chaque lettre de l'alphabet standard, le joueur n'a encore rien découvert
        foreach (char c in CipherAlphabet)
        {
            playerSubstitutions.Add(c, '\0'); // Initialise toutes les substitutions à '_'
        }
    }

    // -------------------------------------------------
    // LOGIQUE DE JEU
    // -------------------------------------------------

    /// <summary>
    /// Chiffre la phrase secrète en utilisant la clé de substitution et l'affiche
    /// </summary>
    /// <param name="phrase">La phrase à chiffrer</param>
    /// <returns>La phrase chiffrée</returns>
    public string EncryptPhrase(string phrase)
    {
        StringBuilder encrypted = new StringBuilder();

        foreach (char c in phrase.ToUpper())
        {
           if (char.IsLetter(c))
            {
                // Trouver l'index de la lettre dans l'alphabet standard
                int index = CipherAlphabet.IndexOf(c);
                
                // Si la lettre est trouvée, utiliser la lettre correspondante de la clé chiffrée
                if (index >= 0)
                {
                    encrypted.Append(SubstitutionKey[index]);
                }
            }
            else if (c == ' ')
            {
                // Laisser les espaces et autres symboles tels quels
                encrypted.Append(' ');
            }
        }
        return encrypted.ToString();
    }

    /// <summary>
    /// Affiche l'état actuel de la résolution (mélange de chiffré et de résolu)
    /// </summary>
    public void EncryptAndDisplay()
    {
        StringBuilder displayText = new StringBuilder();
        string encryptedPhrase = EncryptPhrase(SecretPhrase);

        foreach (char cipherChar in encryptedPhrase)
        {
            if (char.IsLetter(cipherChar))
            {
               // Si la lettre chiffrée a été substituée par le joueur
                if (playerSubstitutions.ContainsKey(cipherChar) && playerSubstitutions[cipherChar] != '\0')
                {
                    char playerChar = playerSubstitutions[cipherChar];

                   // 1. Trouver où se trouve la lettre chiffrée dans la clé
                    int indexInKey = SubstitutionKey.IndexOf(cipherChar);
                    // 2. La lettre originale est celle au même index dans l'alphabet normal
                    char realOriginalLetter = CipherAlphabet[indexInKey];

                    // Comparaison simple : est-ce que le joueur a deviné la bonne lettre originale ?
                    string color = (playerChar == realOriginalLetter) ? "#33FF33" : "#FF5555";

                    // Afficher la substitution choisie par le joueur
                    displayText.Append($"<color={color}>{playerChar}</color>");
                }
                else
                {
                    // Afficher la lettre chiffrée (avec un style cliquable)
                    // Nous allons encapsuler chaque lettre chiffrée dans un lien TextMeshPro cliquable
                    displayText.Append($"<link=\"{cipherChar}\"><color=#FFFFFF>{cipherChar}</color></link>");
                }
            }
            else
            {
                // Afficher l'espace ou la ponctuation
                displayText.Append(cipherChar);
            }
        }
        cipherTextDisplay.text = displayText.ToString();
        CheckWinCondition(); // Vérifier la victoire après chaque mise à jour de l'affichage
    }

    /// <summary>
    /// Vérifie si la phrase entière a été correctement déchiffrée
    /// </summary>
    private void CheckWinCondition()
    {
        // Reconstruite la phrase décryptée basée sur les substitutions du joueur
        // StringBuilder solved = new StringBuilder();
        string encrypted = EncryptPhrase(SecretPhrase);

        StringBuilder solved = new StringBuilder();

        // On vérifie si toutes les lettres ont été correctement substituées
        foreach (char cipherChar in encrypted)
        {
            if (char.IsLetter(cipherChar))
            {
                // Si une lettre n'est pas encore résolue -> pas de victoire
                if (playerSubstitutions[cipherChar] == '\0')
                    return;

                solved.Append(playerSubstitutions[cipherChar]);
            }
            else
            {
                solved.Append(cipherChar); // Ajouter les espaces et ponctuations tels quels
            }
        }

        // Comparaison finale (on enlève les espaces pour une comparaison robuste, 
        // car la mise en forme de la phrase secrète peut varier légèrement)
        if (solved.ToString().Replace(" ", "") == SecretPhrase.Replace(" ", ""))
        {
            Debug.Log("PHRASE VALIDEE !");
            // AJOUTER ICI une logique de fin de jeu (écran de victoire, désactiver l'interaction, etc.)
            OnWin();
        }
    }

    private void OnWin()
    {
        // Empêche plusieurs validations successives
        if (victoryPanel != null && !victoryPanel.activeSelf)
        {
            victoryPanel.SetActive(true);
        }
    }


      // --- LOGIQUE D'INTERACTION DU JOUEUR ---
    /// <summary>
    /// Gère le clic du joueur sur une lettre du texte chiffré
    /// </summary>
    public void OnClickCipherLetter(BaseEventData eventData)
    {
        PointerEventData pointerData = eventData as PointerEventData;
        if (pointerData == null) return;

        // Lire le lien TextMeshPro cliqué
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(cipherTextDisplay, Input.mousePosition, null);
        
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = cipherTextDisplay.textInfo.linkInfo[linkIndex];

            // Le nom du lien est la lettre chiffrée
            string linkID = linkInfo.GetLinkID();

            // Mettre à jour la lettre actuellement sélectionnée
            currentlySelectedCipherLetter = linkID[0];

            Debug.Log($"Lettre chiffrée sélectionnée : {currentlySelectedCipherLetter}");
        }
    } 

    public void OnClickCipherLetterProxy()
    {
        // Simule un PointerEventData reçu par l’EventTrigger
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        OnClickCipherLetter(pointerData);
    }

    /// <summary>
    /// Gère le clic du joueur sur un bouton du clavier de substitution
    /// </summary>
    /// <param name="substitutionChar">La lettre choisie par le joueur</param>
    public void OnClickKeyboardLetter(string substitutionChar)
    {
        if (currentlySelectedCipherLetter != '\0')
        {
            char subChar = substitutionChar.ToUpper()[0];

            // Vérifier si cette lettre est déjà utilisée
            if (playerSubstitutions.ContainsValue(subChar))
            {
                Debug.LogWarning($"La lettre {subChar} est déjà utilisée. Réinitialisation de l'ancienne affectation.");
                // Retirer l'ancienne affectation (si le joueur change d'idée)
                foreach(KeyValuePair<char, char> entry in playerSubstitutions)
                {
                    if (entry.Value == subChar)
                    {
                        // On réinitialise l'ancienne lettre chiffrée à '\0'
                        playerSubstitutions[entry.Key] = '\0'; 
                        break;
                    }
                }
            }

            // Enregistrer la substitution du joueur
            playerSubstitutions[currentlySelectedCipherLetter] = subChar;

            // Mettre à jour l'affichage
            EncryptAndDisplay();

            // Réinitialiser la sélection
            currentlySelectedCipherLetter = '\0';

            Debug.Log($"Substitution: {currentlySelectedCipherLetter} remplacé par {subChar}");
            CheckWinCondition();
        }
        else
        {
            Debug.LogWarning("Veuillez d'abord cliquer sur une lettre chiffrée à remplacer dans le message.");
        }
    }

}
