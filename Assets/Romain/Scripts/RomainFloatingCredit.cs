using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingCreditTMP : MonoBehaviour
{
    [Header("Texte à faire apparaître")]
    public TMP_Text creditText;

    [Header("Paramètres fade")]
    public float fadeDuration = 1f;

    private bool hasAppeared = false;

    private void Start()
    {
        if (creditText != null)
        {
            Color c = creditText.color;
            c.a = 0f;
            creditText.color = c; // invisible au départ
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasAppeared) return;
        if (!other.CompareTag("Player")) return;

        hasAppeared = true;
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        Color baseColor = creditText.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / fadeDuration);

            creditText.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);

            yield return null;
        }

        creditText.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
    }
}
