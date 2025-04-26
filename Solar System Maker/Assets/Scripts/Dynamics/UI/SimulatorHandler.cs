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
    public VisualTreeAsset explorerEntryTemplate;
    public VisualTreeAsset optionsPopupTemplate;
    public VisualTreeAsset savePopupTemplate;
    
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
        VisualTreeAsset explorerEntryTemplate,
        VisualTreeAsset optionsPopupTemplate,
        VisualTreeAsset savePopupTemplate
    ) {
        this.ui = ui;
        this.uiController = uiController;
        this.simController = simController;
        this.explorerEntryTemplate = explorerEntryTemplate;
        this.optionsPopupTemplate = optionsPopupTemplate;
        this.savePopupTemplate = savePopupTemplate;

        ui.Q<VisualElement>("ToolBar").Q<Label>("Title").text = simController.simulation.settings.Name;
        ui.Q<Button>("Options").clicked += OnOptionsClicked;
        ui.Q<Button>("Save").clicked += OnSaveClicked;
        ui.Q<Button>("SaveAs").clicked += OnSaveAsClicked;
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

        ui.Q<Button>("TogglePlay").clicked += OnTogglePlayClicked;
    
        ui.Q<Button>("TogglePlay").Q<VisualElement>("Icon").style.backgroundImage 
            = Resources.Load<Texture2D>($"Icons/Toolbox/Options/{(simController.IsRunning ? "Paused" : "Playing")}");

        ui.Q<Button>("ToggleFlow").clicked += OnToggleFlowClicked;
        simController.IsRewinding = false;

        InitialisePropertyFields();
        ui.Q<Button>("Close").clicked += OnPropertiesCloseClicked;
        SetPropertiesVisible(false);

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

            // toggling explorer visibility on "Tab" pressed
            if (e.keyCode == KeyCode.Tab)
            {
                bool visible = ui.Q<VisualElement>("Explorer").style.display == DisplayStyle.Flex;
                SetExplorerVisible(!visible);
            } 

            // deselecting the current body on "E" pressed
            else if (e.keyCode == KeyCode.E)
            {
                DeselectBodyItem();
            }

            // toggling properties visibility on "P" pressed
            else if (e.keyCode == KeyCode.P)
            {
                bool visible = ui.Q<VisualElement>("Properties").style.display == DisplayStyle.Flex;
                SetPropertiesVisible(!visible);
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
        SetPropertiesVisible(true);
    }

    public void OnBodyDeselected(Body deselectedBody)
    {
        UpdateProperties(null);
    }

    public void OnScaleChanged(float newScale)
    {
        UpdateScaleLabel();
    }

    public void OnPhysicsUpdate()
    {
        if (ui.Q<VisualElement>("Properties").style.display == DisplayStyle.Flex)
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
        Debug.Log("[showing options]");
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
            CreateSavePopup();
        }
    }

    // prompts to save the simulation as a new copy on click
    private void OnSaveAsClicked()
    {
        CreateSavePopup();
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
        VisualElement explorerEntry = explorerEntryTemplate.Instantiate();

        explorerEntry.Q<Label>("Type").text = body.Type;
        explorerEntry.Q<Label>("Name").text = body.Name;
        explorerEntry.Q<Button>("Delete").visible = false;

        ui.Q<ScrollView>("List").Add(explorerEntry);

        explorerEntry.RegisterCallback<ClickEvent>(e =>
        {
            simController.SelectBody(body);
            CameraController.SetSubject(body);
            CameraController.SetCameraMode(CameraController.CameraMode.Following);
        });

        explorerEntry.RegisterCallback<MouseEnterEvent>(e =>
        {
            explorerEntry.Q<Button>("Delete").visible = true;
        });

        explorerEntry.RegisterCallback<MouseLeaveEvent>(e =>
        {
            explorerEntry.Q<Button>("Delete").visible = false;
        });

        explorerEntry.Q<Button>("Delete").clicked += () => 
        {
            simController.RemoveBody(body);
        };

        return explorerEntry;
    }

    public void OnExplorerCloseClicked()
    {
        SetExplorerVisible(false);
    }

    #endregion Explorer


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


    #region Properties
    
    // changes the selected body's properties when any property field is changed
    private void InitialisePropertyFields()
    {
        VisualElement properties = ui.Q<VisualElement>("Properties");
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
        ui.Q<VisualElement>("Properties").style.display 
            = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // shows the properties window with the given body's details
    public void UpdateProperties(Body body)
    {
        VisualElement properties = ui.Q<VisualElement>("Properties");
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


    #region Options Popup

    public void CreateOptionsPopup()
    {

    }

    #endregion Options Popup

    

    #region Save As Popup

    // creates a prompt to save the simulation as a new copy
    public void CreateSavePopup()
    {
        if (ui.Q<VisualElement>("SavePopup") != null) return;

        if (simController.IsRunning)
        {
            OnTogglePlayClicked();
        }

        VisualElement popup = UIController.CloneInterface(savePopupTemplate);
        popup.name = "SavePopup";
        ui.Add(popup);

        Settings settings = simController.simulation.settings;
        popup.Q<TextField>("Name").value = settings.Name;
        popup.Q<TextField>("Description").value = settings.Description;
        popup.Q<TextField>("GravitationalConstant").value = settings.GravitationalConstant.ToString();

        popup.Q<Button>("SaveSimulation").clicked += () => {
            if (!ValidateDetails(popup)) return;

            popup.RemoveFromHierarchy();

            Settings settings = GetDetailsAsSettings(popup);

            List<Body> bodyCopies = new();
            foreach (Body body in simController.simulation.bodies)
                bodyCopies.Add(body.GetCopy());

            Simulation newSave = DataHandler.CreateSimulationSave();
            newSave.settings = settings;
            newSave.bodies = bodyCopies;

            DataHandler.AddSimulationSave(newSave);

            ui.Q<VisualElement>("ToolBar").Q<Label>("Title").text = newSave.settings.Name;
        }; 

        popup.Q<Button>("SaveTemplate").clicked += () => {
            if (!ValidateDetails(popup)) return;

            popup.RemoveFromHierarchy();

            Settings settings = GetDetailsAsSettings(popup);

            List<Body> bodyCopies = new();
            foreach (Body body in simController.simulation.bodies)
                bodyCopies.Add(body.GetCopy());

            Simulation newSave = DataHandler.CreateSimulationSave();
            newSave.settings = settings;
            newSave.bodies = bodyCopies;

            Debug.Log("[run template saving process]");
        };

        popup.Q<Button>("Close").clicked += () => {
            popup.RemoveFromHierarchy();
        };

        // pause simulation
        simController.IsRunning = false;

        ui.Q<Button>("TogglePlay").Q<VisualElement>("Icon").style.backgroundImage 
                = Resources.Load<Texture2D>("Icons/Toolbox/Options/Playing");
    }

    // validates the save's details
    public bool ValidateDetails(VisualElement popup)
    {
        var name = popup.Q<TextField>("Name");

        if (name.value == "")
        {
            DisplayValidation(popup, "<b>'Name'</b> cannot be blank!");
            return false;
        }

        // description can be blank
        
        var gravitationalConstant = popup.Q<TextField>("GravitationalConstant");


        if (gravitationalConstant.value == "")
        {
            DisplayValidation(popup, "<b>'G'</b> must be greater than 0!");
            return false;
        }

        return true;
    }
        
    // gathers the save's details
    public Settings GetDetailsAsSettings(VisualElement popup)
    {
        var name = popup.Q<TextField>("Name");
        var description = popup.Q<TextField>("Description");
        // var gravitationalConstant = popup.Q<TextField>("GravitationalConstant");

        return new()
        {
            Name = name.text,
            Description = description.text,
            GravitationalConstant = simController.simulation.settings.GravitationalConstant
        };
    }

    // displays the given message as a validation notification
    public void DisplayValidation(VisualElement popup, string message)
    {
        popup.Q<Label>("Validation").text = message;
    }

    public void DestroySavePopup()
    {
        ui.Q<VisualElement>("SavePopup").RemoveFromHierarchy();
    }

    #endregion Save As Popup
}
