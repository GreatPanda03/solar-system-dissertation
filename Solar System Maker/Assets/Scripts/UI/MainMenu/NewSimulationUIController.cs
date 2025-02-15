// Handles the Main Menu interface

// LIBRARIES
using UnityEngine;
using UnityEngine.UIElements;


// CLASS DEFINITION
public static class NewSimulationUIController
{
    // INTERFACE ELEMENTS
    public static VisualElement ui;
    public static VisualElement simulatorUI;


    #region UI Setup

    // initialises the New Simulation interface elements
    public static void Initialise()
    {
        ui = GameObject.Find("NewSimulationUI").GetComponent<UIDocument>().rootVisualElement;
        simulatorUI = GameObject.Find("SimulatorUI").GetComponent<UIDocument>().rootVisualElement;

        ui.Q<Button>("CreateSimulation").clicked += OnCreateSimulationClicked;
        ui.Q<Button>("CloseWindow").clicked += OnCloseWindowClicked;

        Debug.Log("[NEW SIM] SUCCESS: UI Loaded");
    }

    #endregion UI Setup


    #region General UI

    // displays the New Simulation interface
    public static void ShowInterface()
    {
        ui.Q("Root").style.display = DisplayStyle.Flex;
    }

    // hides the New Simulation Interface
    public static void HideInterface()
    {
        ui.Q("Root").style.display = DisplayStyle.None;
    }

    // closes the new simulation window and re-displays the main menu
    private static void OnCloseWindowClicked()
    {
        HideInterface();
        MainMenuUIController.ShowInterface();
    }

    #endregion General UI


    #region Simulation Creation

    // creates a blank simulation with the settings provided
    // * hides the Main Menu and displays the Simulator and its interface
    private static void OnCreateSimulationClicked()
    {
        Settings settings = RunValidation();
        if (settings == null) return; // data was invalid!

        // constructing a new simulation
        Simulation newSimulation = DataManager.CreateSimulationSave();
        newSimulation.settings = settings;

        // hiding the entire Main Menu interface
        MainMenuUIController.HideInterface();
        HideInterface();

        // displaying the entire Simulator interface
        SimulatorUIController simUIController = GameObject.Find("Simulator").GetComponent<SimulatorUIController>();
        simUIController.ShowInterface();
        simUIController.EnableInterface();
        simUIController.SetTitle(settings.SimulationName);

        SimulatorController simController = GameObject.Find("Simulator").GetComponent<SimulatorController>();
        simController.LoadSimulation(newSimulation);
        simController.EnableSimulator();

        // adjusting the camera        
        Camera.main.GetComponent<CameraController>()
            .SetScene(CameraController.Mode.Simulator);
    }

    // validates the given data - if valid, returns a Settings object
    private static Settings RunValidation()
    {
        // data inputs
        var simNameInput = ui.Q<TextField>("SimulationNameInput");
        var gravConstantInput = ui.Q<FloatField>("GravityConstantInput");
        var gravAlgorithmInput = ui.Q<DropdownField>("GravityAlgorithmInput");
        var collQualityInput = ui.Q<DropdownField>("CollisionQualityInput");
        
        var validationLabel = ui.Q<Label>("Validation");

        // validating each data field
        if (simNameInput.text == "")
        {
            validationLabel.text = "\"Name\" cannot be blank!";
            return null;
        }
        
        if (gravConstantInput.value == float.NaN)
        {
            validationLabel.text = "\"Gravity Constant\" cannot be blank!";
            return null;
        }
        
        if (gravAlgorithmInput.value == "")
        {
            validationLabel.text = "\"Gravity Algorithm\" cannot be blank!";
            return null;
        }

        if (collQualityInput.value == null)
        {
            validationLabel.text = "\"Collision Quality\" cannot be blank!";
            return null;
        }

        // creating a new settings object from the validated data
        Settings settings = new()
        {
            SimulationName = simNameInput.text,
            GravityConstant = gravConstantInput.value,
            GravityAlgorithm = gravAlgorithmInput.value,
            CollisionQuality = collQualityInput.value
        };

        return settings;
    }
    
    #endregion Simulation Creation
}


