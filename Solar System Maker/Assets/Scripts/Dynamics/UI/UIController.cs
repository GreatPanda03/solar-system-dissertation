using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{

    #region Attributes

    public UIDocument uiDocument;
    public VisualTreeAsset mainMenuUI;
    public VisualTreeAsset newSimulationUI;
    public VisualTreeAsset savedSimulationsUI;
    public VisualTreeAsset saveEntryTemplate;
    public VisualTreeAsset templateSimulationsUI;
    public VisualTreeAsset tutorialUI;
    public VisualTreeAsset simulatorUI;
    public VisualTreeAsset explorerEntryTemplate;
    public VisualTreeAsset createSaveTemplate;
    public VisualElement root; // main screen holder

    #endregion Attributes


    #region Start Up

    void Start()
    {
        root = uiDocument.rootVisualElement;
        ShowMainMenu();
    }

    #endregion Start Up


    #region Screen Methods

    // auxiliary: returns the clone of the true root of the given interface
    public static VisualElement CloneInterface(VisualTreeAsset screen)
    {
        VisualElement clone = screen.CloneTree();
        clone.style.width = new Length(100, LengthUnit.Percent);
        clone.style.height = new Length(100, LengthUnit.Percent);
        
        return clone;
    }

    // shows a new instance of the "Main Menu" interface
    public void ShowMainMenu()
    {
        root.Clear();
        VisualElement screen = CloneInterface(mainMenuUI);
        root.Add(screen);
        new MainMenuHandler(screen, this);
    }

    // shows a new instance of the "New Simulations" interface
    public void ShowNewSimulation()
    {
        root.Clear();
        VisualElement screen = CloneInterface(newSimulationUI);
        root.Add(screen);
        new NewSimulationHandler(screen, this);
    }

    // shows a new instance of the "Saved Simulations" interface
    public void ShowSavedSimulations()
    {
        root.Clear();
        VisualElement screen = CloneInterface(savedSimulationsUI);
        root.Add(screen);
        new SavedSimulationsHandler(screen, this, saveEntryTemplate);
    }

    // shows a new instance of the "Template Simulations" interface
    public void ShowTemplateSimulations()
    {
        root.Clear();
        VisualElement screen = CloneInterface(templateSimulationsUI);
        root.Add(screen);
        new TemplateSimulationsHandler(screen, this);
    }

    // shows a new instance of the "Tutorial" interface
    public void ShowTutorial()
    {
        root.Clear();
        VisualElement screen = CloneInterface(tutorialUI);
        root.Add(screen);
        new TutorialHandler(screen, this);
    }

    // shows a new instance of the "Simulator" interface
    public void ShowSimulator()
    {
        root.Clear();
        VisualElement screen = CloneInterface(simulatorUI);
        root.Add(screen);

        new SimulatorHandler(screen, 
            this, 
            GameObject.Find("Simulator").GetComponent<SimulationController>(), 
            explorerEntryTemplate,
            createSaveTemplate);
    }

    #endregion Screen Methods


    #region General Methods

    // returns True if the mouse has any UI selected (for button clicks)
    public bool IsMouseOverUI()
    {
        return false;

        /* METHOD 3
        List<VisualElement> allElements = root.Q<VisualElement>("UI-container")
            .Query<VisualElement>().ToList();

        foreach (VisualElement element in allElements)
        {
            if (element is Button button)
            {
                Vector2 mousePosition = Input.mousePosition;
                float X = mousePosition.x;
                float Y = Screen.height - mousePosition.y;

                Rect extents = button.worldBound;

                Vector2 TL = extents.position; // top left
                Vector2 BR = extents.position; // bottom right

                if (TL.x <= X && X <= BR.x  &&  TL.y <= Y && Y <= BR.y)
                {
                    Debug.Log(button.name);
                    Debug.Log(button.worldBound);
                    Debug.Log(X);
                    Debug.Log(Y);
                    Debug.Log(TL);
                    Debug.Log(BR);
                    Debug.Log(button);
                    return true;
                }
            }   
        }

        // allElements.Any(e => e.worldBound.Contains(mousePos));

        Debug.Log("No UI hit");
        return false;
        */

        /* METHOD 2
        Debug.Log(EventSystem.current.currentSelectedGameObject);

        return EventSystem.current.currentSelectedGameObject != null;
        */

        /* METHOD 1
        Vector2 mousePosition = Input.mousePosition;
        mousePosition.y = Screen.height - mousePosition.y;
        // ^ UI works from bottom left, not top left (hence flip)

        VisualElement picked = uiDocument.rootVisualElement.panel?.Pick(mousePosition);
        // ^ if "empty UI space" is clicked, it'll be the panel (not null)
        //   if a button (etc.) is clicked, it WILL be null

        Debug.Log(picked);

        return picked == null;
        */
    }

    // returns True if the user currently has a text box selected
    public static bool IsTextFieldFocused()
    {
        var uiDocuments = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);

        foreach (var doc in uiDocuments)
        {
            var panel = doc.rootVisualElement?.panel;
            var focused = panel?.focusController?.focusedElement;

            if (focused is TextField)
            {
                return true;
            }
        }

        return false;
    }

    #endregion General Methods
}