// manages instances of the Tutorial popup


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TutorialPopupHandler : IPopup
{

    #region Attributes

    public static string identifier = "TutorialPopup";
    public VisualElement popup;
    public UIController uiController;
    public VisualElement parent;

    #endregion Attributes


    #region Constructor

    // prepares the given UI as an instance of a "Tutorial" popup for the given parent container
    public TutorialPopupHandler(
        VisualElement popup,
        UIController uiController,
        VisualElement parent)
    {
        this.popup = popup;
        this.uiController = uiController;
        this.parent = parent;

        if (parent.Q<VisualElement>(identifier) != null) return;

        popup.name = identifier;
        parent.Add(popup);

        // load text based on current controls
        string finalText = "";

        foreach(KeyValuePair<string, KeyCode> pair in AppControls.keybinds)
            finalText += $"<b>{pair.Key}</b>   -   {pair.Value}<br>";

        popup.Q<Label>("Controls").text = finalText;

        popup.Q<Button>("Close").clicked += () => {
            popup.RemoveFromHierarchy();
        };
    }

    #endregion Constructor

    
    #region Methods

    // closes the existing help popup
    public void CloseHelpPopup()
    {
        VisualElement existingPopup = popup.Q<VisualElement>(identifier);
        if (existingPopup == null) return;
        
        existingPopup.RemoveFromHierarchy();
    }

    #endregion Methods


    #region Pages

    public Dictionary<int, string> LoadPages()
    {
        Dictionary<int, string> pages = new();

        // introduction
        pages[0] = "<b>Welcome to the application!</b>" +
            "The application allows you to view existing solar or planetary systems, like our own. " +
            "You can also create your own systems or bodies to analyse and use. " +

            "The simulator can simulate basic physics, such as gravity, collisions. It can also " +
            "simulate basic effective temperature and habitability zones." +
            
            "This tutorial covers the technical features available in the application and how to use them.";
        
        // new simulation
        pages[1] = "<b>Creating a Simulation</b>" +
            "From the main menu, you can choose to create a new simulation with a name, description, " +
            "and gravitational constant. " + 
            
            "The gravitational constant represents the strength of " +
            "gravity in the system and is relative to the real value of G. For example, 2G would " + 
            "be a gravity twice as strong as our universe's gravity. " +

            "After creating a new simulation, a blank simulation space will be opened for you to " +
            "add bodies to.";

        // template simulations
        pages[2] = "<b>Loading Existing Simulations</b>" +
            "Alternatively, you can also load premade or custom templates from the main menu. " +
            "This wi' will load all stored bodies starting from their last saved state. " + 
            
            "Saved simulations are files which can be overwritten, while template simulations act " +
            "as starting points from which saves are created - templates cannot be overwritten!";

        // in a simulation
        pages[3] = "<b>In a Simulation</b>" +
            "Once in a simulation, there'll be a blank space for you to work with and view." +

            "<b>Toolbox</b>" +
            "The toolbox contains tools which you to manipulate the simulation and adjust " +
            "its details, such as add bodies or toggle body labels and trails." +

            "<b>Explorer</b>" +
            "The explorer can be selected from the toolbox and shows all bodies in the simulation. " +
            "Bodies can be deleted directly from the explorer, or followed by selecting the " +
            "corresponding entry. Selecting a body from the entry will also display its properties." +

            "<b>Properties</b>" +
            "Selecting a body displays its properties live in another panel, such as its mass, " +
            "radius, and density. Some fields can be modified to modify the body, while others are " +
            "read-only, such as speed and rotations over time. Some properties scale with the current " +
            "timescale and distance, as indicated by 'd' and 't'." +

            "<b>Scaling Distance</b>" +
            "The scrollwheel can be used to enlarge or shrink the size of the simulation. Specific " +
            "distances can be selected from the list of metrics in the toolbox. The scale factor in " +
            "the bottom right displays the scale of the simulation relative to the current unit selected." +

            "<b>Time Adjustments<b/>" +
            "The time controls in the toolbox can be used to pause or resume the simulation. There " +
            "are also controls to speed up or slow down the rate of the simulation. The time label " +
            "in the bottom right displays how much time passes per second." +

            "<b>Orbits</b>" +
            "Selecting orbit mode allows bodies to be set in a stable orbit around other bodies. " +
            "The eccentricity and distance to the periapsis and apoapsis are displayed. Using " +
            "keyboard controls, the eccentricity and elevation of the orbit can be adjusted." +

            "<b>Body Tools</b>" +
            "After selecting a body, a separate set of body tools will appear in the top right. " +
            "These can be used to affect a body directly, such as following it or toggling its " +
            "label or trail. A template of the body can also be created.";

        pages[4] = "<b>Simulation Options</b>" +
            "During a simulation, the name, description, and gravitational constant can be changed. " +
            "The quality of collisions and the lifespan of body trails can also be adjusted. These " +
            "changes will only be reflected if saved.";

        pages[5] = "<b>Saving Simulations</b>" +
            "Simulations can be saved with a name and description to come back to later. These will " +
            "appear in the list of saved simulations, which can be accessed from the main menu. " +
            "Opening the save popup will pause the simulation while you enter the details, and will " +
            "need to be resumed when closed." +

            "Simulations can also be saved as templates in the same way, which can be accessed " +
            "from the main menu as well." +

            "Saving a simulation for the first time will prompt you to save the simulation as a " +
            "new save or template, just as above.";

        pages[6] = "<b>Creating Body Templates</b>"
            + "After selecting a body, the body tools list will appear with an option to save the " +
            "body as a template. This opens a popup allowing you to save the body with a name " +
            "and description. " +
        
            "The simulation will be paused while providing the details and will need to be resumed " +
            "when the popup is closed.";

        pages[7] = "<b>Controls</b>" +
            "Below is the entire list of controls available for the application.";

        pages[8] = "<b>End of Tutorial</b>" +
            "You've reached the end of the tutorial - have fun simulation!";

        return pages;
    }

    #endregion
}