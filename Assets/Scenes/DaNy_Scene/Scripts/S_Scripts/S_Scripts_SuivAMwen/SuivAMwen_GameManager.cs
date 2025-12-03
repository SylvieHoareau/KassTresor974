using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuivAMwen_GameManager : MonoBehaviour
{
    [Header("Configuration")]
    public float vitesseSequence = 1.0f; // Temps entre chaque indication

    [Header("Références UI")]
    public List<S_InstrumentButton> boutonsInstruments; // Liste de nos 4 boutons
    public Text messageText; // Pour afficher "Ecoutez..." ou "A vous !"

    // Variables internes
    private List<int> sequenceDeJeu = new List<int>(); // La séquence à mémoriser
    private int indexJoueur = 0; // Où en est le joueur dans la séquence actuelle
    private bool tourDuJoueur = false; // Est-ce au joueur de joueur ?

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // On commence une nouvelle partie
    }

    void DemarreNouvellePartie()
    {
        sequenceDeJeu.Clear();
        indexJoueur = 0;
        StartCoroutine(LancerProchainTour());
    }

    // Ajoute une étéape et joue la séquence
    IEnumerator LancerProchainTour()
    {
        tourDuJoueur = false;
        indexJoueur = 0;
        messageText.text = "Ecoute bien la musique...";

        yield return new WaitForSeconds(1f);

        // Ajoute une direction aléatorie (0 à 3)
        // 0=Haut, 1=Droite, 2=Bas, 3=Gauche
        sequenceDeJeu.Add(Random.Range(0, 4));

        // Joue la séquence pour le joueur
        foreach (int indexInstrument in sequenceDeJeu)
        {
            // Active visuellement et sonorement le bouton correspondant
            boutonsInstruments[indexInstrument].ActiverBoutonAutomatiquement();
            yield return new WaitForSeconds(vitesseSequence);
        }

        messageText.text = "Té suiv' à mwen ! (A toi)";
        tourDuJoueur = true;
    }

    // Cette fonction est appelée par les boutons quand le joueur clique
    public void TraiterInputJoueur(int idButton)
    {
        if (tourDuJoueur) return; // Si ce n'est pas le tour du joueur, on ignore

        // Vérification
        if (idButton == sequenceDeJeu[indexJoueur])
        {
            // C'est correct !
            indexJoueur++;

            // Si on a fini toute la séquences actuelle
            if (indexJoueur >= sequenceDeJeu.Count)
            {
                messageText.text = "Gayar ! (Bravo)";
                StartCoroutine(LancerProchainTour()); // On lance la suite
            }
            else
            {
                // Erreur
                GameOver();
            }
        }
    }

    // Update is called once per frame
    void GameOver()
    {
        tourDuJoueur = false;
        messageText.text = "Aie aie aie... Perdu !";
        // Ajouter un bouton pour recommencer

    }
}
