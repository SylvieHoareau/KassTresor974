using UnityEngine;
using System.Collections; // Ajout indispensable pour les Coroutines
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

    [Header("Settings")]
    [SerializeField] private AudioDatabase audioDatabase;
    [SerializeField] private float fadeDuration = 1.0f; // Durée du fondu en secondes

    private Dictionary<MusicType, Music> musicDict;
    private Dictionary<SFXType, SFX> sfxDict;

    // --- Configuration (Volumes) ---

    // Propriétés pour l'accès et la modification des volumes
    public float MusicVolume
    {
        get => PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        set
        {
            PlayerPrefs.SetFloat("MusicVolume", value); // Sauvegarde immédiate
            musicAudioSource.volume = value;
        }
    }

    public float SFXVolume
    {
        get => PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        set => PlayerPrefs.SetFloat("SFXVolume", value); // Sauvegarde immédiate
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
        }

    }

    private void InitializeAudioSources()
    {
        if (musicAudioSource == null) musicAudioSource = gameObject.AddComponent<AudioSource>();
        if (sfxAudioSource == null) sfxAudioSource = gameObject.AddComponent<AudioSource>();
        
        musicAudioSource.loop = true;
        musicAudioSource.volume = MusicVolume;
    }

    private void InitializeDatabase()
    {
        if (audioDatabase == null)
        {
            Debug.LogError("AudioDatabase non assigné dans l'AudioManager !");
            return;
        }
        
        musicDict = new Dictionary<MusicType, Music>();
        sfxDict = new Dictionary<SFXType, SFX>();

        foreach (var m in audioDatabase.musics) 
            if (m != null) musicDict[m.type] = m;

        foreach (var s in audioDatabase.sfxs) 
            if (s != null) sfxDict[s.type] = s;
    }


    // --- Méthodes publiques pour le Gameplay ---
    /// <summary>
    /// Démarre ou change la lecture d'une musique de fond (BGM)
    /// </summary>
    /// <param name="clip">Clip audio à jouer en boucle</param>
    public void PlayMusic(MusicType type)
    {

        if (!musicDict.ContainsKey(type)) return;

        Music targetMusic = musicDict[type];
        if (musicAudioSource.clip == targetMusic.clip) return; // La musique est déjà en cours de lecture

        StopAllCoroutines(); // Arrête le fondu précédent si un nouveau commence
        StartCoroutine(FadeMusicTransition(targetMusic));
    }

    private IEnumerator FadeMusicTransition(Music newMusic)
    {
        // Fondu sortant
        float startVolume = musicAudioSource.volume;

        // Fondu sortant
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicAudioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        musicAudioSource.clip = newMusic.clip;
        musicAudioSource.loop = newMusic.loop;
        musicAudioSource.Play();

        // Fondu entrant
        float targetVol = newMusic.volume * MusicVolume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicAudioSource.volume = Mathf.Lerp(0, targetVol, t / fadeDuration);
            yield return null;
        }

        musicAudioSource.volume = targetVol;
    }

    /// <summary>
    /// Joue un effet sonore (SFX) une seule fois
    /// Utilise PlayOneShot pour permettre la superposition des sons
    /// </summary>
    /// <param name="clip">Clip audio à jouer</param>
    public void PlaySFX(SFXType type)
    {
        if (!sfxDict.ContainsKey(type)) return;
        SFX sfx = sfxDict[type];

        // On applique le volume spécifique du SFX Multiplié par le volume global
        float finalVolume = sfx.volume * SFXVolume;
        float finalPitch = sfx.useRandomPitch ? sfx.pitch + Random.Range(-0.1f, 0.1f) : sfx.pitch;
        
        sfxAudioSource.pitch = finalPitch;
        sfxAudioSource.PlayOneShot(sfx.clip, finalVolume);
    }

    // Méthode pour jouer un SFX à une position 3D spécifique
    // public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float spatialBlend = 1f)
    // {
    //     // Crée un AudioSource temporaire sur place pour les sons 3D
    //     GameObject tempAudioObject = new GameObject("TempSFX_3D");
    //     tempAudioObject.transform.position = position;
    //     AudioSource tempSource = tempAudioObject.AddComponent<AudioSource>();
        
    //     tempSource.clip = clip;
    //     tempSource.spatialBlend = spatialBlend; // 1 = 3D,
    //     tempSource.volume = SFXVolume;
    //     tempSource.Play();

    //     Destroy(tempAudioObject, clip.length); // Détruit après la lecture
    // }
}
