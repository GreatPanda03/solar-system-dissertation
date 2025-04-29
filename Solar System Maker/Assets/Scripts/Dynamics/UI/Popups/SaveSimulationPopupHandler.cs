// manages instances of Save popups
// * these popups allow the user to edit and save the given simulation as another save or as a template

using System.Collections.Generic;
using UnityEngine.UIElements;

public class SaveSimulationPopupHandler : IPopup
{

    #region Attributes

    public static string identifier = "SaveSimulationPopup";
    public VisualElement popup;
    public UIController uiController;
    public VisualElement parent;
    public Simulation simulation;

    #endregion Attributes


    #region Constructor

    // prepares the given popup as an "Options Popup", bound to
    //   the given parent interface and simulation instance
    public SaveSimulationPopupHandler(
        VisualElement popup, 
        UIController uiController,
        VisualElement parent,
        Simulation simulation)
    {
        this.popup = popup;
        this.uiController = uiController;
        this.parent = parent;
        this.simulation = simulation;
        
        if (parent.Q<VisualElement>(identifier) != null) return;
        
        popup.name = identifier;
        parent.Add(popup);

        UIController.PrepareFieldFocus(popup);

        Settings settings = simulation.settings;
        popup.Q<TextField>("Name").value = settings.Name;
        popup.Q<TextField>("Description").value = settings.Description;
        popup.Q<TextField>("GravitationalConstant").value = settings.GravitationalConstant.ToString();

        popup.Q<Button>("SaveSimulation").clicked += () => {
            if (!ValidateDetails(popup)) return;

            popup.RemoveFromHierarchy();

            Settings settings = GetSaveDetailsAsSettings(popup);

            List<Body> bodyCopies = new();
            foreach (Body body in simulation.bodies)
                bodyCopies.Add(body.GetShallowCopy());

            Simulation newSave = DataHandler.GetNextSimulationSaveSlot();
            newSave.settings = settings;
            newSave.bodies = bodyCopies;

            DataHandler.SimulationSaves.Add(newSave);
            simulation.id = newSave.id;

            // TODO: change this so that it's not specific!
            parent.Q<VisualElement>("Toolbar").Q<Label>("Title").text = newSave.settings.Name;
        }; 

        popup.Q<Button>("SaveTemplate").clicked += () => {
            if (!ValidateDetails(popup)) return;

            popup.RemoveFromHierarchy();

            Settings settings = GetSaveDetailsAsSettings(popup);

            List<Body> bodyCopies = new();
            foreach (Body body in simulation.bodies)
                bodyCopies.Add(body.GetShallowCopy());

            Simulation newSave = DataHandler.GetNextSimulationTemplateSlot();
            newSave.settings = settings;
            newSave.bodies = bodyCopies;

            DataHandler.SimulationTemplates.Add(newSave);
        };

        popup.Q<Button>("Close").clicked += () => {
            popup.RemoveFromHierarchy();
        };
    }

    #endregion Constructor


    #region Validation

    // validates the save's details
    public bool ValidateDetails(VisualElement popup)
    {
        var name = popup.Q<TextField>("Name");

        if (name.value == "")
        {
            DisplayValidation("<b>'Name'</b> cannot be blank!");
            return false;
        }

        // description can be blank
        
        var gravitationalConstant = popup.Q<TextField>("GravitationalConstant");

        if (gravitationalConstant.value == "")
        {
            DisplayValidation("<b>'G'</b> must be greater than 0!");
            return false;
        }

        return true;
    }

    // displays the given message as a validation message
    public void DisplayValidation(string message)
    {
        popup.Q<Label>("Validation").text = message;
    }

    #endregion Validation


    #region Extraction
        
    // gathers the save's details
    public Settings GetSaveDetailsAsSettings(VisualElement popup)
    {
        var name = popup.Q<TextField>("Name");
        var description = popup.Q<TextField>("Description");
        // var gravitationalConstant = popup.Q<TextField>("GravitationalConstant");

        return new()
        {
            Name = name.value,
            Description = description.value,
            GravitationalConstant = simulation.settings.GravitationalConstant
        };
    }

    #endregion Extraction

}