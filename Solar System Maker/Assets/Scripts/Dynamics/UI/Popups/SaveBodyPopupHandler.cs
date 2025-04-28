// manages instances of Body Template popups
// * these popups allow the user to save the given body as a template

using UnityEngine.UIElements;

public class SaveBodyPopupHandler : IPopup
{
    
    #region Attributes

    public static string identifier = "SaveBodyPopup";
    public VisualElement popup;
    public UIController uiController;
    public VisualElement parent;
    public Body body;

    #endregion Attributes


    #region Constructor

    // prepares the given popup as an "Body Template Popup" bound to the given parent interface
    public SaveBodyPopupHandler(
        VisualElement popup, 
        UIController uiController,
        VisualElement parent,
        Body body)
    {
        this.popup = popup;
        this.uiController = uiController;
        this.parent = parent;
        this.body = body;

        if (parent.Q<VisualElement>(identifier) != null) return;

        popup.name = identifier;
        parent.Add(popup);

        popup.Q<TextField>("Name").value = body.Name;

        UIController.PrepareFieldFocus(popup);

        popup.Q<Button>("Save").clicked += () => {
            if (!ValidateDetails(popup)) return;

            popup.RemoveFromHierarchy();

            BodyTemplate newSave = DataHandler.GetNextBodyTemplateSlot();
            newSave.Name = popup.Q<TextField>("Name").value;
            newSave.Description = popup.Q<TextField>("Description").value;
            newSave.Body = body;

            DataHandler.BodyTemplates.Add(newSave);
        }; 

        popup.Q<Button>("Close").clicked += () => {
            popup.RemoveFromHierarchy();
        };
    }

    #endregion Constructor

    
    #region Validation

    // validates the body template's details
    public bool ValidateDetails(VisualElement popup)
    {
        var name = popup.Q<TextField>("Name");

        if (name.value == "")
        {
            DisplayValidation("<b>'Name'</b> cannot be blank!");
            return false;
        }

        // description can be blank

        return true;
    }

    // displays the given message as a validation message
    public void DisplayValidation(string message)
    {
        popup.Q<Label>("Validation").text = message;
    }

    #endregion Validation


    // extraction is simple enough
}