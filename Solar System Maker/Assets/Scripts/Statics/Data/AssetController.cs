// "preloads" assets so that there aren't any frame skips when used
// * frame skips started when "Label" was introduced

using UnityEngine;

public class AssetController
{
    public static void PreloadAssets()
    {
        Debug.Log("Preloading assets...");

        PreloadPrefabs();
        PreloadMaterials();
        
        Debug.Log("Assets preloaded");
    }

    // preloads all prefabs
    private static void PreloadPrefabs()
    {
        foreach (var prefab in Resources.LoadAll("Prefabs"))
        {
            var prefabInstance = GameObject.Instantiate(prefab);
            GameObject.Destroy(prefabInstance);
        }
    }

    // preloads all materials
    public static void PreloadMaterials()
    {
        foreach (var prefab in Resources.LoadAll("Materials/Generic"))
        {
            var prefabInstance = GameObject.Instantiate(prefab);
            GameObject.Destroy(prefabInstance);
        }

        foreach (var prefab in Resources.LoadAll("Materials/Specific"))
        {
            var prefabInstance = GameObject.Instantiate(prefab);
            GameObject.Destroy(prefabInstance);
        }   
    }
}