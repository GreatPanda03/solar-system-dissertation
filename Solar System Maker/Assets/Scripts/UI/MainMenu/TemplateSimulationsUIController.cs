// Handles the Main Menu interface


// LIBRARIES
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


// CLASS DEFINITION
public static class TemplateSimulationsUIController
{
    public static VisualElement ui;


    #region UI Setup

    // initialises the Saved Simulations interface
    public static void Initialise()
    {
        ui = GameObject.Find("TemplateSimulationsUI").GetComponent<UIDocument>().rootVisualElement;
        
        ui.Q<Button>("CloseWindow").clicked += OnCloseWindowClicked;

        Debug.Log("[TEMPLATE SIMS] SUCCESS: UI Loaded");
    }

    #endregion UI Setup


    #region General UI

    // shows the Saved Simulations interface
    public static void ShowInterface()
    {
        ui.Q("Root").style.display = DisplayStyle.Flex;
    }

    // hides the Saved Simulations interface
    public static void HideInterface()
    {
        ui.Q("Root").style.display = DisplayStyle.None;
    }

    // closes the new saves window and re-displays the main menu
    private static void OnCloseWindowClicked()
    {
        HideInterface();
        MainMenuUIController.ShowInterface();
    }

    #endregion General UI


    #region Template Management

    // load all template simulations - one entry per template
    public static void LoadTemplateEntries()
    {
        List<Simulation> templates = DataManager.templateSaves;

        foreach (Simulation template in templates)
        {
            // create an entry of the template - copy a template entry
            // .GetVisualElement / RootElement or smth like that
        }
    }

    #endregion Template Management
}