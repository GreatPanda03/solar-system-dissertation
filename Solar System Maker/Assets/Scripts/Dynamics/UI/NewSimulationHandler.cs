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

        uiController.ShowSimulator();

        SimulationController simController = GameObject.Find("Simulator").GetComponent<SimulationController>();
        simController.LoadSimulation(newSimulation);
        simController.IsActive = true;

        // adjusting the camera        
        CameraController.SetScene(CameraController.Mode.Simulator);
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
            DisplayValidation("'Name' cannot be blank!");
            return false;
        }
        
        var gravityConstant = ui.Q<VisualElement>("GravityConstant").Q<FloatField>("Input");

        if (gravityConstant.value == float.NaN)
        {
            DisplayValidation("'Gravity Constant' cannot be blank!");
            return false;
        }
        
        var gravityAlgorithm = ui.Q<VisualElement>("GravityAlgorithm").Q<DropdownField>("Input");

        if (gravityAlgorithm.value == "")
        {
            DisplayValidation("'Gravity Algorithm' cannot be blank!");
            return false;
        }

        var collisionQuality = ui.Q<VisualElement>("CollisionQuality").Q<DropdownField>("Input");;

        if (collisionQuality.value == null)
        {
            DisplayValidation("'Collision Quality' cannot be blank!");
            return false;
        }

        return true;
    }

    // gathers and returns all details as a settings object
    public Settings GetDetailsAsSettings()
    {
        var name = ui.Q<VisualElement>("Name").Q<TextField>("Input");
        var gravityConstant = ui.Q<VisualElement>("GravityConstant").Q<FloatField>("Input");
        var gravityAlgorithm = ui.Q<VisualElement>("GravityAlgorithm").Q<DropdownField>("Input");
        var collisionQuality = ui.Q<VisualElement>("CollisionQuality").Q<DropdownField>("Input");;

        return new()
        {
            SimulationName      = name.text,
            GravityConstant     = gravityConstant.value,
            GravityAlgorithm    = gravityAlgorithm.value,
            CollisionQuality    = collisionQuality.value
        };
    }

    // shows the given validation message
    public void DisplayValidation(string message)
    {
        ui.Q<Label>("Validation").text = message;
    }

    #endregion Creation Methods
}