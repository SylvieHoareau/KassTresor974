using System.Collections;
using UnityEngine;

public class AudioZoneFade : MonoBehaviour
{
    [Header("Cibles audio à contrôler (sur Cimetiere_Ambience ou Grotte_Ambience)")]
    public AudioSource[] sources;

    [Header("Volumes")]
    [Tooltip("Volumes cibles (même taille que sources). Exemple: pluie 0.4, ocean 0.25")]
    public float[] targetVolumes;

    [Header("Fade")]
    public float fadeInTime = 1.0f;
    public float fadeOutTime = 1.0f;

    [Header("Déclenchement")]
    public string playerTag = "Player";

    [Header("Comportement")]
    [Tooltip("Si vrai, Stop() les sources après un fade-out (recommandé pour perf).")]
    public bool stopOnFadeOut = true;

    private Coroutine fadeRoutine;

    // Anti-spam si OnTriggerEnter/Exit se déclenche plusieurs fois
    private int playerInsideCount = 0;
    private bool isAudible = false;

    // Buffers réutilisés (évite allocations/GC)
    private float[] startVolumes;
    private float[] endVolumes;

    void Reset()
    {
        sources = GetComponents<AudioSource>();
    }

    private void Awake()
    {
        if (sources == null || sources.Length == 0)
            return;

        // Si targetVolumes pas OK, on prend les volumes actuels comme cibles
        if (targetVolumes == null || targetVolumes.Length != sources.Length)
        {
            targetVolumes = new float[sources.Length];
            for (int i = 0; i < sources.Length; i++)
                targetVolumes[i] = sources[i] != null ? sources[i].volume : 0f;
        }

        // Prépare les buffers
        startVolumes = new float[sources.Length];
        endVolumes = new float[sources.Length];

        // Prépare les sources : silencieuses et stoppées (pas de Play() en fond)
        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] == null) continue;

            sources[i].loop = true;
            sources[i].playOnAwake = false;
            sources[i].volume = 0f;

            if (sources[i].isPlaying)
                sources[i].Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInsideCount++;
        if (playerInsideCount > 1) return; // déjà dedans via un autre collider

        // Fade in uniquement si on n'est pas déjà audible
        if (!isAudible)
        {
            isAudible = true;
            StartFade(toAudible: true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInsideCount = Mathf.Max(0, playerInsideCount - 1);
        if (playerInsideCount > 0) return; // encore dedans via un autre collider

        // Fade out uniquement si on était audible
        if (isAudible)
        {
            isAudible = false;
            StartFade(toAudible: false);
        }
    }

    private void StartFade(bool toAudible)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeCoroutine(toAudible));
    }

    private IEnumerator FadeCoroutine(bool toAudible)
    {
        if (sources == null || sources.Length == 0)
            yield break;

        float duration = Mathf.Max(0.01f, toAudible ? fadeInTime : fadeOutTime);

        // Si on fade-in, on Play avant de monter le volume
        if (toAudible)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] == null) continue;
                if (!sources[i].isPlaying)
                    sources[i].Play();
            }
        }

        // Cache volumes départ/arrivée dans buffers (pas de new)
        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] == null) continue;

            startVolumes[i] = sources[i].volume;
            endVolumes[i] = toAudible ? targetVolumes[i] : 0f;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] == null) continue;
                sources[i].volume = Mathf.Lerp(startVolumes[i], endVolumes[i], k);
            }

            yield return null;
        }

        // Snap final
        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] == null) continue;
            sources[i].volume = endVolumes[i];
        }

        // Si fade-out terminé : Stop pour économiser CPU audio
        if (!toAudible && stopOnFadeOut)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] == null) continue;
                if (sources[i].isPlaying)
                    sources[i].Stop();
            }
        }

        fadeRoutine = null;
    }
}
