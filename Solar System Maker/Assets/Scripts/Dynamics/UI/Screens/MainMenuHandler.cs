// creates an instance of the "Main Menu" interface
// * the interface allows the user to navigate around the app

/* REFERENCES:
[1] - Game Dev Guide, "Building Runtime UI with UI Toolkit in Unity", www.youtube.com [Online]. Available at: https://www.youtube.com/watch?v=6DcwHPxCE54.
[2] - Bunny Coding, "Unity3D UI Toolkit Tutorial - Main Menu", www.youtube.com [Online]. Available at: https://www.youtube.com/watch?v=u6UJrsZ577k
[3] - DA LAB, "Unity - Call a method/function from another script", www.youtube.com [Online]. Available at: https://www.youtube.com/watch?v=gFwf_T8_8po
*/

using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Collections.Generic;

public class MainMenuHandler
{
    private VisualElement ui;
    private UIController uiController;

    // prepares the given UI as a "Main Menu" interface
    public MainMenuHandler(VisualElement ui, UIController uiController)
    {
        this.ui = ui;
        this.uiController = uiController;

        ui.Q<Button>("NewSimulation").clicked += OnNewSimulationClicked;
        ui.Q<Button>("SavedSimulations").clicked += OnSavedSimulationsClicked;
        ui.Q<Button>("TemplateSimulations").clicked += OnTemplateSimulationsClicked;
        // ui.Q<Button>("Tutorial").clicked += OnTutorialButtonClicked;             // WIP
        ui.Q<Button>("ExitApp").clicked += OnExitClicked;

        // loading a sample simulation
        float G = 0.000_1f;

        List<Body> bodies = SystemGenerator.GenerateMenuSystem(G);

        Simulation simulation = new()
        {
            bodies = bodies,
            settings = new() { GravitationalConstant = G }
        };

        SimulationController simController = GameObject.Find("Simulator").GetComponent<SimulationController>();
        simController.LoadSimulation(simulation);
        simController.renderer.Distance = "Metre";
        simController.renderer.ZoomFactor = -0.5f;
        simController.physics.TimePerSecond = "1 Minute";

        simController.IsActive = true;
        simController.IsRunning = true;
        simController.ShowLabels = true;
        simController.ShowTrails = false;

        // clear simulation when main menu is left
        foreach (VisualElement element in ui.Q<VisualElement>("Container").Children())
        {
            if (element is Button button && element.name != "Tutorial") // tutorial button is a WIP
            {
                button.clicked += () => {
                    simController.DisableSimulator();
                    simController.ClearSimulator();
                    simController = null;
                };
            }
        }
    }

    // shows the "New Simulations" interface on click
    private void OnNewSimulationClicked()
    {
        uiController.ShowNewSimulation();
        
    }

    // shows the "Saved Simulations" interface on click
    private void OnSavedSimulationsClicked()
    {
        uiController.ShowSavedSimulations();
    }

    // shows the "Template Simulations" interface on click
    private void OnTemplateSimulationsClicked()
    {
        uiController.ShowTemplateSimulations();
    }

    // shows the "Tutorial" interface on click
    private void OnTutorialButtonClicked()
    {
        uiController.ShowTutorial();
    }

    // completely closes the application
    private static void OnExitClicked()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #endif
    }
}


