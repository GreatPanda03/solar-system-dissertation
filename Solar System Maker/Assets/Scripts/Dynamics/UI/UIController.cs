// manages core and common UI functionality

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{

    #region Attributes

    public UIDocument uiDocument;
    public VisualTreeAsset mainMenuUI;
    public VisualTreeAsset newSimulationUI;
    public VisualTreeAsset savedSimulationsUI;
    public VisualTreeAsset saveEntry;
    public VisualTreeAsset templateSimulationsUI;
    public VisualTreeAsset tutorialPopup;
    public VisualTreeAsset simulatorUI;
    public VisualTreeAsset explorerEntry;
    public VisualTreeAsset saveBodyPopup;
    public VisualTreeAsset bodyTemplatesPopup;
    public VisualTreeAsset optionsPopup;
    public VisualTreeAsset saveSimulationPopup;
    public VisualTreeAsset tooltip;
    public VisualElement root; // main screen holder

    private static bool mousePressedLast = false;

    private static VisualElement focusedField;
    public static VisualElement FocusedField {
        get => focusedField;
        set => focusedField = value;
    }

    #endregion Attributes


    #region Start Up

    void Start()
    {
        root = uiDocument.rootVisualElement;

        // used to determine whether to focus on fields or not 
        // * using "navigational keys" (namely WASD) interferes with this
        //     so occassionally fields are selected
        root.panel.visualTree.RegisterCallback<MouseDownEvent>((e) => { 
            mousePressedLast = true;
        }, TrickleDown.TrickleDown);

        root.panel.visualTree.RegisterCallback<KeyDownEvent>((e) => {
            mousePressedLast = false;
        }, TrickleDown.TrickleDown);

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
        
        PrepareScreen(root);
    }

    // shows a new instance of the "New Simulations" interface
    public void ShowNewSimulation()
    {
        root.Clear();
        VisualElement screen = CloneInterface(newSimulationUI);
        root.Add(screen);
        new NewSimulationHandler(screen, this);
        
        PrepareScreen(root);
    }

    // shows a new instance of the "Saved Simulations" interface
    public void ShowSavedSimulations()
    {
        root.Clear();
        VisualElement screen = CloneInterface(savedSimulationsUI);
        root.Add(screen);
        new SavedSimulationsHandler(screen, this, saveEntry);
        
        PrepareScreen(root);
    }

    // shows a new instance of the "Template Simulations" interface
    public void ShowTemplateSimulations()
    {
        root.Clear();
        VisualElement screen = CloneInterface(templateSimulationsUI);
        root.Add(screen);
        new TemplateSimulationsHandler(screen, this, saveEntry);
        
        PrepareScreen(root);
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
            explorerEntry,
            optionsPopup,
            saveSimulationPopup,
            saveBodyPopup);

        PrepareScreen(screen);
    }

    #endregion Screen Methods


    #region Popup Methods

    // shows a new instance of a "Tutorial" popup for the given screen
    public void ShowTutorialPopup(VisualElement screen)
    {
        VisualElement popup = CloneInterface(tutorialPopup);
        new TutorialPopupHandler(popup, this, screen);
    }

    // shows a new instance of an "Options" popup for the given screen and simulation instance
    public void ShowOptionsPopup(VisualElement screen, SimulationController simController)
    {
        VisualElement popup = CloneInterface(optionsPopup);
        new OptionsPopupHandler(popup, this, screen, simController);
    }

    // shows a new instance of a "Save Simulation" popup for the given screen and simulation
    public void ShowSaveSimulationPopup(VisualElement screen, Simulation simulation)
    {
        VisualElement popup = CloneInterface(saveSimulationPopup);
        new SaveSimulationPopupHandler(popup, this, screen, simulation);
    }

    // shows a new instance of a "Save Body" popup for the given screen and body instance
    public void ShowSaveBodyPopup(VisualElement screen, Body body)
    {
        VisualElement popup = CloneInterface(saveBodyPopup);
        new SaveBodyPopupHandler(popup, this, screen, body);
    }

    // shows a new instance of a "Body Templates" popup for the given screen
    public BodyTemplatesPopupHandler ShowBodyTemplatesPopup(VisualElement screen)
    {
        VisualElement popup = CloneInterface(bodyTemplatesPopup);
        return new BodyTemplatesPopupHandler(popup, this, saveEntry, screen);
    }

    #endregion Popup Methods

    
    #region General Methods

    // prepares general element functionality for the given screen
    public void PrepareScreen(VisualElement screen)
    {
        PrepareFieldFocus(screen);
        PrepareButtonTooltips(screen);
    }

    #endregion General Methods


    #region Field Focus

    // registers all field buttons of the given element to be marked as a focused field when focused
    // * also restricts focusing to clicks ONLY (no focus for navigation!)
    public static void PrepareFieldFocus(VisualElement parent)
    {
        List<VisualElement> allElements = parent.Query<VisualElement>().ToList();

        foreach (VisualElement element in allElements)
        {
            if (element is TextField || element is FloatField || element is Vector3Field)
            {
                // only focus if the field is clicked
                element.RegisterCallback<FocusEvent>((e) => { // after focus gained
                    if (!mousePressedLast)
                    {
                        element.Blur();
                    }
                    else
                    {
                        focusedField = element;
                    }
                });

                element.RegisterCallback<FocusOutEvent>((e) => { // before focus lost
                    if (element == focusedField)
                    {
                        focusedField = null;
                    }
                });
            }
        }
    }

    #endregion Field Focus


    #region Button Tooltips

    // for all buttons in the given visual, when a button is hovered over,
    //   any tooltip assigned to it is displayed over the button
    public void PrepareButtonTooltips(VisualElement parent)
    {
        float offset = 0;

        List<VisualElement> allElements = parent.Query<VisualElement>().ToList();

        foreach (VisualElement element in allElements)
        {
            if (element is Button button)
            {
                if (button.tooltip != "")
                {
                    VisualElement tip = null;

                    button.RegisterCallback<MouseEnterEvent>((e) => {
                        if (tip == null)
                        {
                            tip = CloneInterface(tooltip);
                            tip.Q<Label>("Text").text = button.tooltip;
                            tip.style.left = Input.mousePosition.x - offset;
                            tip.style.bottom = Input.mousePosition.y - offset;
                            parent.Add(tip);
                        }
                    });

                    button.RegisterCallback<MouseLeaveEvent>((e) => {
                        tip?.RemoveFromHierarchy();
                        tip = null;
                    });

                    button.clicked += () => {
                        tip?.RemoveFromHierarchy();
                        tip = null;
                    };
                }
            }
        }
    }

    #endregion Button Tooltips


    #region Mouse Checks

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

    #endregion Mouse Checks
}