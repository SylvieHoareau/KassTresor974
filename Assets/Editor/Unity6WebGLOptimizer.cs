using UnityEngine;
using UnityEditor;

/// <summary>
/// Outil d'optimisation d'Éditeur Unity 6 pour la préparation de Builds WebGL.
/// </summary>
public class Unity6WebGLOptimizer : EditorWindow
{
    [MenuItem("Tools/WebGL/Appliquer la configuration WebGL Optimisée")]
    public static void ApplyWebGLSettings()
    {
        Debug.Log("[WebGL Optimizer] Début de la configuration des paramètres WebGL...");

        // 1. Passage de la plateforme cible à WebGL
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

        // 2. Activation de la compression Brotli (fichiers plus petits pour le navigateur)
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;

        // 3. Désactivation des exceptions C# pour accélérer l'exécution du code
        // (SetStackTraceType à None réduit la taille du binaire compilé WebAssembly)
        PlayerSettings.SetStackTraceType(ScriptBackingLayer.IL2CPP, StackTraceType.None);

        // 4. Optimisation de l'utilisation mémoire WebGL
        // Autorise le navigateur à étendre la mémoire de façon dynamique si besoin
        PlayerSettings.WebGL.memorySize = 512; // Taille initiale en Mo

        // 5. Désactivation des fonctionnalités inutiles sur le Web pour gagner du poids
        PlayerSettings.WebGL.showDiagnostics = false;

        Debug.Log("[WebGL Optimizer] Configuration WebGL terminée avec succès !");
        EditorUtility.DisplayDialog("Optimisation WebGL", "Les paramètres de Build WebGL ont été appliqués avec succès !", "OK");
    }

    [MenuItem("Tools/WebGL/Optimiser Textures pour WebGL")]
    public static void OptimizeTexturesForWebGL()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D");
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            if (textureImporter != null)
            {
                // Configuration spécifique pour la plateforme WebGL
                TextureImporterPlatformSettings webglSettings = textureImporter.GetPlatformTextureSettings("WebGL");
                webglSettings.overridden = true;
                webglSettings.maxTextureSize = 1024; // Seuil recommandé sur WebGL pour préserver la RAM
                webglSettings.format = TextureImporterFormat.ASTC_6x6; // Format très léger

                textureImporter.SetPlatformTextureSettings(webglSettings);
                textureImporter.SaveAndReimport();
                count++;
            }
        }

        Debug.Log($"[WebGL Optimizer] {count} texture(s) réoptimisée(s) spécifiquement pour le WebGL.");
    }
}
