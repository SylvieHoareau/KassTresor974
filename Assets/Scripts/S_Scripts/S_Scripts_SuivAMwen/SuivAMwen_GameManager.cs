using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SuivAMwen_GameManager : MonoBehaviour
{
    [Header("Configuration")]
    public float vitesseSequence = 1.0f; // Temps entre chaque indication

    [Header("Références UI")]
    public List<S_InstrumentButton> boutonsInstruments; // Liste de nos 4 boutons
    public TextMeshProUGUI messageText; // Pour afficher "Ecoutez..." ou "A vous !"
    public GameObject boutonRejouer; // Bouton pour rejouer après une défaite

    // Variables internes
    private List<int> sequenceDeJeu = new List<int>(); // La séquence à mémoriser
    private int indexJoueur = 0; // Où en est le joueur dans la séquence actuelle
    private bool tourDuJoueur = false; // Est-ce au joueur de joueur ?

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // On commence une nouvelle partie
        DemarreNouvellePartie();
    }

    void DemarreNouvellePartie()
    {
        sequenceDeJeu.Clear();
        indexJoueur = 0;
        StartCoroutine(LancerProchainTour());
    }

    // Ajoute une étape et joue la séquence
    IEnumerator LancerProchainTour()
    {
        // Logique d'accélération
        vitesseSequence = Mathf.Max(0.2f, vitesseSequence - 0.05f); // Accélère légèrement la séquence

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
            // Coupe l'éventuel son en cours avant de jouer le suivant
            S_SuivAMwen_AudioManager.Instance.CouperInstruments();

            // Joue le son de l'instrument
            S_SuivAMwen_AudioManager.Instance.JouerInstrument(indexInstrument);

            // Active visuellement et sonorement le bouton correspondant
            boutonsInstruments[indexInstrument].ActiverBoutonAutomatiquement();
            
            // Met le jeu en pause pour la durée définie par vitesseSequence
            yield return new WaitForSeconds(vitesseSequence);
        }

        messageText.text = "Té suiv' à mwen ! (A toi)";
        tourDuJoueur = true;
    }

    // Cette fonction est appelée par les boutons quand le joueur clique
    public void TraiterInputJoueur(int idButton)
    {
        // Le joueur ne peut cliquer QUE pendant son tour
        if (!tourDuJoueur) return; 

        // Vérification
        if (idButton != sequenceDeJeu[indexJoueur])
        {
            // Mauvaise note
            GameOver();
            return;
        }

        // C'est correct !
        indexJoueur++;

        // Si on a fini toute la séquences actuelle
        if (indexJoueur >= sequenceDeJeu.Count)
        {
            tourDuJoueur = false;
            messageText.text = "Gayar ! (Bravo)";
            S_SuivAMwen_AudioManager.Instance.JouerVictoire();
            StartCoroutine(LancerProchainTour()); // On lance la suite
        }
    }

    // Fonction appelée par le bouton "Rejouer"
    public void RejouerPartie()
    {

        // Cacher le bouton avant de commencer
        if (boutonRejouer != null)
        {
            boutonRejouer.SetActive(false);
        }

        // Réinitialiser la vitesse au niveau de départ (1.0f)
        vitesseSequence = 1.0f;

        // Relancer la partie
        DemarreNouvellePartie();
    }

    // Gérer l'état de défaite
    void GameOver()
    {
        tourDuJoueur = false;
        messageText.text = "Aie aie aie... Perdu !";

        // Jouer le son de défaite
        S_SuivAMwen_AudioManager.Instance.JouerDefaite();

        // Afficher la bouton Rejouer
        if (boutonRejouer != null)
        {
            boutonRejouer.SetActive(true);
        }
    }
}
