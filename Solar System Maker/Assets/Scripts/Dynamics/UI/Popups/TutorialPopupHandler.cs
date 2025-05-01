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

    private int currentPage;
    Dictionary<int, string> pages;

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

        currentPage = 0; // index
        pages = LoadPages();

        ShowPage(0);
        
        popup.Q<Button>("Backwards").clicked += () => {
            FlipPageBackwards();
            ShowPage(currentPage);
        };

        popup.Q<Button>("Forwards").clicked += () => {
            FlipPageForwards();
            ShowPage(currentPage);
        };

        popup.Q<Button>("Close").clicked += () => {
            popup.RemoveFromHierarchy();
        };

        parent.Add(popup);
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

    // flips backwards to the previous page
    public void FlipPageBackwards()
    {
        currentPage -= 1;
        
        if (currentPage < 0) 
        {
            currentPage = 0;
        }
    }

    // flips forwards to the next page
    public void FlipPageForwards()
    {
        currentPage += 1;

        if (currentPage >= pages.Count)
        {
            currentPage = pages.Count - 1;
        }
    }

    // shows the content of the given page (by index)
    public void ShowPage(int page)
    {
        popup.Q<Label>("Page").text = pages[page];
        popup.Q<Label>("PageNumber").text = $"Page {page + 1} / {pages.Count}";
    }

    // loads all the text for all pages
    public Dictionary<int, string> LoadPages()
    {
        Dictionary<int, string> pages = new();

        // loads all controls as a string for the controls page
        string controlsText = "";

        foreach(KeyValuePair<string, KeyCode> pair in AppControls.keybinds)
            controlsText += $"<b>{pair.Key}</b>   -   {pair.Value}<br>";

        // introduction
        pages[0] = "<b>Welcome to the application!</b>" +
            "<br><br>The application allows you to view existing solar or planetary systems, like our own. " +
            "You can also create your own systems or bodies to analyse and use. " +

            "<br><br>The simulator can simulate basic physics, such as gravity, collisions. It can also " +
            "simulate basic effective temperature and habitability zones." +
            
            "<br><br>This tutorial covers the technical features available in the application and how to use them.";
        
        // new simulation
        pages[1] = "<b>Creating a Simulation</b>" +
            "<br><br>From the main menu, you can choose to create a new simulation with a name, description, " +
            "and gravitational constant. " + 
            
            "<br><br>The gravitational constant represents the strength of " +
            "gravity in the system and is relative to the real value of G. For example, 2G would " + 
            "be a gravity twice as strong as our universe's gravity. " +

            "<br><br>After creating a new simulation, a blank simulation space will be opened for you to " +
            "add bodies to.";

        // template simulations
        pages[2] = "<b>Loading Existing Simulations</b>" +
            "<br><br>Alternatively, you can also load premade or custom templates from the main menu. " +
            "<br><br>This will load all stored bodies starting from their last saved state. " + 
            
            "<br><br>Saved simulations are files which can be overwritten, while template simulations act " +
            "as starting points from which saves are created - templates cannot be overwritten!";

        // in a simulation
        pages[3] = "<b>In a Simulation</b>" +
            "<br><br>Once in a simulation, there'll be a blank space for you to work with and view." +

            "<br><br><b>Toolbox</b>" +
            "<br>The toolbox contains tools which you to manipulate the simulation and adjust " +
            "its details, such as add bodies or toggle body labels and trails." +

            "<br><br><b>Explorer</b>" +
            "<br>The explorer can be selected from the toolbox and shows all bodies in the simulation. " +
            "Bodies can be deleted directly from the explorer, or followed by selecting the " +
            "corresponding entry. Selecting a body from the entry will also display its properties." +

            "<br><br><b>Properties</b>" +
            "<br>Selecting a body displays its properties live in another panel, such as its mass, " +
            "radius, and density. Some fields can be modified to modify the body, while others are " +
            "read-only, such as speed and rotations over time. Some properties scale with the current " +
            "timescale and distance, as indicated by 'd' and 't'." +

            "<br><br><b>Scaling Distance</b>" +
            "<br>The scrollwheel can be used to enlarge or shrink the size of the simulation. Specific " +
            "distances can be selected from the list of metrics in the toolbox. The scale factor in " +
            "the bottom right displays the scale of the simulation relative to the current unit selected." +

            "<br><br><b>Time Adjustments</b>" +
            "<br>The time controls in the toolbox can be used to pause or resume the simulation. There " +
            "are also controls to speed up or slow down the rate of the simulation. The time label " +
            "in the bottom right displays how much time passes per second." +

            "<br><br><b>Orbits</b>" +
            "<br>Selecting orbit mode allows bodies to be set in a stable orbit around other bodies. " +
            "The eccentricity and distance to the periapsis and apoapsis are displayed. Using " +
            "keyboard controls, the eccentricity and elevation of the orbit can be adjusted." +

            "<br><br><b>Body Tools</b>" +
            "<br>After selecting a body, a separate set of body tools will appear in the top right. " +
            "These can be used to affect a body directly, such as following it or toggling its " +
            "label or trail. A template of the body can also be created.";

        // simulation options
        pages[4] = "<b>Simulation Options</b>" +
            "<br><br>During a simulation, the name, description, and gravitational constant can be changed. " +
            "The quality of collisions and the lifespan of body trails can also be adjusted. These " +
            "changes will only be reflected if saved.";

        // saving simulations (as saves & templates)
        pages[5] = "<b>Saving Simulations</b>" +
            "<br><br>Simulations can be saved with a name and description to come back to later. These will " +
            "appear in the list of saved simulations, which can be accessed from the main menu. " +
            "Opening the save popup will pause the simulation while you enter the details, and will " +
            "need to be resumed when closed." +

            "<br><br>Simulations can also be saved as templates in the same way, which can be accessed " +
            "from the main menu as well." +

            "<br><br>Saving a simulation for the first time will prompt you to save the simulation as a " +
            "new save or template, just as above.";

        // creating templates of bodies
        pages[6] = "<b>Creating Body Templates</b>"
            + "<br><br>After selecting a body, the body tools list will appear with an option to save the " +
            "body as a template. This opens a popup allowing you to save the body with a name " +
            "and description. " +
        
            "<br><br>The simulation will be paused while providing the details and will need to be resumed " +
            "when the popup is closed.";

        // full list of controls
        pages[7] = "<b>Controls</b>" +
            "<br><br>Below is the entire list of controls available for the application."

            + "<br><br>" + controlsText;

        // final page
        pages[8] = "<b>End of Tutorial</b>" +
            "<br><br>You've reached the end of the tutorial - have fun simulation!";

        return pages;
    }

    #endregion
}