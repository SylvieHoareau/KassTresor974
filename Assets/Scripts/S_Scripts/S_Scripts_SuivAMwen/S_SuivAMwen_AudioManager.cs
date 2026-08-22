using UnityEngine;

public class S_SuivAMwen_AudioManager : MonoBehaviour
{

    // Singleton : Premet d'accéder à ce script depuis partout
    public static S_SuivAMwen_AudioManager Instance;

    [Header("Sources Audio")]
    public AudioSource sourceSFX; // Pour les sons courts
    public AudioSource sourceMusic; // Pour l'ambiance de fond

    [Header("Instruments (Ordre des IDs)")]
    // L'ordre doit correspondre à tes boutons : 0=Haut, 1=Droite, 2=Bas, 3=Gauche
    public AudioClip[] clipsInstruments;

    [Header("Sons système")]
    public AudioClip soundVictory; // Quand le joueur réussit
    public AudioClip soundDefeat; // Quand le joueur se trompe

    void Awake()
    {
        // Configuration du Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

   // Fonction pour jouer un instrument selon son ID
   public void JouerInstrument(int id)
   {
        if (id >= 0 && id <clipsInstruments.Length)
        {
            // Stop le son précédent pour éviter les chevauchements désagréables
            sourceSFX.Stop();
            // PlayOneShot permet de superposer les sons sans couper le précédent
            sourceSFX.PlayOneShot(clipsInstruments[id]);
        }
   }

   // Récupère la durée du clip audio en secondes
    public float ObtenirDureeClip(int id)
    {
        if (id >= 0 && id < clipsInstruments.Length && clipsInstruments[id] != null)
        {
            return clipsInstruments[id].length;
        }
        return 0.5f; // Valeur par défaut si le clip est absent
    }

   public void JouerVictoire()
   {
        if (soundVictory != null)
        {
            sourceSFX.Stop();
            sourceSFX.PlayOneShot(soundVictory);
        }
   }

    public void JouerDefaite()
   {
        if (soundDefeat != null)
        {
            sourceSFX.Stop();
            sourceSFX.PlayOneShot(soundDefeat);
        }
   }

   // Coupe tous les effets sonores courts
   public void CouperInstruments()
    {
        // Stoppe tous les sons joués par cette source
        sourceSFX.Stop();
    }

}
