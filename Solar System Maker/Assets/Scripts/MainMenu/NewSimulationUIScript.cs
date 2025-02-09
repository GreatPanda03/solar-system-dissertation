// Handles the Main Menu interface

using System;
using UnityEngine;
using UnityEngine.UIElements;

public class NewSimulationUIScript : MonoBehaviour
{
    public VisualElement mainMenuUI;
    public VisualElement newSimUI;
    public VisualElement simulatorUI;

    // retrieves the relevant interfaces
    private void Awake()
    {
        mainMenuUI = GameObject.Find("MainMenuUI").GetComponent<UIDocument>().rootVisualElement;
        newSimUI = GetComponent<UIDocument>().rootVisualElement;
        simulatorUI = GameObject.Find("SimulatorUI").GetComponent<UIDocument>().rootVisualElement;
    }

    // initialises the functionality of all buttons in the interface
    private void OnEnable()
    {
        newSimUI.Q<Button>("CreateSimulation").clicked += OnCreateSimulationClicked;
        newSimUI.Q<Button>("CloseWindow").clicked += OnCloseWindowClicked;

        Debug.Log("[NEW SIM] SUCCESS: UI Loaded");
    }

    // creates a blank simulation with the settings provided
    private void OnCreateSimulationClicked()
    {
        // validating data
        var simNameInput = newSimUI.Q<TextField>("SimulationNameInput");
        var gravConstantInput = newSimUI.Q<FloatField>("GravityConstantInput");
        var gravAlgorithmInput = newSimUI.Q<DropdownField>("GravityAlgorithmInput");
        var validationLabel = newSimUI.Q<Label>("Validation");

        if (simNameInput.text == "")
        {
            validationLabel.text = "\"Name\" cannot be blank!";
            return;
        /*}
        else if (gravConstantInput.value == ...)
        {
            validationLabel.text = "\"Gravity Constant\" cannot be blank!";
            return;
        */}
        else if (gravAlgorithmInput.value == "")
        {
            validationLabel.text = "\"Gravity Algorithm\" cannot be blank!";
            return;
        }

        // loading the new simulation if valid data
        mainMenuUI.style.display = DisplayStyle.None;
        newSimUI.style.display = DisplayStyle.None;
        simulatorUI.style.display = DisplayStyle.Flex;

        var sampleSimulationScript = GameObject.Find("SampleSimulation").GetComponent<SampleSimulationScript>();
        sampleSimulationScript.ClearSampleSimulation();

        Settings settings = new Settings();
        settings.SimulationName = simNameInput.value;
        settings.GravityConstant = gravConstantInput.value;
        settings.GravityAlgorithm = gravAlgorithmInput.value;

        var simulatorUIScript = GameObject.Find("SimulatorUI").GetComponent<SimulatorUIScript>();
        simulatorUIScript.LoadSimulator(settings);
        simulatorUI.Q<Label>("Title").text = simNameInput.value;

        var cameraScript = Camera.main.GetComponent<CameraScript>();
        cameraScript.SetScene(CameraScript.Scenes.Simulator);
    }

    // closes the new simulation window and re-displays the main menu
    private void OnCloseWindowClicked()
    {
        mainMenuUI.style.display = DisplayStyle.Flex;
        newSimUI.style.display = DisplayStyle.None;
    }
}


