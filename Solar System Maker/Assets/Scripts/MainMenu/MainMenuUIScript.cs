// Handles the Main Menu interface

/* REFERENCES:
[1] - Game Dev Guide, "Building Runtime UI with UI Toolkit in Unity", www.youtube.com [Online]. Available at: https://www.youtube.com/watch?v=6DcwHPxCE54.
[2] - Bunny Coding, "Unity3D UI Toolkit Tutorial - Main Menu", www.youtube.com [Online]. Available at: https://www.youtube.com/watch?v=u6UJrsZ577k
[3] - DA LAB, "Unity - Call a method/function from another script", www.youtube.com [Online]. Available at: https://www.youtube.com/watch?v=gFwf_T8_8po
*/

using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

public class MainMenuUIScript : MonoBehaviour
{
    public VisualElement mainMenuUI;
    public VisualElement newSimUI;
    public VisualElement savedSimsUI;
    public VisualElement templateSimsUI;
    public VisualElement simulatorUI;

    // retrieves the main menu interface
    // * the main menu is displayed first - all other UI is hidden
    // * also ensures all UI is enabled in case they've been disabled during editing
    private void Awake()
    {
        mainMenuUI = GetComponent<UIDocument>().rootVisualElement;
        newSimUI = GameObject.Find("NewSimulationUI").GetComponent<UIDocument>().rootVisualElement;
        savedSimsUI = GameObject.Find("SavedSystemsUI").GetComponent<UIDocument>().rootVisualElement;
        templateSimsUI = GameObject.Find("TemplateSystemsUI").GetComponent<UIDocument>().rootVisualElement;
        simulatorUI = GameObject.Find("SimulatorUI").GetComponent<UIDocument>().rootVisualElement;

        newSimUI.style.display = DisplayStyle.None;
        savedSimsUI.style.display = DisplayStyle.None;
        templateSimsUI.style.display = DisplayStyle.None;
        simulatorUI.style.display = DisplayStyle.None;
    }

    // initialises the functionality of all buttons in the menu
    private void OnEnable()
    {
        mainMenuUI.Q<Button>("NewSimulation").clicked += OnNewSimButtonClicked;
        mainMenuUI.Q<Button>("SavedSimulations").clicked += OnSavedSimsButtonClicked;
        mainMenuUI.Q<Button>("TemplateSimulations").clicked += OnTemplateSimsButtonClicked;
        mainMenuUI.Q<Button>("Tutorial").clicked += OnTutorialButtonClicked;
        mainMenuUI.Q<Button>("ExitApp").clicked += OnExitButtonClicked;

        Debug.Log("[MAIN MENU] SUCCESS: UI Loaded");
    }

    // creates a new blank simulation
    private void OnNewSimButtonClicked()
    {
        mainMenuUI.style.display = DisplayStyle.None;
        newSimUI.style.display = DisplayStyle.Flex;
    }

    // displays the list of all saved simulations
    private void OnSavedSimsButtonClicked()
    {
        mainMenuUI.style.display = DisplayStyle.None;
        savedSimsUI.style.display = DisplayStyle.Flex;
    }

    // displays the list of all simulation templates
    private void OnTemplateSimsButtonClicked()
    {
        mainMenuUI.style.display = DisplayStyle.None;
        templateSimsUI.style.display = DisplayStyle.Flex;
    }

    // displays the tutorial section
    private void OnTutorialButtonClicked()
    {
        Debug.Log("Displaying Tutorial");
    }

    // exits the application
    private void OnExitButtonClicked()
    {
        Application.Quit();
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #endif
    }
}


