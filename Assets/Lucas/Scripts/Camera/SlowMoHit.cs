using UnityEngine;
using System.Collections;

public class HitSlowMotion : MonoBehaviour
{
    public static HitSlowMotion Instance;

    [Header("Slow-motion settings")]
    public float slowScale = 0.2f;      // À quel point ralentir
    public float slowDuration = 0.08f;  // Temps réel du ralentissement

    private bool isSlowing = false;

    void Awake()
    {
        Instance = this;
    }

    public void DoSlowMotion()
    {
        if (!isSlowing)
            StartCoroutine(SlowRoutine());
    }

    private IEnumerator SlowRoutine()
    {
        isSlowing = true;

        // Activer le slow motion
        Time.timeScale = slowScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Attendre en temps réel (pas affecté par timeScale)
        yield return new WaitForSecondsRealtime(slowDuration);

        // Retour normal du jeu
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        isSlowing = false;
    }
}