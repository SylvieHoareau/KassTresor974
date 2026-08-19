using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Outil d'optimisation d'assets 3D conçu pour Unity 6 (6000.x).
/// Il parcourt les fichiers du projet pour appliquer des configurations d'importation légères.
/// </summary>
public class Unity6AssetOptimizer : EditorWindow
{
    [MenuItem("Tools/Unity 6/Optimiser les Assets 3D")]
    public static void ShowWindow()
    {
        // Ouvre la fenêtre d'outil dans l'éditeur Unity
        GetWindow<Unity6AssetOptimizer>("Optimiseur Unity 6");
    }

    private void OnGUI()
    {
        GUILayout.Label("Optimisation des Modèles 3D et Textures", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Cet outil va analyser les fichiers du dossier Assets et optimiser la taille mémoire des modèles 3D et des textures pour réduire les temps de chargement sous Unity 6.",
            MessageType.Info
        );

        GUILayout.Space(15);

        // Bouton 1 : Optimisation des modèles 3D
        if (GUILayout.Button("1. Optimiser les Modèles 3D (.fbx, .obj)", GUILayout.Height(30)))
        {
            OptimizeAllMeshes();
        }

        GUILayout.Space(10);

        // Bouton 2 : Optimisation des textures
        if (GUILayout.Button("2. Compressor les Textures Lourdes", GUILayout.Height(30)))
        {
            OptimizeAllTextures();
        }
    }

    /// <summary>
    /// Parcourt et optimise tous les fichiers de modèles 3D.
    /// </summary>
    private static void OptimizeAllMeshes()
    {
        // Recherche tous les modèles 3D dans le projet
        string[] guids = AssetDatabase.FindAssets("t:Model");
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;

            if (modelImporter != null)
            {
                // Désactive l'option Read/Write pour économiser la RAM en cours de jeu
                modelImporter.isReadable = false;

                // Active la compression de maillage
                modelImporter.meshCompression = ModelImporterMeshCompression.Medium;

                // Réimporte l'asset avec les nouveaux paramètres
                modelImporter.SaveAndReimport();
                count++;
            }
        }

        Debug.Log($"[Unity 6 Optimizer] {count} modèle(s) 3D optimisé(s) avec succès !");
    }

    /// <summary>
    /// Parcourt et applique une compression sur les textures 3D.
    /// </summary>
    private static void OptimizeAllTextures()
    {
        // Recherche toutes les textures dans le projet
        string[] guids = AssetDatabase.FindAssets("t:Texture2D");
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            if (textureImporter != null)
            {
                // Limite la taille maximale des textures à 2048x2048 pour la 3D
                if (textureImporter.maxTextureSize > 2048)
                {
                    textureImporter.maxTextureSize = 2048;
                    textureImporter.textureCompression = TextureImporterCompression.Compressed;
                    textureImporter.SaveAndReimport();
                    count++;
                }
            }
        }

        Debug.Log($"[Unity 6 Optimizer] {count} texture(s) compressée(s) avec succès !");
    }
}
