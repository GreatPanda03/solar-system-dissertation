// Handles the Main Menu interface


// LIBRARIES
using UnityEngine;
using UnityEngine.UIElements;


// CLASS DEFINITION
public class TutorialUIController
{
    public static VisualElement ui;


    #region UI Setup

    // initialises the Tutorial interface
    public static void Initialise()
    {
        ui = GameObject.Find("TutorialUI").GetComponent<UIDocument>().rootVisualElement;
        
        ui.Q<Button>("CloseWindow").clicked += OnCloseWindowClicked;

        Debug.Log("[TUTORIAL] SUCCESS: UI Loaded");
    }

    #endregion UI Setup


    #region General UI

    // shows the Tutorial interface
    public static void ShowInterface()
    {
        ui.Q("Root").style.display = DisplayStyle.Flex;
    }

    // hides the Tutorial interface
    public static void HideInterface()
    {
        ui.Q("Root").style.display = DisplayStyle.None;
    }

    // closes the new Tutorial window and displays the Main Menu
    private static void OnCloseWindowClicked()
    {
        HideInterface();
        MainMenuUIController.ShowInterface();
    }

    #endregion General UI
}