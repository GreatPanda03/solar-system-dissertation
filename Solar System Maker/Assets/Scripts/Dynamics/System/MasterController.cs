// manages processes throughout the entire application

using UnityEditor;
using UnityEngine;

public class MasterController : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Master started...");

        DataHandler.LoadAllData();
        AssetController.PreloadAssets();

        Debug.Log("Master finished successfully");
    }

    public void QuitApplication()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #endif
    }

    void OnApplicationQuit()
    {
        DataHandler.SaveAllData();
    }
}