using UnityEngine;
using System.Collections.Generic;

// Utilisation d'un attribut pour forcer l'ajout de composants AudioSource
// si le script est ajouté à un GameObject sans eux
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    // Singleton pour un accès facile depuis d'autres scripts
    public static AudioManager Instance { get; private set; }

    // AudioSource pour jouer la musique de fond (BGM - Background Music)
    [Header("Audio Sources")]
    [Tooltip("Source pour la musique de fond (BGM)")]
    [SerializeField] private AudioSource musicAudioSource;
    // AudioSource pour jouer les effets sonores (SFX - Sound Effects)
    [Tooltip("Source pour les effets sonores (SFX)")]
    [SerializeField] private AudioSource sfxAudioSource;

    [SerializeField] private AudioDatabaseSO audioDatabase;

    private Dictionary<MusicType, AudioClip> musicDict;
    private Dictionary<SFXType, AudioClip> sfxDict;

    // --- Configuration (Volumes) ---

    // Clés pour la sauvegarde des volumes dans les PlayerPrefs
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";

    // Propriétés pour l'accès et la modification des volumes
    public float MusicVolume
    {
        get => musicAudioSource.volume;
        set
        {
            musicAudioSource.volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicVolumeKey, musicAudioSource.volume); // Sauvegarde immédiate
        }
    }

    public float SFXVolume
    {
        get => sfxAudioSource.volume;
        set
        {
            sfxAudioSource.volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SFXVolumeKey, sfxAudioSource.volume); // Sauvegarde immédiate
        }
    }

    // --- Initialisation ---
    private void Awake()
    {
        // Mise en place du singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre les scènes
            InitializeAudioSources();
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject); // Évite les doublons
            return;
        }

        // Chargement des volumes sauvegardés ou réglage par défaut
        musicAudioSource.volume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.5f); // Par défaut à 0.5
        sfxAudioSource.volume = PlayerPrefs.GetFloat(SFXVolumeKey, 0.5f); // Par défaut à 0.5


    }

    private void InitializeDatabase()
    {
         if (audioDatabase == null)
        {
            Debug.LogError("AudioDatabaseSO non assigné dans l'AudioManager !");
            return;
        }
        
        musicDict = new Dictionary<MusicType, AudioClip>();
        sfxDict = new Dictionary<SFXType, AudioClip>();

        foreach (var music in audioDatabase.musics)
        {
            if (!musicDict.ContainsKey(music.type))
                musicDict.Add(music.type, music.clip);
        }

        foreach (var sfx in audioDatabase.sfxs)
        {
            if (!sfxDict.ContainsKey(sfx.type))
                sfxDict.Add(sfx.type, sfx.clip);
        }
    }

    private void InitializeAudioSources()
    {
        // Assure que les AudioSources sont assignées
        if (musicAudioSource == null)
        {
            musicAudioSource = gameObject.AddComponent<AudioSource>();
            musicAudioSource.loop = true; // La musique de fond doit boucler
        }
        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // Charge les volumes sauvegardés, sinon utilise la valeur par défaut
        musicAudioSource.volume = PlayerPrefs.GetFloat(MusicVolumeKey, musicAudioSource.volume);
        sfxAudioSource.volume = PlayerPrefs.GetFloat(SFXVolumeKey, sfxAudioSource.volume);

        // Configuration MusicSource : Loop par défait pour la musique
        musicAudioSource.loop = true;

        // Configuration SFXSource : ne doit pas boucler
        sfxAudioSource.loop = false;
    }

    // --- Méthodes publiques pour le Gameplay ---
    /// <summary>
    /// Démarre ou change la lecture d'une musique de fond (BGM)
    /// </summary>
    /// <param name="clip">Clip audio à jouer en boucle</param>
    public void PlayMusic(MusicType type)
    {

        if (!musicDict.ContainsKey(type)) return;

        AudioClip clip = musicDict[type];

        if (musicAudioSource.clip == clip && musicAudioSource.isPlaying)
            return; // La musique est déjà en cours de lecture

        musicAudioSource.clip = clip;
        musicAudioSource.Play();
    }

    /// <summary>
    /// Joue un effet sonore (SFX) une seule fois
    /// Utilise PlayOneShot pour permettre la superposition des sons
    /// </summary>
    /// <param name="clip">Clip audio à jouer</param>
    public void PlaySFX(SFXType type)
    {
        if (!sfxDict.ContainsKey(type)) return;
        sfxAudioSource.PlayOneShot(sfxDict[type], SFXVolume);
    }

    // Méthode pour jouer un SFX à une position 3D spécifique
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float spatialBlend = 1f)
    {
        // Crée un AudioSource temporaire sur place pour les sons 3D
        GameObject tempAudioObject = new GameObject("TempSFX_3D");
        tempAudioObject.transform.position = position;
        AudioSource tempSource = tempAudioObject.AddComponent<AudioSource>();
        
        tempSource.clip = clip;
        tempSource.spatialBlend = spatialBlend; // 1 = 3D,
        tempSource.volume = SFXVolume;
        tempSource.Play();

        Destroy(tempAudioObject, clip.length); // Détruit après la lecture
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
