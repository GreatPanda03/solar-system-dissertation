// creates an instance of the "New Simulations" interface
// * the interface allows users to create and start new simulations

using UnityEngine;
using UnityEngine.UIElements;


public class NewSimulationHandler
{

    #region Attributes

    public VisualElement ui;
    public UIController uiController;

    #endregion Attributes


    #region Constructor

    // prepares the given UI as a "New Simulation" interface
    public NewSimulationHandler(VisualElement ui, UIController uiController)
    {
        this.ui = ui;
        this.uiController = uiController;

        ui.Q<Button>("Create").clicked += OnCreateClicked;
        ui.Q<Button>("Close").clicked += OnCloseClicked;
    }

    #endregion Constructor


    #region UI Methods

    // creates a blank simulation save with the settings provided
    private void OnCreateClicked()
    {
        if (!ValidateDetails()) return;

        Settings settings = GetDetailsAsSettings();

        Simulation newSimulation = DataHandler.CreateSimulationSave();
        newSimulation.settings = settings;

        SimulationController simController = GameObject.Find("Simulator").GetComponent<SimulationController>();
        simController.LoadSimulation(newSimulation);
        simController.FullEnable();

        uiController.ShowSimulator();

        // adjusting the camera        
        CameraController.SetCameraMode(CameraController.CameraMode.Simulator);
    }

    // returns to the "Main Menu" on click
    private void OnCloseClicked()
    {
        uiController.ShowMainMenu();
    }
    
    #endregion UI Methods


    #region Creation Methods

    // returns True if all simulation details are valid
    public bool ValidateDetails()
    {
        var name = ui.Q<VisualElement>("Name").Q<TextField>("Input");

        if (name.text == "")
        {
            DisplayValidation("<b>'Name'</b> cannot be blank!");
            return false;
        }

        // description can be blank
        
        var gravitationalConstant = ui.Q<VisualElement>("GravitationalConstant").Q<FloatField>("Input");

        if (gravitationalConstant.value == float.NaN || gravitationalConstant.value == 0)
        {
            DisplayValidation("<b>'G'</b> must be greater than 0!");
            return false;
        }
        
        return true;
    }

    // gathers and returns all details as a settings object
    public Settings GetDetailsAsSettings()
    {
        var name = ui.Q<VisualElement>("Name").Q<TextField>("Input");
        var description = ui.Q<VisualElement>("Description").Q<TextField>("Input");
        var gravitationalConstant = ui.Q<VisualElement>("GravitationalConstant").Q<FloatField>("Input");

        return new()
        {
            Name = name.text,
            Description = description.text,
            GravitationalConstant = gravitationalConstant.value
        };
    }

    // shows the given validation message
    public void DisplayValidation(string message)
    {
        ui.Q<Label>("Validation").text = message;
    }

    #endregion Creation Methods
}