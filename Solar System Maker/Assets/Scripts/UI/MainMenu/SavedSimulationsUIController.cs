// Handles the Main Menu interface


// LIBRARIES
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


// CLASS DEFINITION
public static class SavedSimulationsUIController
{
    public static VisualElement ui;


    #region UI Setup

    // initialises the Saved Simulations interface
    public static void Initialise()
    {
        ui = GameObject.Find("SavedSimulationsUI").GetComponent<UIDocument>().rootVisualElement;
        
        ui.Q<Button>("CloseWindow").clicked += OnCloseWindowClicked;

        Debug.Log("[SAVED SIMS] SUCCESS: UI Loaded");
    }

    #endregion UI Setup


    #region General UI

    // shows the Saved Simulations interface
    public static void ShowInterface()
    {
        ui.style.display = DisplayStyle.Flex;
    }

    // hides the Saved Simulations interface
    public static void HideInterface()
    {
        ui.style.display = DisplayStyle.None;
    }

    // closes the new saves window and re-displays the main menu
    private static void OnCloseWindowClicked()
    {
        HideInterface();
        MainMenuUIController.ShowInterface();
    }

    #endregion General UI


    #region Save Management

    // load all saved simulations - one entry per save
    public static void LoadSaveEntries()
    {
        List<Simulation> saves = DataManager.simulationSaves;

        foreach (Simulation save in saves)
        {
            // create an entry of the save - copy a template entry
            // .GetVisualElement / RootElement or smth like that
        }
    }

    #endregion Save Management
}


