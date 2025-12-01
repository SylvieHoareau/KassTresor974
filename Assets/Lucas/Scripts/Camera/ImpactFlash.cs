using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ImpactFlash : MonoBehaviour
{
    public static ImpactFlash Instance;

    private Image flashImage;
    private Coroutine flashRoutine;

    [Header("Timings")]
    public float flashDuration = 0.1f;

    [Header("Colors")]
    public float whiteMaxAlpha = 0.5f;
    public float redMaxAlpha = 0.6f;

    void Awake()
    {
        Instance = this;
        flashImage = GetComponent<Image>();
    }

    // ----------------------------------------
    // FLASH BLANC = HIT RÉUSSI
    // ----------------------------------------
    public void FlashWhite()
    {
        StartColorFlash(Color.white, whiteMaxAlpha);
    }

    // ----------------------------------------
    // FLASH ROUGE = DÉGÂTS OU ERREUR
    // ----------------------------------------
    public void FlashRed()
    {
        StartColorFlash(Color.red, redMaxAlpha);
    }

    private void StartColorFlash(Color baseColor, float maxAlpha)
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashEffect(baseColor, maxAlpha));
    }

    private IEnumerator FlashEffect(Color baseColor, float maxAlpha)
    {
        float upDuration = flashDuration * 0.3f;
        float downDuration = flashDuration * 0.7f;

        // ====== MONTÉE ======
        float t = 0f;
        while (t < upDuration)
        {
            float a = Mathf.Lerp(0, maxAlpha, t / upDuration);
            flashImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // ====== DESCENTE ======
        t = 0f;
        while (t < downDuration)
        {
            float a = Mathf.Lerp(maxAlpha, 0, t / downDuration);
            flashImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        flashImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0);
        flashRoutine = null;
    }
}
