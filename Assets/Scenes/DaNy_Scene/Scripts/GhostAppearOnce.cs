using UnityEngine;
using System.Collections;

public class GhostAppearOnce : MonoBehaviour
{
    [Header("Référence du fantôme (La Buse)")]
    public GameObject ghostObject; // ton perso 3D

    [Header("Timing")]
    public float fadeInDuration = 0.5f;
    public float visibleDuration = 3.0f;
    public float fadeOutDuration = 0.8f;

    [Header("Option: désactiver le trigger après usage")]
    public bool disableTriggerAfterUse = true;

    private bool hasPlayed = false;
    private bool isPlaying = false;

    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;

    private void Awake()
    {
        if (ghostObject == null)
        {
            Debug.LogWarning("GhostAppearOnce: ghostObject non assigné !");
            return;
        }

        renderers = ghostObject.GetComponentsInChildren<Renderer>(true);
        mpb = new MaterialPropertyBlock();

        // état initial : invisible + désactivé
        SetAlpha(0f);
        ghostObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasPlayed || isPlaying) return;
        if (!other.CompareTag("Player")) return;

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        isPlaying = true;
        hasPlayed = true;

        if (ghostObject != null)
            ghostObject.SetActive(true);

        // Fade in
        yield return Fade(0f, 1f, fadeInDuration);

        // Reste visible
        yield return new WaitForSeconds(visibleDuration);

        // Fade out
        yield return Fade(1f, 0f, fadeOutDuration);

        if (ghostObject != null)
            ghostObject.SetActive(false);

        isPlaying = false;

        if (disableTriggerAfterUse)
            gameObject.SetActive(false); // désactive le trigger
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            SetAlpha(to);
            yield break;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float a = Mathf.Lerp(from, to, t);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(to);
    }

    private void SetAlpha(float alpha)
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null) continue;

            r.GetPropertyBlock(mpb);

            // Important: ça ne marche que si ton shader utilise une couleur avec alpha (_Color ou _BaseColor)
            // URP Lit = _BaseColor, Standard = _Color
            if (r.sharedMaterial != null)
            {
                if (r.sharedMaterial.HasProperty("_BaseColor"))
                {
                    Color c = r.sharedMaterial.GetColor("_BaseColor");
                    c.a = alpha;
                    mpb.SetColor("_BaseColor", c);
                }
                else if (r.sharedMaterial.HasProperty("_Color"))
                {
                    Color c = r.sharedMaterial.GetColor("_Color");
                    c.a = alpha;
                    mpb.SetColor("_Color", c);
                }
            }

            r.SetPropertyBlock(mpb);
        }
    }
}
