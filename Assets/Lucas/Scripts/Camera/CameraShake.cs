using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 originalPos;
    private Coroutine shakeCoroutine;  // 🔥 Pour pouvoir l'arrêter

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        originalPos = transform.localPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        // Si un shake tourne déjà, on l’arrête :
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // ⚠️ SI LE JOUEUR EST MORT → on arrête immédiatement
            if (!PlayerIsAlive())
                break;

            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        shakeCoroutine = null;
    }

    // 🔥 On vérifie l’état du joueur
    private bool PlayerIsAlive()
    {
        return FindObjectOfType<HealthCock>().isAlive;
    }

    // 🔥 Fonction pour éteindre tous les shakes
    public void StopShake()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = null;
        transform.localPosition = originalPos;
    }
}