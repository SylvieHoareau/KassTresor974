using UnityEngine;
using TMPro;
using System.Collections;

public class RomainCreditsFader : MonoBehaviour
{
    [Header("Liste des crédits à gérer (tous tes TMP ici)")]
    public TMP_Text[] credits;

    [Header("Durées")]
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 0.6f;

    [Header("Option")]
    [Tooltip("Si vrai: fade out + fade in en même temps. Sinon: fade out puis fade in.")]
    public bool crossFade = true;

    [Tooltip("Si vrai, les crédits sont désactivés (SetActive false) au lancement.")]
    public bool disableOnStart = true;

    private TMP_Text current;
    private Coroutine transitionCo;

    private void Start()
    {
        // Cache tout au lancement
        if (credits != null)
        {
            for (int i = 0; i < credits.Length; i++)
            {
                if (credits[i] == null) continue;
                SetAlpha(credits[i], 0f);
                if (disableOnStart)
                    credits[i].gameObject.SetActive(false);
            }
        }

        current = null;
    }

    public void TriggerCredit(TMP_Text next)
    {
        if (next == null) return;
        if (current == next) return;

        if (transitionCo != null)
            StopCoroutine(transitionCo);

        transitionCo = StartCoroutine(TransitionTo(next));
    }

    private IEnumerator TransitionTo(TMP_Text next)
    {
        // Prépare le prochain
        SetAlpha(next, 0f);
        next.gameObject.SetActive(true);

        TMP_Text previous = current;
        current = next;

        if (crossFade)
        {
            float t = 0f;
            float prevStartA = previous ? previous.color.a : 0f;

            float maxT = Mathf.Max(fadeInDuration, fadeOutDuration);
            if (maxT <= 0f) maxT = 0.0001f;

            while (t < maxT)
            {
                t += Time.deltaTime;

                // Fade in
                float inA = fadeInDuration <= 0f ? 1f : Mathf.Clamp01(t / fadeInDuration);
                SetAlpha(next, inA);

                // Fade out
                if (previous != null)
                {
                    float outT = fadeOutDuration <= 0f ? 1f : Mathf.Clamp01(t / fadeOutDuration);
                    float outA = Mathf.Lerp(prevStartA, 0f, outT);
                    SetAlpha(previous, outA);
                }

                yield return null;
            }

            SetAlpha(next, 1f);

            if (previous != null)
            {
                SetAlpha(previous, 0f);
                previous.gameObject.SetActive(false);
            }
        }
        else
        {
            // Fade out previous puis fade in next
            if (previous != null)
            {
                yield return Fade(previous, previous.color.a, 0f, fadeOutDuration);
                SetAlpha(previous, 0f);
                previous.gameObject.SetActive(false);
            }

            yield return Fade(next, 0f, 1f, fadeInDuration);
            SetAlpha(next, 1f);
        }

        transitionCo = null;
    }

    private IEnumerator Fade(TMP_Text txt, float from, float to, float duration)
    {
        if (txt == null) yield break;

        if (duration <= 0f)
        {
            SetAlpha(txt, to);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            SetAlpha(txt, a);
            yield return null;
        }

        SetAlpha(txt, to);
    }

    private void SetAlpha(TMP_Text txt, float a)
    {
        if (txt == null) return;
        Color c = txt.color;
        c.a = a;
        txt.color = c;
    }
}
