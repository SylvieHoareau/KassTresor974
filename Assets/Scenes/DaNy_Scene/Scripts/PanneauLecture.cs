using UnityEngine;
using System.Collections;

public class PanneauLecture : MonoBehaviour
{
    [Header("UI")]
    public GameObject uiPanneau;              // Canvas plein écran
    public PromptUIAnimator promptUI;         // "Appuyez sur E pour lire" (avec anim)

    [Header("Contrôle joueur")]
    public MonoBehaviour playerController;    // déplacement joueur
    public MonoBehaviour playerCameraLook;    // rotation caméra (script souris / 3e pers)

    [Header("Animation panneau")]
    public float animDuration = 0.25f;        // durée fade/zoom
    public Vector3 startScale = new Vector3(0.8f, 0.8f, 1f);
    public AnimationCurve animCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool playerInRange = false;
    private bool isOpen = false;
    private bool isAnimating = false;

    private CanvasGroup panelCanvasGroup;
    private RectTransform panelRect;

    private void Start()
    {
        if (uiPanneau != null)
        {
            panelCanvasGroup = uiPanneau.GetComponent<CanvasGroup>();
            panelRect = uiPanneau.GetComponent<RectTransform>();

            if (panelCanvasGroup == null)
                panelCanvasGroup = uiPanneau.AddComponent<CanvasGroup>();

            // état initial : caché et réduit
            panelCanvasGroup.alpha = 0f;
            if (panelRect != null)
                panelRect.localScale = startScale;

            uiPanneau.SetActive(false);
        }

        // prompt caché (avec son anim)
        if (promptUI != null)
            promptUI.Hide();
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isOpen)
                OuvrirPanneau();
            else
                FermerPanneau();
        }

        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            FermerPanneau();
        }
    }

    private void OuvrirPanneau()
    {
        if (isAnimating || uiPanneau == null || panelCanvasGroup == null || panelRect == null)
            return;

        isOpen = true;

        // cacher le prompt avec anim
        if (promptUI != null)
            promptUI.Hide();

        // bloquer mouvement + rotation caméra
        if (playerController != null)
            playerController.enabled = false;
        if (playerCameraLook != null)
            playerCameraLook.enabled = false;

        // souris visible
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        StartCoroutine(AnimOpen());
    }

    private void FermerPanneau()
    {
        if (isAnimating || uiPanneau == null || panelCanvasGroup == null || panelRect == null)
            return;

        isOpen = false;

        // réactiver mouvement + rotation
        if (playerController != null)
            playerController.enabled = true;
        if (playerCameraLook != null)
            playerCameraLook.enabled = true;

        // remettre prompt si joueur présent (avec anim)
        if (playerInRange && promptUI != null)
            promptUI.Show();
        else if (promptUI != null)
            promptUI.Hide();

        // rebloquer souris FPS / 3e pers
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        StartCoroutine(AnimClose());
    }

    private IEnumerator AnimOpen()
    {
        isAnimating = true;

        uiPanneau.SetActive(true);
        panelCanvasGroup.alpha = 0f;
        panelRect.localScale = startScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / animDuration;
            float eval = animCurve.Evaluate(t);

            panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, eval);
            panelRect.localScale = Vector3.Lerp(startScale, Vector3.one, eval);

            yield return null;
        }

        panelCanvasGroup.alpha = 1f;
        panelRect.localScale = Vector3.one;
        isAnimating = false;
    }

    private IEnumerator AnimClose()
    {
        isAnimating = true;

        float startAlpha = panelCanvasGroup.alpha;
        Vector3 startScaleLocal = panelRect.localScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / animDuration;
            float eval = animCurve.Evaluate(t);

            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, eval);
            panelRect.localScale = Vector3.Lerp(startScaleLocal, startScale, eval);

            yield return null;
        }

        panelCanvasGroup.alpha = 0f;
        panelRect.localScale = startScale;
        uiPanneau.SetActive(false);

        isAnimating = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (!isOpen && !isAnimating && promptUI != null)
                promptUI.Show();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (promptUI != null)
                promptUI.Hide();

            if (isOpen)
                FermerPanneau();
        }
    }
}
