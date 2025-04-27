// manages instances of Options popups
// * these popups allow the user to edit settings of the given simulation instance

using UnityEngine.UIElements;

public class OptionsPopupHandler
{

    #region Attributes

    public VisualElement popup;
    public UIController uiController;
    public VisualElement parent;
    public SimulationController simController;

    #endregion Attributes


    #region Constructor

    // prepares the given popup as an "Options Popup", bound to
    //   the given parent interface and simulation instance
    public OptionsPopupHandler(
        VisualElement popup, 
        UIController uiController, 
        VisualElement parent, 
        SimulationController simController)
    {
        this.popup = popup;
        this.uiController = uiController;
        this.parent = parent;
        this.simController = simController;

        if (parent.Q<VisualElement>("OptionsPopup") != null) return;

        popup.name = "OptionsPopup";
        parent.Add(popup);

        UIController.PrepareFieldFocus(popup);

        Settings settings = simController.simulation.settings;
        popup.Q<TextField>("Name").value = settings.Name;
        popup.Q<TextField>("Description").value = settings.Description;
        popup.Q<FloatField>("GravitationalConstant").value = settings.GravitationalConstant;
        popup.Q<DropdownField>("CollisionQuality").value = simController.physics.collisionQuality.ToString();

        popup.Q<Button>("Reset").clicked += () => {
            Settings settings = simController.simulation.settings;
            popup.Q<TextField>("Name").value = settings.Name;
            popup.Q<TextField>("Description").value = settings.Description;
            popup.Q<FloatField>("GravitationalConstant").value = settings.GravitationalConstant;
            popup.Q<DropdownField>("CollisionQuality").value = simController.physics.collisionQuality.ToString();
        };

        popup.Q<Button>("Save").clicked += () => {
            if (!ValidateDetails(popup)) return;

            popup.RemoveFromHierarchy();

            Settings settings = GetOptionsDetailsAsSettings(popup);
            simController.simulation.settings = settings;

            string quality = popup.Q<DropdownField>("CollisionQuality").value;
            simController.physics.SetCollisionQuality(quality);

            parent.Q<VisualElement>("ToolBar").Q<Label>("Title").text = settings.Name;
        }; 

        popup.Q<Button>("Close").clicked += () => {
            popup.RemoveFromHierarchy();
        };
    }

    #endregion Constructor


    #region Validation

    // validates the option panel's details
    public bool ValidateDetails(VisualElement popup)
    {
        var name = popup.Q<TextField>("Name");

        if (name.value == "")
        {
            DisplayValidation("<b>'Name'</b> cannot be blank!");
            return false;
        }

        // description can be blank
        
        var gravitationalConstant = popup.Q<FloatField>("GravitationalConstant");

        if (gravitationalConstant.value == float.NaN || gravitationalConstant.value == 0)
        {
            DisplayValidation("<b>'G'</b> must be greater than 0!");
            return false;
        }

        var collisionQuality = popup.Q<DropdownField>("CollisionQuality");

        if (collisionQuality.value == "")
        {
            DisplayValidation("<b>'Collisions'</b> cannot be blank!");
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
        
    // gathers the settings-related options data
    public Settings GetOptionsDetailsAsSettings(VisualElement popup)
    {
        var name = popup.Q<TextField>("Name");
        var description = popup.Q<TextField>("Description");
        var gravitationalConstant = popup.Q<FloatField>("GravitationalConstant");

        return new()
        {
            Name = name.value,
            Description = description.value,
            GravitationalConstant = gravitationalConstant.value
        };
    }

    #endregion Extraction
}