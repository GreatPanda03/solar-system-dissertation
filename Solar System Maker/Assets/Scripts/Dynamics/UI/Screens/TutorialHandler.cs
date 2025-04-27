// creates instances of the "Tutorial" interface
// * the interface shows how to use the application

using UnityEngine.UIElements;

public class TutorialHandler
{
    
    #region Attributes

    public VisualElement ui;
    public UIController uiController;

    #endregion Attributes


    #region Constructor

    // prepares the given interface as a "Tutorial" interface
    public TutorialHandler(VisualElement ui, UIController uiController)
    {
        this.ui = ui;
        this.uiController = uiController;
        
        ui.Q<Button>("Close").clicked += OnCloseClicked;
    }

    #endregion Constructor


    #region UI Methods

    // returns to the "Main Menu" interface on click
    private void OnCloseClicked()
    {
        uiController.ShowMainMenu();
    }

    #endregion UI Methods
}