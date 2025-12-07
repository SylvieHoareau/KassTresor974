using UnityEngine;
using System.Collections;

public class PromptUIAnimator : MonoBehaviour
{
    public float animDuration = 0.2f;
    public Vector2 hiddenOffset = new Vector2(0, -40f);

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Vector2 basePos;
    private Coroutine current;

    // On initialise TOUT ici
    void Awake()
    {
        rect = GetComponent<RectTransform>();

        if (rect == null)
        {
            Debug.LogError("[PromptUIAnimator] Aucun RectTransform trouvé sur " + gameObject.name);
            return;
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        basePos = rect.anchoredPosition;

        // CACHÉ dès le début
        rect.anchoredPosition = basePos + hiddenOffset;
        canvasGroup.alpha = 0f;
    }

    public void Show()
    {
        if (rect == null || canvasGroup == null)
            return;

        if (current != null) StopCoroutine(current);
        current = StartCoroutine(Anim(rect.anchoredPosition, basePos, canvasGroup.alpha, 1f));
    }

    public void Hide()
    {
        if (rect == null || canvasGroup == null)
            return;

        if (current != null) StopCoroutine(current);
        current = StartCoroutine(Anim(rect.anchoredPosition, basePos + hiddenOffset, canvasGroup.alpha, 0f));
    }

    private IEnumerator Anim(Vector2 fromPos, Vector2 toPos, float fromA, float toA)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / animDuration;
            float eval = Mathf.SmoothStep(0f, 1f, t);

            rect.anchoredPosition = Vector2.Lerp(fromPos, toPos, eval);
            canvasGroup.alpha = Mathf.Lerp(fromA, toA, eval);

            yield return null;
        }

        rect.anchoredPosition = toPos;
        canvasGroup.alpha = toA;
    }
}
