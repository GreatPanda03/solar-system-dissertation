// Handles the Main Menu interface

/* REFERENCES:
[1] - Game Dev Guide, "Building Runtime UI with UI Toolkit in Unity", www.youtube.com [Online]. Available at: https://www.youtube.com/watch?v=6DcwHPxCE54.
[2] - Bunny Coding, "Unity3D UI Toolkit Tutorial - Main Menu", www.youtube.com [Online]. Available at: https://www.youtube.com/watch?v=u6UJrsZ577k
[3] - DA LAB, "Unity - Call a method/function from another script", www.youtube.com [Online]. Available at: https://www.youtube.com/watch?v=gFwf_T8_8po
*/

// LIBRARIES
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;


// CLASS DEFINITION
public static class MainMenuUIController
{
    // UI ELEMENTS
    public static VisualElement ui;
    public static SampleSimulationScript sampleSimScript;


    #region UI Setup

    // initialises all Main Menu elements
    public static void Initialise()
    {
        ui = GameObject.Find("MainMenuUI").GetComponent<UIDocument>().rootVisualElement;
        sampleSimScript = GameObject.Find("SampleSimulation").GetComponent<SampleSimulationScript>(); 

        ui.Q<Button>("NewSimulation").clicked += OnNewSimButtonClicked;
        ui.Q<Button>("SavedSimulations").clicked += OnSavedSimsButtonClicked;
        ui.Q<Button>("TemplateSimulations").clicked += OnTemplateSimsButtonClicked;
        // ui.Q<Button>("Tutorial").clicked += OnTutorialButtonClicked;             // WIP
        ui.Q<Button>("ExitApp").clicked += OnExitButtonClicked;

        Debug.Log("[MAIN MENU] SUCCESS: UI Loaded");
    }

    #endregion UI Setup


    #region General UI

    // displays the Main Menu interface
    public static void ShowInterface()
    {
        ui.style.display = DisplayStyle.Flex;

        sampleSimScript.LoadSampleSimulation();
        sampleSimScript.RunSampleSimulation();
    }

    // hides the Main Menu interface
    public static void HideInterface()
    {
        ui.style.display = DisplayStyle.None;

        sampleSimScript.StopSampleSimulation();
        sampleSimScript.ClearSampleSimulation();
    }

    // displays the New Simulation interface
    private static void OnNewSimButtonClicked()
    {
        HideInterface();
        NewSimulationUIController.ShowInterface();
    }

    // displays the Saved Simulations interface
    private static void OnSavedSimsButtonClicked()
    {
        HideInterface();
        SavedSimulationsUIController.ShowInterface();
    }

    // displays the Template Simulations interface
    private static void OnTemplateSimsButtonClicked()
    {
        HideInterface();
        TemplateSimulationsUIController.ShowInterface();
    }

    // displays the Tutorial interface
    private static void OnTutorialButtonClicked()
    {
        HideInterface();
        TutorialUIController.ShowInterface();
    }

    // completely closes the application
    private static void OnExitButtonClicked()
    {
        Application.Quit();
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #endif
    }

    #endregion General UI
}


