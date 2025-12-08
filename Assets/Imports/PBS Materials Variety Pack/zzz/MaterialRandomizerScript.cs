using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MaterialRandomizerScript : MonoBehaviour
{
    public List<Material> materials;
    public List<GameObject> gameObjects;

    public void randomizeMaterials()
    {
        if (materials == null || materials.Count == 0) return;
        if (gameObjects == null || gameObjects.Count == 0) return;

        List<Material> materialsCopy = new List<Material>(materials);

        foreach (GameObject go in gameObjects)
        {
            if (go == null) continue;

            if (materialsCopy.Count == 0)
            {
                // plus de matériaux dispo
                return;
            }

            // max est EXCLUSIF, donc on met Count (pas Count - 1)
            int chosen = Random.Range(0, materialsCopy.Count);

            MeshRenderer rend = go.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                rend.material = materialsCopy[chosen];
            }

            materialsCopy.RemoveAt(chosen);
        }
    }

#if UNITY_EDITOR
    public void findMaterials()
    {
        string[] guids = AssetDatabase.FindAssets(
            "t:Material",
            new[] { "Assets/PBS Materials Variety Pack/" }
        );

        if (materials == null)
            materials = new List<Material>();
        materials.Clear();

        foreach (string id in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(id);

            if (path.Contains("coming_soon"))
                continue;

            Material mat = AssetDatabase.LoadMainAssetAtPath(path) as Material;
            if (mat != null)
            {
                materials.Add(mat);
            }
        }
    }
#endif

    public void findMaterialSpheres()
    {
        if (gameObjects == null)
            gameObjects = new List<GameObject>();

        gameObjects.Clear();

#if UNITY_2022_2_OR_NEWER
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
#else
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
#endif

        foreach (var gameObj in allObjects)
        {
            if (gameObj != null && gameObj.name == "Material Sphere")
            {
                gameObjects.Add(gameObj);
            }
        }
    }
}
