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

    private Coroutine fadeRoutine;

    void Reset()
    {
        // aide à config rapide si tu ajoutes le script sur un objet qui a des AudioSources
        sources = GetComponents<AudioSource>();
    }

    private void Awake()
    {
        // sécurité : si targetVolumes pas rempli, on prend les volumes actuels comme cibles
        if (sources != null && sources.Length > 0)
        {
            if (targetVolumes == null || targetVolumes.Length != sources.Length)
            {
                targetVolumes = new float[sources.Length];
                for (int i = 0; i < sources.Length; i++)
                    targetVolumes[i] = sources[i] != null ? sources[i].volume : 0f;
            }

            // on démarre silencieux (mais prêts)
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] == null) continue;
                sources[i].loop = true;
                sources[i].playOnAwake = false;
                sources[i].volume = 0f;
                if (!sources[i].isPlaying) sources[i].Play();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        StartFade(toAudible: true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        StartFade(toAudible: false);
    }

    private void StartFade(bool toAudible)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeCoroutine(toAudible));
    }

    private IEnumerator FadeCoroutine(bool toAudible)
    {
        if (sources == null) yield break;

        float duration = Mathf.Max(0.01f, toAudible ? fadeInTime : fadeOutTime);

        float[] startVolumes = new float[sources.Length];
        float[] endVolumes = new float[sources.Length];

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

        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] == null) continue;
            sources[i].volume = endVolumes[i];
        }
    }
}
