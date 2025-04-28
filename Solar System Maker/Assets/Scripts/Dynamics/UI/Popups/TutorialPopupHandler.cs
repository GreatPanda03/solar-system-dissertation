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
}