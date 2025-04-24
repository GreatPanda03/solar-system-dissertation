// creates instances of the "Templates Simulations" interface
// * the interface shows all templates the user has which they can load a copy of and run

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TemplateSimulationsHandler
{
    
    #region Attributes

    public VisualElement ui;
    public UIController uiController;

    #endregion Attributes


    #region Constructor

    // prepares the given interface as a "Template Simulations" interface
    public TemplateSimulationsHandler(VisualElement ui, UIController uiController)
    {
        this.ui = ui;
        this.uiController = uiController;
        
        ui.Q<Button>("Close").clicked += OnCloseClicked;

        Debug.Log("[TEMPLATE SIMS] SUCCESS: UI Loaded");
    }

    #endregion Constructor


    #region UI Methods

    // closes the new saves window and re-displays the main menu
    private void OnCloseClicked()
    {
        uiController.ShowMainMenu();
    }

    #endregion UI Methods


    #region Template Management

    // load all template simulations
    public static void LoadTemplateEntries()
    {
        List<Simulation> templates = DataHandler.TemplateSaves;

        foreach (Simulation template in templates)
        {
            // create an entry of the template - copy a template entry
            // .GetVisualElement / RootElement or smth like that
        }
    }

    #endregion Template Management
}