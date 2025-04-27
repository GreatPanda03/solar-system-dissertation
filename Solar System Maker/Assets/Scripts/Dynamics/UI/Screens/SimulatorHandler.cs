// creates an instance of the "Simulator" interface
// * the interface is a medium to interact with the Simulator

/* REFERENCES:
* [1] - Game Dev Beginner, "How to convert the mouse position to world space in Unity", www.youtube.com [Online]. Available at: https://www.youtube.com/watch?v=5NTmxDSKj-Q
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;


public class SimulatorHandler
{

    #region Attributes

    public VisualElement ui;
    public UIController uiController;
    private SimulationController simController;
    public VisualTreeAsset explorerEntry;
    public VisualTreeAsset optionsPopup;
    public VisualTreeAsset savePopup;
    public VisualTreeAsset bodyTemplatePopup;
    public VisualTreeAsset helpPopup;
    
    private string selectedBodyItem;
    private VisualElement focusedField;

    #endregion Attributes


    #region Constructor

    // prepares a new instance of the "Simulator" interface
    public SimulatorHandler
    (
        VisualElement ui, 
        UIController uiController,
        SimulationController simController, 
        VisualTreeAsset explorerEntry,
        VisualTreeAsset optionsPopup,
        VisualTreeAsset savePopup,
        VisualTreeAsset bodyTemplatePopup,
        VisualTreeAsset helpPopup
    ) {
        this.ui = ui;
        this.uiController = uiController;
        this.simController = simController;
        this.explorerEntry = explorerEntry;
        this.optionsPopup = optionsPopup;
        this.savePopup = savePopup;
        this.bodyTemplatePopup = bodyTemplatePopup;
        this.helpPopup = helpPopup;

        ui.Q<VisualElement>("ToolBar").Q<Label>("Title").text = simController.simulation.settings.Name;
        ui.Q<Button>("Options").clicked += OnOptionsClicked;
        ui.Q<Button>("Save").clicked += OnSaveClicked;
        ui.Q<Button>("SaveAs").clicked += OnSaveAsClicked;
        ui.Q<Button>("Help").clicked += OnHelpClicked;
        ui.Q<Button>("ExitSimulator").clicked += OnExitSimulatorClicked;

        ui.Q<Button>("Explorer").clicked += OnExplorerClicked;
        InitialiseExplorerSearchBar();
        ui.Q("Explorer").Q<Button>("Close").clicked += OnExplorerCloseClicked;
        ui.Q<VisualElement>("Explorer").style.display = DisplayStyle.None;
        RefreshExplorer();

        ui.Q<Button>("Bodies").clicked += OnBodiesClicked;
        InitialiseBodyItemButtons();
        ui.Q<VisualElement>("BodyList").style.display = DisplayStyle.None;

        ui.Q<Button>("Modes").clicked += OnModesClicked;
        InitialiseModeButtons();
        SelectModeItem("FreePlacement");
        ui.Q<VisualElement>("ModeList").style.display = DisplayStyle.None;

        ui.Q<Button>("Distance").clicked += OnDistanceClicked;
        InitialiseDistanceButtons();
        SelectDistanceItem("Kilometre");
        ui.Q<VisualElement>("DistanceList").style.display = DisplayStyle.None;

        UpdateScaleLabel();

        ui.Q<Button>("ToggleLabels").clicked += OnToggleLabelsClicked;

        ui.Q<Button>("ToggleTrails").clicked += OnToggleTrailsClicked;

        ui.Q<Button>("TogglePlay").clicked += OnTogglePlayClicked;
    
        ui.Q<Button>("TogglePlay").Q<VisualElement>("Icon").style.backgroundImage 
            = Resources.Load<Texture2D>($"Icons/Toolbox/Options/{(simController.IsRunning ? "Paused" : "Playing")}");

        ui.Q<Button>("ToggleFlow").clicked += OnToggleFlowClicked;
        simController.IsRewinding = false;

        InitialiseBodyTools();
        SetBodyToolsVisible(false);

        InitialisePropertyFields();
        ui.Q<Button>("Close").clicked += OnPropertiesCloseClicked;
        SetPropertiesVisible(false);
        UpdateProperties(null); // hide fields too

        SubscribeEvents();
        InitialiseInputs();
    }

    #endregion Constructor


    #region Event Methods

    public void SubscribeEvents()
    {
        simController.OnBodyAdded += OnBodyAdded;
        simController.OnBodyRemoved += OnBodyRemoved;
        simController.OnBodySelected += OnBodySelected;
        simController.OnBodyDeselected += OnBodyDeselected;
        simController.OnScaleChanged += OnScaleChanged;
        simController.OnPhysicsUpdate += OnPhysicsUpdate;
    }

    public void UnsubscribeEvents()
    {
        simController.OnBodyAdded -= OnBodyAdded;
        simController.OnBodyRemoved -= OnBodyRemoved;
        simController.OnBodySelected -= OnBodySelected;
        simController.OnBodyDeselected -= OnBodyDeselected;
        simController.OnScaleChanged -= OnScaleChanged;
        simController.OnPhysicsUpdate -= OnPhysicsUpdate;
    }

    #endregion Event Methods


    #region Input Controls

    public void InitialiseInputs()
    {
        // NB: "ui.panel.visualTree" so keybinds persist even when UI not selected!
        ui.panel.visualTree.RegisterCallback<KeyDownEvent>((e) => {
            if (UIController.FocusedField != null) return;

            // toggling explorer visibility on "Tab" pressed
            if (e.keyCode == AppControls.keybinds["Explorer"])
            {
                bool visible = ui.Q<VisualElement>("Explorer").style.display == DisplayStyle.Flex;
                SetExplorerVisible(!visible);
            } 

            // deselecting the current body on "E" pressed
            else if (e.keyCode == AppControls.keybinds["Deselect Preview"])
            {
                DeselectBodyItem();
            }

            // toggling properties visibility on "P" pressed
            else if (e.keyCode == AppControls.keybinds["Properties"])
            {
                if (simController.SelectedBody != null)
                {
                    bool visible = ui.Q<VisualElement>("PropertiesPanel").style.display == DisplayStyle.Flex;
                    SetPropertiesVisible(!visible);
                }
            }

        }, TrickleDown.TrickleDown);

        // ^ NB: "TrickleDown" means you don't have to press it twice
    }

    #endregion Input Controls


    #region Simulator Events

    public void OnBodyAdded(Body addedBody)
    {
        CreateExplorerEntry(addedBody);
        
        ApplyExplorerFilters();

        if (simController.simulation.bodies.Count > 0)
        {
            ui.Q<Label>("NoBodies").style.display = DisplayStyle.None;
        }
    }

    public void OnBodyRemoved(Body removedBody)
    {
        if (simController.SelectedBody == removedBody)
        {   
            SetPropertiesVisible(false);
        }

        RefreshExplorer();
    }

    public void OnBodySelected(Body selectedBody)
    {
        SetBodyToolsVisible(true);
        SetPropertiesVisible(true);
    }

    public void OnBodyDeselected(Body deselectedBody)
    {
        SetBodyToolsVisible(false);
        SetPropertiesVisible(false);
        UpdateProperties(null);
    }

    public void OnScaleChanged(float newScale)
    {
        UpdateScaleLabel();
    }

    public void OnPhysicsUpdate()
    {
        if (ui.Q<VisualElement>("PropertiesPanel").style.display == DisplayStyle.Flex)
        {
            UpdateProperties(simController.SelectedBody);
        }
    }

    #endregion Simulator Events


    #region Toolbar Methods

    // sets the title of the interface at the top of the screen
    public void SetTitle(string title)
    {
        ui.Q<Label>("Title").text = title;
    }

    // displays the simulation's options on click
    public void OnOptionsClicked()
    {
        VisualElement optionsPopup = ui.Q<VisualElement>("OptionsPopup");

        if (optionsPopup == null)
        {
            uiController.ShowOptionsPopup(ui, simController);
        }
        else
        {
            optionsPopup.RemoveFromHierarchy();
        }
    }

    // on click - saves the simulation directly, or prompts for "Save As"
    //   if the simulation is being saved for the first time
    private void OnSaveClicked()
    {
        Simulation simulation = simController.simulation;

        if (DataHandler.UpdateSimulationSave(simulation.id, simulation))
        {
            Debug.Log("[successful save notification process]");
        }
        else
        {
            if (simController.IsRunning)
                OnTogglePlayClicked(); // pause

            uiController.ShowSaveSimulationPopup(ui, simController.simulation);
        }
    }

    // prompts to save the simulation as a new copy on click
    private void OnSaveAsClicked()
    {
        VisualElement savePopup = ui.Q<VisualElement>("SavePopup");

        if (savePopup == null)
        {
            uiController.ShowSaveSimulationPopup(ui, simController.simulation);
        }
        else
        {
            savePopup.RemoveFromHierarchy();
        }
    }

    // returns to the "Main Menu" interface
    private void OnExitSimulatorClicked()
    {
        // TODO: check if no save has been made - then prompt accordingly

        UnsubscribeEvents();

        simController.DisableSimulator();
        simController.ClearSimulator();
        
        uiController.ShowMainMenu();

        CameraController.SetCameraMode(CameraController.CameraMode.MainMenu);
    }

    #endregion Toolbar Methods


    #region Toolbox Methods

    public void OnExplorerClicked()
    {
        bool state = ui.Q<VisualElement>("Explorer").style.display == DisplayStyle.Flex;
        SetExplorerVisible(!state);        
    }

    public void OnBodiesClicked()
    {
        VisualElement bodyList = ui.Q<VisualElement>("BodyList");
        bodyList.style.display = bodyList.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;
    }

    public void OnModesClicked()
    {
        VisualElement modeList = ui.Q<VisualElement>("ModeList");
        modeList.style.display = modeList.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;
    }

    public void OnDistanceClicked()
    {
        VisualElement distanceList = ui.Q<VisualElement>("DistanceList");
        distanceList.style.display = distanceList.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;
    }
    
    private void OnToggleLabelsClicked()
    {
        simController.ShowLabels = !simController.ShowLabels;

        VisualElement disabledIcon = ui.Q<Button>("ToggleLabels").Q<VisualElement>("Disabled");
        disabledIcon.style.display = simController.ShowLabels ? DisplayStyle.None : DisplayStyle.Flex;

        // TODO: render body labels individually
    }

    private void OnToggleTrailsClicked()
    {
        simController.ShowTrails = !simController.ShowTrails;

        VisualElement disabledIcon = ui.Q<Button>("ToggleTrails").Q<VisualElement>("Disabled");
        disabledIcon.style.display = simController.ShowTrails ? DisplayStyle.None : DisplayStyle.Flex;
    }

    public void OnTogglePlayClicked()
    {
        if (simController.IsRunning)
        {
            simController.IsRunning = false;

            ui.Q<Button>("TogglePlay").Q<VisualElement>("Icon").style.backgroundImage 
                = Resources.Load<Texture2D>("Icons/Toolbox/Options/Playing");
        }
        else
        {
            simController.IsRunning = true;

            ui.Q<Button>("TogglePlay").Q<VisualElement>("Icon").style.backgroundImage
                = Resources.Load<Texture2D>("Icons/Toolbox/Options/Paused");
        }
    }

    public void OnToggleFlowClicked()
    {
        simController.IsRewinding = !simController.IsRewinding;

        if (simController.IsRewinding)
        {
            ui.Q<Button>("ToggleFlow").Q<VisualElement>("Icon").style.backgroundImage
                = Resources.Load<Texture2D>("Icons/Toolbox/Options/Rewind");
        }
        else
        {
            ui.Q<Button>("ToggleFlow").Q<VisualElement>("Icon").style.backgroundImage
                = Resources.Load<Texture2D>("Icons/Toolbox/Options/FastForward");
        }
    }

    #endregion Toolbox Functions


    #region Preview Selection

    // selects the respective body as a preview on click
    public void InitialiseBodyItemButtons()
    {
        foreach (var element in ui.Q<VisualElement>("BodyList").Children())
        {
            if (element is Button item)
            {
                item.clicked += () => {
                    if (item.name != selectedBodyItem)
                    {
                        SelectBodyItem(item.name);
                    }
                    else
                    {
                        DeselectBodyItem();
                    }
                };
            }
        }
    }

    // selects the given item for body previews
    public void SelectBodyItem(string newItem)
    {
        DeselectBodyItem();
        
        Button newButton = ui.Q<Button>(newItem);
        newButton.AddToClassList("selectedButton");
        selectedBodyItem = newItem;

        simController.SetEntityPreview(newItem);
    }

    // deselects the current item as a body preview
    public void DeselectBodyItem()
    {
        if (selectedBodyItem == null) return;

        Button button = ui.Q<Button>(selectedBodyItem);
        button.RemoveFromClassList("selectedButton");

        selectedBodyItem = null;
        
        simController.RemoveEntityPreview();
    }

    #endregion Preview Selection


    #region Mode Selection

    // selects the respective placement mode on click
    public void InitialiseModeButtons()
    {
        foreach (var element in ui.Q<VisualElement>("ModeList").Children())
        {
            if (element is Button mode)
            {
                mode.clicked += () => {
                    SelectModeItem(mode.name);
                };
            }
        }
    }

    // selects the given mode button
    public void SelectModeItem(string newMode)
    {
        Button oldButton = ui.Q<Button>(simController.placementMode.ToString());
        oldButton.RemoveFromClassList("selectedButton");

        simController.SetPlacementMode(newMode);

        Button newButton = ui.Q<Button>(newMode);
        newButton.AddToClassList("selectedButton");
    }

    // NB: a placement mode is ALWAYS selected - there can never be none

    #endregion Mode Selection


    #region Distance Selection

    // selects the respective distance measure on click
    public void InitialiseDistanceButtons()
    {
        foreach (var element in ui.Q<VisualElement>("DistanceList").Children())
        {
            if (element is Button distance)
            {
                distance.clicked += () => {
                    SelectDistanceItem(distance.name);
                };
            }
        }
    }

    // selects the given distance item
    public void SelectDistanceItem(string newItem)
    {
        string oldDistance = simController.renderer.Distance.Replace(" ", "_");
        string newDistance = newItem.Replace(" ", "");

        Button oldButton = ui.Q<Button>(oldDistance);
        oldButton.RemoveFromClassList("selectedButton");

        Button newButton = ui.Q<Button>(newDistance);
        newButton.AddToClassList("selectedButton");

        simController.renderer.Distance = newItem.Replace("_", " ");
        simController.renderer.ZoomFactor = 0;

        UpdateScaleLabel();
    }

    // NB: a distance is ALWAYS selected - there can never be none

    #endregion Distance Selection


    #region Help Popup
    
    // toggles the visibility of the help popup on click
    public void OnHelpClicked()
    {
        if (ui.Q<VisualElement>("HelpPopup") == null)
        {
            DisplayHelpPopup();
        }
        else
        {
            CloseHelpPopup();
        }
    }

    // shows a help popup with the current controls
    public void DisplayHelpPopup()
    {
        VisualElement existingPopup = ui.Q<VisualElement>("HelpPopup");
        if (existingPopup != null) return;
        
        VisualElement popup = UIController.CloneInterface(helpPopup);
        popup.name = "HelpPopup";

        // load text based on current controls
        string finalText = "";

        foreach(KeyValuePair<string, KeyCode> pair in AppControls.keybinds)
            finalText += $"<b>{pair.Key}</b>   -   {pair.Value}<br>";

        popup.Q<Label>("Controls").text = finalText;

        popup.Q<Button>("Close").clicked += () => {
            popup.RemoveFromHierarchy();
        };

        ui.Add(popup);
    }

    // closes the existing help popup
    public void CloseHelpPopup()
    {
        VisualElement existingPopup = ui.Q<VisualElement>("HelpPopup");
        if (existingPopup == null) return;
        
        existingPopup.RemoveFromHierarchy();
    }

    #endregion Help Popup`


    #region Scaling Methods

    // updates the zoom label with the current scale and distance
    private void UpdateScaleLabel()
    {
        string zoomText = string.Format("<b>Scale:</b> 1 {0} x {1:0.000}", 
            simController.renderer.Distance, 
            1 / Math.Pow(10, simController.renderer.ZoomFactor));

        ui.Q<Label>("Scale").text = zoomText;
    }

    #endregion Scaling Methods


    
    #region Explorer

    // sets whether the explorer is visible or not
    public void SetExplorerVisible(bool visible)
    {
        ui.Q<VisualElement>("Explorer").style.display
            = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // enables bodies to be searched for by name
    public void InitialiseExplorerSearchBar()
    {
        VisualElement search = ui.Q<VisualElement>("Explorer")
            .Q<VisualElement>("Search");

        TextField input = search.Q<TextField>("Input");

        input.RegisterCallback<KeyDownEvent>((e) =>
        {
            if (e.keyCode == KeyCode.Return)
            {
                ApplyExplorerFilters();
            }
        }, TrickleDown.TrickleDown);

        input.RegisterValueChangedCallback((e) => 
        {
            ApplyExplorerFilters();
        });
    }

    // filters all explorer entries by the current explorer filters
    public void ApplyExplorerFilters()
    {
        VisualElement list = ui.Q<VisualElement>("Explorer").Q<VisualElement>("List");
        VisualElement container = list.Q<VisualElement>("unity-content-container");

        string searchQuery = GetExplorerSearchQuery().ToLower();

        foreach (VisualElement entry in container.Children())
        {
            string name = entry.Q<Label>("Name").text.ToLower();

            if (searchQuery == "" || name.Contains(searchQuery))
            {
                entry.style.display = DisplayStyle.Flex;
            }
            else
            {
                entry.style.display = DisplayStyle.None;
            }
        }
    }

    // auxiliary: returns the current search query
    public string GetExplorerSearchQuery()
    {
        VisualElement search = ui.Q<VisualElement>("Explorer").Q<VisualElement>("Search");
        return search.Q<TextField>("Input").value;
    }

    // creates a new entry in the explorer for the given body
    public void RefreshExplorer()
    {
        ClearExplorer();

        List<Body> bodies = simController.GetBodies();

        if (bodies.Count > 0)
        {
            ui.Q<Label>("NoBodies").style.display = DisplayStyle.None;

            foreach (Body body in simController.GetBodies())
            {
                CreateExplorerEntry(body);
            }

            ApplyExplorerFilters();
        }
        else
        {
            ui.Q<Label>("NoBodies").style.display = DisplayStyle.Flex;
        }
    }

    // clears the explorer
    public void ClearExplorer()
    {
        VisualElement explorer = ui.Q<VisualElement>("Explorer");
        explorer.Q<ScrollView>("List").Clear();
        explorer.Q<Label>("NoBodies").style.display = DisplayStyle.Flex;
    }

    // creates an entry for the given body
    public VisualElement CreateExplorerEntry(Body body)
    {
        VisualElement entry = explorerEntry.Instantiate();

        entry.Q<Label>("Type").text = body.Type;
        entry.Q<Label>("Name").text = body.Name;
        entry.Q<Button>("Delete").visible = false;

        ui.Q<ScrollView>("List").Add(entry);

        entry.RegisterCallback<ClickEvent>(e =>
        {
            simController.SelectBody(body);
            CameraController.SetSubject(body);
            CameraController.SetCameraMode(CameraController.CameraMode.Following);
        });

        entry.RegisterCallback<MouseEnterEvent>(e =>
        {
            entry.Q<Button>("Delete").visible = true;
        });

        entry.RegisterCallback<MouseLeaveEvent>(e =>
        {
            entry.Q<Button>("Delete").visible = false;
        });

        entry.Q<Button>("Delete").clicked += () => 
        {
            simController.RemoveBody(body);
        };

        return entry;
    }

    public void OnExplorerCloseClicked()
    {
        SetExplorerVisible(false);
    }

    #endregion Explorer


    #region Body Tools

    // shows / hides the body tools panel
    public void SetBodyToolsVisible(bool visible)
    {
        VisualElement bodyTools = ui.Q<VisualElement>("BodyTools");
        bodyTools.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

        // updating body tool button visuals accordingly
        if (visible)
        {
            bodyTools.Q<Button>("Trail").Q<VisualElement>("Disabled")
                .style.display = simController.SelectedBody.ShowTrail ? DisplayStyle.None : DisplayStyle.Flex;

            bodyTools.Q<Button>("Label").Q<VisualElement>("Disabled")
                .style.display = simController.SelectedBody.ShowLabel ? DisplayStyle.None : DisplayStyle.Flex;

            bodyTools.Q<Button>("Anchored").Q<VisualElement>("Disabled")
                .style.display = simController.SelectedBody.Anchored ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }

    // prepares each button in the body tools panel
    public void InitialiseBodyTools()
    {
        VisualElement bodyTools = ui.Q<VisualElement>("BodyTools");

        // toggle the body's properties on click
        bodyTools.Q<Button>("Properties").clicked += () => {
            bool visible = ui.Q<VisualElement>("PropertiesPanel").style.display == DisplayStyle.Flex;
            SetPropertiesVisible(!visible);
        };

        // follows the body on click
        bodyTools.Q<Button>("Follow").clicked += () => {
            CameraController.SetSubject(simController.SelectedBody);
            CameraController.SetCameraMode(CameraController.CameraMode.Following);
        };

        // adjusts the scale to the selected body on click
        bodyTools.Q<Button>("Scale").clicked += () => {
            CameraController.SetSubject(simController.SelectedBody);
            CameraController.SetCameraMode(CameraController.CameraMode.Following);
            simController.ScaleToFit(simController.SelectedBody);
        };

        // toggles the body's trail path on click
        bodyTools.Q<Button>("Trail").clicked += () => {
            if (simController.SelectedBody == null) return;
            
            bool visible = !simController.SelectedBody.ShowTrail;
            simController.SelectedBody.ShowTrail = visible;

            // keep the trail hidden if all labels are hidden
            if (!simController.ShowTrails)
                simController.SelectedBody.SetTrailVisibility(false);

            bodyTools.Q<Button>("Trail").Q<VisualElement>("Disabled").style.display
                = visible ? DisplayStyle.None : DisplayStyle.Flex;
        };

        // toggles the body's label on click
        bodyTools.Q<Button>("Label").clicked += () => {
            if (simController.SelectedBody == null) return;
            
            bool visible = !simController.SelectedBody.ShowLabel;
            simController.SelectedBody.ShowLabel = visible;

            // keep the label hidden if all labels are hidden
            if (!simController.ShowLabels)
                simController.SelectedBody.SetLabelVisibility(false);

            bodyTools.Q<Button>("Label").Q<VisualElement>("Disabled").style.display
                = visible ? DisplayStyle.None : DisplayStyle.Flex;
        };

        // toggles the body's anchor on click (affected by gravity or not)
        bodyTools.Q<Button>("Anchored").clicked += () => {
            if (simController.SelectedBody == null) return;

            bool anchored = !simController.SelectedBody.Anchored;
            simController.SelectedBody.Anchored = anchored;

            bodyTools.Q<Button>("Anchored").Q<VisualElement>("Disabled").style.display
                = anchored ? DisplayStyle.None : DisplayStyle.Flex;
        };

        // opens a prompt to save the selected body as a template
        bodyTools.Q<Button>("Template").clicked += () => {
            if (simController.SelectedBody == null) return;

            if (simController.IsRunning)
                OnTogglePlayClicked(); // pause

            uiController.ShowBodyTemplatePopup(ui, simController.SelectedBody);
        };

        // deletes the body on click
        bodyTools.Q<Button>("Delete").clicked += () => {
            if (simController.SelectedBody == null) return;

            simController.RemoveBody(simController.SelectedBody);
        };
    }

    #endregion Body Tools


    #region Properties
    
    // changes the selected body's properties when any property field is changed
    private void InitialisePropertyFields()
    {
        VisualElement properties = ui.Q<VisualElement>("PropertiesPanel");
        VisualElement groups = properties.Q<VisualElement>("Groups");

        foreach (VisualElement group in groups.Children())
        {
            foreach (VisualElement field in group.Children())
            {
                string property = field.name;

                if (field is TextField textField)
                {
                    InitialisePropertyField(textField, property);
                }
                else if (field is FloatField floatField)
                {
                    InitialisePropertyField(floatField, property);
                }
                else if (field is Vector3Field vectorField)
                {
                    InitialisePropertyField(vectorField, property);
                }
            }
        }
    }

    // sets up the given property field so that, when enter is pressed,
    //   the corresponding property is updated with the value provided
    private void InitialisePropertyField<T>(BaseField<T> field, string property)
    {   
        field.RegisterCallback<KeyDownEvent>((e) =>
        {
            Body selectedBody = simController.SelectedBody;

            if (selectedBody == null) return;

            if (e.keyCode == KeyCode.Return && selectedBody != null)
            {
                var value = field.value;
                PropertyInfo info = selectedBody.GetType().GetProperty(property);
                info.SetValue(selectedBody, field.value);
                field.Blur(); // unfocuses the field

                RefreshExplorer();
            }
        }, TrickleDown.TrickleDown);

        // ^ NB: "TrickleDown" makes it only require one Enter press

        field.RegisterCallback<FocusEvent>((e) => // after gaining focus
        {
            focusedField = field;
        });

        field.RegisterCallback<FocusOutEvent>((e) => // before losing focus
        {
            focusedField = null;
        });
    }

    // sets the visiblity of the properties window
    public void SetPropertiesVisible(bool visible)
    {
        ui.Q<VisualElement>("PropertiesPanel").style.display 
            = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // shows the properties window with the given body's details
    public void UpdateProperties(Body body)
    {
        VisualElement properties = ui.Q<VisualElement>("PropertiesPanel");
        VisualElement groups = properties.Q<VisualElement>("Groups");

        if (body == null)
        {
            groups.style.display = DisplayStyle.None;
            properties.Q<Label>("NoSelection").style.display = DisplayStyle.Flex;
        }
        else 
        {
            Type type = body.GetType();
            var properties_ = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            // direct property fields
            foreach (var property in properties_)
            {
                VisualElement field = groups.Q(property.Name);
                if (field == null) continue;

                if (property.GetValue(body) == null)
                {
                    field.style.display = DisplayStyle.None;
                }
                else
                {   
                    field.style.display = DisplayStyle.Flex;

                    // don't update when focused (to permit editing)
                    if (field is TextField textField)
                    {
                        if (focusedField == textField) continue;
                        textField.value = (string) property.GetValue(body);
                    } 
                    else if (field is FloatField floatField)
                    {
                        if (focusedField == floatField) continue;
                        floatField.value = (float) property.GetValue(body);
                    } 
                    else if (field is Vector3Field vectorField)
                    {
                        if (focusedField == vectorField) continue;
                        vectorField.value = (Vector3) property.GetValue(body);
                    }
                }
            }

            // special case fields
            properties.Q<FloatField>("Speed").value = body.Velocity.magnitude
                * simController.renderer.Scale;

            properties.Q<Label>("NoSelection").style.display = DisplayStyle.None;
            groups.Q<VisualElement>("Star").style.display = body is Star ? DisplayStyle.Flex : DisplayStyle.None;
            groups.Q<VisualElement>("Planet").style.display = body is Planet ? DisplayStyle.Flex : DisplayStyle.None;
            groups.style.display = DisplayStyle.Flex;
        }
    }

    // hides the properties window when the close button is clicked
    public void OnPropertiesCloseClicked()
    {
        SetPropertiesVisible(false);
    }

    #endregion Properties
}
