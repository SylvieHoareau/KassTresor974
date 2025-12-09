using UnityEngine;

/// <summary>
/// Gère le contrôle d'un système de particules de fumée volcanique.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class VolcanicSmokeController : MonoBehaviour
{
    private ParticleSystem smokeParticleSystem;

    // Intensité maximale pour les émissions
    private const float MaxEmissionRate = 100f;
    // Intensité minimale (fumée constante)
    private const float MinEmissionRate = 10f;

    void Awake()
    {
        // Récupère la référence au système de particules
        smokeParticleSystem = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Intensifie la fumée volcanique en augmentant le taux d'émission.
    /// </summary>
    public void IntensifySmoke()
    {
        // Obtenir la structure du module d'emission
        var emission = smokeParticleSystem.emission;

        // Mettre à jour le taux d'émission pour l'intensifier
        emission.rateOverTime = MaxEmissionRate;
    }

    /// <summary>
    /// Diminue l'intensité de la fumée volcanique en réduisant le taux d'émission.
    /// </summary>
    public void ReduceSmoke()
    {
        // Obtenir la structure du module d'emission
        var emission = smokeParticleSystem.emission;

        // Mettre à jour le taux d'émission pour le réduire
        emission.rateOverTime = MinEmissionRate;
    }

    // Update is called once per frame
    void Update()
    {
        // Si l'utilisateur appuie sur 'Space', intensifier la fumée
        if (Input.GetKeyDown(KeyCode.Space))
        {
            IntensifySmoke();
        }
        // Si l'utilisateur appuie sur 'Space', réduire la fumée
        if (Input.GetKeyUp(KeyCode.Space))
        {
            ReduceSmoke();
        }
    }
}
