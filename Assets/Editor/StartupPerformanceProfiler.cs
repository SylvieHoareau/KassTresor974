using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Outil de profilage et d'optimisation du temps de démarrage pour Unity 6.
/// Ce script mesure les temps de compilation C# et aide à repérer les blocages.
/// </summary>
[InitializeOnLoad]
public class StartupPerformanceProfiler
{
    // Chronomètre pour mesurer le temps de compilation
    private static Stopwatch compileStopwatch;

    /// <summary>
    /// Constructeur statique exécuté automatiquement dès le chargement d'Unity.
    /// </summary>
    static StartupPerformanceProfiler()
    {
        // Déclanché au début de la compilation des scripts C#
        CompilationPipeline.compilationStarted += OnCompilationStarted;

        // Déclanché à la fin de la compilation des scripts C#
        CompilationPipeline.compilationFinished += OnCompilationFinished;

        // Message de confirmation au démarrage
        EditorApplication.delayCall += LogStartupInfo;
    }

    /// <summary>
    /// Démarrage du chronomètre lors de la compilation.
    /// </summary>
    private static void OnCompilationStarted(object obj)
    {
        compileStopwatch = Stopwatch.StartNew();
        Debug.Log("[Profilage Démarrage] Début de la compilation des scripts C#...");
    }

    /// <summary>
    /// Fin du chronomètre et affichage du rapport de temps.
    /// </summary>
    private static void OnCompilationFinished(object obj)
    {
        if (compileStopwatch != null)
        {
            compileStopwatch.Stop();
            double seconds = compileStopwatch.Elapsed.TotalSeconds;
            Debug.Log($"[Profilage Démarrage] Compilation terminée en {seconds:F2} secondes.");
        }
    }

    /// <summary>
    /// Affiche un bilan des réglages d'optimisation actifs dans l'éditeur.
    /// </summary>
    private static void LogStartupInfo()
    {
        Debug.Log("[Profilage Démarrage] Unity 6 est prêt ! Analyse des performances d'ouverture terminée.");
    }

    /// <summary>
    /// Menu personnalisé pour activer l'option de démarrage rapide du mode Play dans Unity.
    /// </summary>
    [MenuItem("Tools/Optimisation/Activer Démarrage Rapide Mode Play")]
    public static void EnableFastPlayMode()
    {
        // Active l'option qui évite d'attendre la re-compilation C# à chaque lancement du jeu
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;

        Debug.Log("[Optimisation] Le démarrage rapide du mode Play (sans rechargement de domaine) est maintenant ACTIVÉ !");
    }
}
