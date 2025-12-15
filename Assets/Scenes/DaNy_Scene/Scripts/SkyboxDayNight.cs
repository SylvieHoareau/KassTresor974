using UnityEngine;
using System.Collections;

public class SkyboxDayNight : MonoBehaviour
{
    [Header("Skybox")]
    public Material daySkybox;    // ex : EpicBlueSunset
    public Material nightSkybox;  // ex : ton sky AllSky de nuit
    public float transitionDuration = 2f;

    [Header("Lumière (optionnel mais conseillé)")]
    public Light sunLight;              // ta Directional Light
    public float dayLightIntensity = 1.2f;
    public float nightLightIntensity = 0.2f;

    private bool isNight = false;
    private Coroutine currentCoroutine;

    void Start()
    {
        // Skybox de départ = jour
        if (daySkybox != null)
            RenderSettings.skybox = daySkybox;

        if (sunLight != null)
            sunLight.intensity = dayLightIntensity;
    }

    void Update()
    {
        // Pour tester : touche N pour switch jour/nuit
        if (Input.GetKeyDown(KeyCode.N))
        {
            ToggleDayNight();
        }
    }

    public void ToggleDayNight()
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(SwitchSkybox());
    }

    private IEnumerator SwitchSkybox()
    {
        if (daySkybox == null || nightSkybox == null)
        {
            Debug.LogWarning("Assigne bien daySkybox et nightSkybox dans l'inspector.");
            yield break;
        }

        Material fromMat = isNight ? nightSkybox : daySkybox;
        Material toMat   = isNight ? daySkybox   : nightSkybox;

        // Mat temporaire pour le fondu
        Material lerpMat = new Material(fromMat.shader);
        lerpMat.CopyPropertiesFromMaterial(fromMat);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / transitionDuration;
            float k = Mathf.Clamp01(t);

            // Lerp entre les 2 skybox (matériaux AllSky utilisent le même shader)
            lerpMat.Lerp(fromMat, toMat, k);
            RenderSettings.skybox = lerpMat;

            // Lerp aussi l'intensité de la lumière
            if (sunLight != null)
            {
                float fromIntensity = isNight ? nightLightIntensity : dayLightIntensity;
                float toIntensity   = isNight ? dayLightIntensity  : nightLightIntensity;
                sunLight.intensity  = Mathf.Lerp(fromIntensity, toIntensity, k);
            }

            yield return null;
        }

        // Fin de transition : applique la skybox cible
        RenderSettings.skybox = toMat;

        isNight = !isNight;
        currentCoroutine = null;
    }
}
