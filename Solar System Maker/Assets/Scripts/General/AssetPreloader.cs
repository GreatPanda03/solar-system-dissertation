// "preloads" assets so that there aren't any frame skips when used
// * frame skips started when "BodyLabel" was introduced

using UnityEngine;

public class AssetPreloader
{
    public static void PreloadAssets()
    {
        Debug.Log("Preloading assets...");
        PreloadPrefabs();
        Debug.Log("Assets preloaded");
    }

    // preloads all prefabs
    private static void PreloadPrefabs()
    {
        Object[] prefabs = Resources.LoadAll("Prefabs");

        foreach (var prefab in prefabs)
        {
            var prefabInstance = GameObject.Instantiate(prefab);
            GameObject.Destroy(prefabInstance);
        }
    }
}