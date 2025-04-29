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
    
    private string selectedBodyItem;
    private VisualElement focusedField;
    private Vector3 lastVelocity = Vector3.zero; // temporary - only for acceleration field

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
        VisualTreeAsset bodyTemplatePopup
    ) {
        this.ui = ui;
        this.uiController = uiController;
        this.simController = simController;
        this.explorerEntry = explorerEntry;
        this.optionsPopup = optionsPopup;
        this.savePopup = savePopup;
        this.bodyTemplatePopup = bodyTemplatePopup;

        ui.Q<VisualElement>("Toolbar").Q<Label>("Title").text 
            = simController.simulation.settings.Name;

        InitialiseToolbarButtons();
        
        InitialiseToolboxButtons();

        InitialiseExplorer();

        InitialiseBodyTools();

        InitialiseProperties();

        InitialiseInputs();

        SubscribeEvents();
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
        simController.OnOrbitChanged += OnOrbitChanged;
    }

    public void UnsubscribeEvents()
    {
        simController.OnBodyAdded -= OnBodyAdded;
        simController.OnBodyRemoved -= OnBodyRemoved;
        simController.OnBodySelected -= OnBodySelected;
        simController.OnBodyDeselected -= OnBodyDeselected;
        simController.OnScaleChanged -= OnScaleChanged;
        simController.OnPhysicsUpdate -= OnPhysicsUpdate;
        simController.OnOrbitChanged += OnOrbitChanged;
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

    public void OnOrbitChanged(float eccentricity, Body attractor)
    {
        ui.Q<Label>("Eccentricity").text = $"(Eccentricity = {eccentricity:0.000})";

        if (simController.BodyPreview != null && attractor != null)
        {
            float distance = (simController.BodyPreview.Position - attractor.Position).magnitude;
            float scale = (float) UnitReference.DISTANCE_UNITS[simController.renderer.Distance]; // zoomFactor not used here
            float scaledDistance = distance / scale;
            string unit = simController.renderer.Distance;

            ui.Q<Label>("OrbitRadius").text = $"(Orbit Radius = {scaledDistance} {unit}(s))";
        }
        else
        {
            ui.Q<Label>("OrbitRadius").text = $"(Orbit Radius = No target)";
        }
    }

    #endregion Simulator Events


    #region General UI

    public void MarkAsSelected(VisualElement element)
    {
        element.AddToClassList("selectedButton");
    }

    public void MarkAsDeselected(VisualElement element)
    {
        element.RemoveFromClassList("selectedButton");
    }

    #endregion General UI


    #region Toolbar Methods

    // prepares all toolbar buttons
    public void InitialiseToolbarButtons()
    {
        ui.Q<Button>("Options").clicked += OnOptionsClicked;
        ui.Q<Button>("Save").clicked += OnSaveClicked;
        ui.Q<Button>("SaveAs").clicked += OnSaveAsClicked;
        ui.Q<Button>("Help").clicked += OnHelpClicked;
        ui.Q<Button>("ExitSimulator").clicked += OnExitSimulatorClicked;
    }

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
                OnPlayClicked(); // pause

            uiController.ShowSaveSimulationPopup(ui, simController.simulation);
        }
    }

    // prompts to save the simulation as a new copy on click
    private void OnSaveAsClicked()
    {
        VisualElement savePopup = ui.Q<VisualElement>("SavePopup");

        if (savePopup == null)
        {
            if (simController.IsRunning)
                OnPlayClicked(); // pause

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

    // toggles the visibility of the help popup on click
    public void OnHelpClicked()
    {
        VisualElement existingPopup = ui.Q<VisualElement>(TutorialPopupHandler.identifier);

        if (existingPopup == null)
        {
            uiController.ShowTutorialPopup(ui);
        }
        else
        {
            existingPopup.RemoveFromHierarchy();
        }
    }

    #endregion Toolbar Methods


    #region Toolbox Methods

    // prepares the toolbox
    public void InitialiseToolboxButtons()
    {
        VisualElement toolbox = ui.Q<VisualElement>("Toolbox");
        VisualElement subToolbox = ui.Q<VisualElement>("SubToolbox");

        // bodies
        toolbox.Q<Button>("Bodies").clicked += OnBodiesClicked;
        subToolbox.Q<VisualElement>("BodyList").style.display = DisplayStyle.None;
        InitialiseBodyItemButtons();

        // plane tools
        toolbox.Q<Button>("Plane").clicked += OnPlaneClicked;
        subToolbox.Q<VisualElement>("PlaneList").style.display = DisplayStyle.None;
        InitialisePlaneButtons();

        // orbit mode
        toolbox.Q<Button>("Orbit").clicked += OnOrbitClicked;
        ui.Q<VisualElement>("OrbitDetails").style.display = DisplayStyle.None;

        // distances
        toolbox.Q<Button>("Distance").clicked += OnDistanceClicked;
        subToolbox.Q<VisualElement>("DistanceList").style.display = DisplayStyle.None;
        InitialiseDistanceButtons();
        
        SelectDistanceItem("Kilometre");
        UpdateScaleLabel();

        // body labels
        toolbox.Q<Button>("Labels").clicked += OnLabelsClicked;

        if (simController.ShowTrails)
        {
            MarkAsSelected(toolbox.Q<Button>("Labels"));
        }
        else
        {
            MarkAsDeselected(toolbox.Q<Button>("Labels"));
        }

        // body trails
        toolbox.Q<Button>("Trails").clicked += OnTrailsClicked;

        if (simController.ShowTrails)
        {
            MarkAsSelected(toolbox.Q<Button>("Trails"));
        }
        else
        {
            MarkAsDeselected(toolbox.Q<Button>("Trails"));
        }

        // time options
        toolbox.Q<Button>("Time").clicked += OnTimeClicked;
        subToolbox.Q<VisualElement>("TimeList").style.display = DisplayStyle.None;
        InitialiseTimeButtons();

        // skybox background
        toolbox.Q<Button>("Skybox").clicked += OnSkyboxClicked;

        if (Camera.main.GetComponent<Skybox>().material.name != "Black")
        {
            MarkAsSelected(toolbox.Q<Button>("Skybox"));
        }
        else
        {
            MarkAsDeselected(toolbox.Q<Button>("Skybox"));
        }
    }

    // toggles the visibility of the explorer on click
    public void OnExplorerClicked()
    {
        bool visible = ui.Q<VisualElement>("Explorer").style.display == DisplayStyle.Flex;
        SetExplorerVisible(!visible);
    }

    // toggles the visibility of the bodies list on click
    public void OnBodiesClicked()
    {
        VisualElement bodyList = ui.Q<VisualElement>("BodyList");
        bool visible = bodyList.style.display == DisplayStyle.Flex;

        if (!visible)
        {
            bodyList.style.display = DisplayStyle.Flex;
            MarkAsSelected(ui.Q<VisualElement>("Toolbox").Q<Button>("Bodies"));
        }
        else
        {
            bodyList.style.display = DisplayStyle.None;
            MarkAsDeselected(ui.Q<VisualElement>("Toolbox").Q<Button>("Bodies"));
        }
    }

    // toggles the visibility of the plane and plane tools list on click
    public void OnPlaneClicked()
    {
        VisualElement planeList = ui.Q<VisualElement>("PlaneList");
        Button planeButton = ui.Q<VisualElement>("Toolbox").Q<Button>("Plane");

        if (!simController.planeEnabled)
        {
            simController.EnablePlane();
            planeList.style.display = DisplayStyle.Flex;
            MarkAsSelected(planeButton);
        }
        else
        {
            planeList.style.display = DisplayStyle.None;
            simController.DisablePlane();
            MarkAsDeselected(planeButton);
        }
    }

    // toggles orbit mode on click
    public void OnOrbitClicked()
    {
        Button orbitButton = ui.Q<VisualElement>("Toolbox").Q<Button>("Orbit");

        if (!simController.orbitEnabled)
        {
            simController.EnableOrbit();
            MarkAsSelected(orbitButton);
            ui.Q<VisualElement>("OrbitDetails").style.display = DisplayStyle.Flex;
        }
        else
        {
            simController.DisableOrbit();
            MarkAsDeselected(orbitButton);
            ui.Q<VisualElement>("OrbitDetails").style.display = DisplayStyle.None;
        }
    }

    // toggles the visibility of the distances units list on click
    public void OnDistanceClicked()
    {
        VisualElement distanceList = ui.Q<VisualElement>("DistanceList");
        bool visible = distanceList.style.display == DisplayStyle.Flex;

        if (!visible)
        {
            distanceList.style.display = DisplayStyle.Flex;
            MarkAsSelected(ui.Q<VisualElement>("Toolbox").Q<Button>("Distance"));
        }
        else
        {
            distanceList.style.display = DisplayStyle.None;
            MarkAsDeselected(ui.Q<VisualElement>("Toolbox").Q<Button>("Distance"));
        }
    }
    
    // toggles the visibility of all labels on click (where individual labels aren't disabled)
    private void OnLabelsClicked()
    {
        simController.ShowLabels = !simController.ShowLabels;

        Button labelsButton = ui.Q<VisualElement>("Toolbox").Q<Button>("Labels");

        if (simController.ShowLabels)
        {
            MarkAsSelected(labelsButton);
        }
        else
        {
            MarkAsDeselected(labelsButton);
        }
    }

    // toggles the visibility of all trails on click (where individual trail aren't disabled)
    private void OnTrailsClicked()
    {
        simController.ShowTrails = !simController.ShowTrails;

        Button trailsButton = ui.Q<VisualElement>("Toolbox").Q<Button>("Trails");

        if (simController.ShowTrails)
        {
            MarkAsSelected(trailsButton);
        }
        else
        {
            MarkAsDeselected(trailsButton);
        }
    }

    // toggles the visibility of the time options on click
    public void OnTimeClicked()
    {
        VisualElement timeList = ui.Q<VisualElement>("TimeList");
        bool visible = timeList.style.display == DisplayStyle.Flex;
        
        if (!visible)
        {
            timeList.style.display = DisplayStyle.Flex;
            MarkAsSelected(ui.Q<VisualElement>("Toolbox").Q<Button>("Time"));
        }
        else
        {
            timeList.style.display = DisplayStyle.None;
            MarkAsDeselected(ui.Q<VisualElement>("Toolbox").Q<Button>("Time"));
        }
    }

    // toggles the visibility of the skybox on click (space / black)
    public void OnSkyboxClicked()
    {
        Button skyboxButton = ui.Q<VisualElement>("Toolbox").Q<Button>("Skybox");
        Skybox skybox = Camera.main.GetComponent<Skybox>();

        if (skybox.material.name == "Black")
        {
            MarkAsSelected(skyboxButton);
            skybox.material = Resources.Load<Material>("Skyboxes/Space");
        }
        else
        {
            skybox.material = Resources.Load<Material>("Skyboxes/Black");
            MarkAsDeselected(skyboxButton);
        }
    }

    #endregion Toolbox Functions


    #region Body Selection

    // selects the respective body as a preview on click
    public void InitialiseBodyItemButtons()
    {
        VisualElement list = ui.Q<VisualElement>("BodyList");

        foreach (var element in list.Children())
        {
            if (element is Button item && element.name != "Templates")
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

        // show body templates on click
        list.Q<Button>("Templates").clicked += () => {
            BodyTemplatesPopupHandler popup = uiController.ShowBodyTemplatesPopup(ui);
            
            popup.OnTemplateSelected += template =>
            {
                DeselectBodyItem();
                simController.SetEntityPreviewToTemplate(template);
            };
        };
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
        if (selectedBodyItem != null)
        {
            Button button = ui.Q<Button>(selectedBodyItem);
            MarkAsDeselected(button);

            selectedBodyItem = null;
        }

        simController.RemoveEntityPreview();
    }

    #endregion Body Selection


    #region Plane Controls

    // prepares functionality for all plane buttons
    public void InitialisePlaneButtons()
    {
        VisualElement planeList = ui.Q<VisualElement>("PlaneList");

        planeList.Q<Button>("Stick").clicked += () => {
            if (!simController.stickToPlane)
            {
                simController.EnableStickToPlane();
                MarkAsSelected(planeList.Q<Button>("Stick"));
            }
            else
            {
                simController.DisableStickToPlane();
                MarkAsDeselected(planeList.Q<Button>("Stick"));
            }
        };

        planeList.Q<Button>("Lower").clicked += () => {
            simController.TranslatePlane(new Vector3(0, -1, 0));
        };

        planeList.Q<Button>("Raise").clicked += () => {
            simController.TranslatePlane(new Vector3(0, 1, 0));
        };
        
        planeList.Q<Button>("TurnLeft").clicked += () => {
            simController.TurnPlane(new Vector3(-1, 0, 0));
        };

        planeList.Q<Button>("TurnRight").clicked += () => {
            simController.TurnPlane(new Vector3(1, 0, 0));
        };

        planeList.Q<Button>("TurnBackwards").clicked += () => {
            simController.TurnPlane(new Vector3(0, 0, -1));
        };

        planeList.Q<Button>("TurnForwards").clicked += () => {
            simController.TurnPlane(new Vector3(0, 0, 1));
        };

        planeList.Q<Button>("Reset").clicked += () => {
            simController.SetPlanePositionAndRotation(
                Vector3.zero, Quaternion.Euler(90, 0, 0));
        };
    }

    #endregion Plane Controls


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
        MarkAsDeselected(oldButton);

        Button newButton = ui.Q<Button>(newDistance);
        MarkAsSelected(newButton);

        simController.SetDistanceScale(newItem.Replace("_", " "));
        simController.SetZoomFactor(0);

        UpdateScaleLabel();
    }

    // NB: a distance is ALWAYS selected - there can never be none

    #endregion Distance Selection


    #region Time Selection

    // prepares all time-related buttons
    public void InitialiseTimeButtons()
    {
        ui.Q<Button>("Play").clicked += OnPlayClicked;
    
        // could be either playing or paused (depending on where this sim was loaded from)
        ui.Q<Button>("Play").Q<VisualElement>("Icon").style.backgroundImage 
            = Resources.Load<Texture2D>($"Icons/Toolbox/Time/{(simController.IsRunning ? "Pause" : "Play")}");

        // will always start with time going forward & 1s/s
        ui.Q<Button>("Flow").clicked += OnFlowClicked;
        simController.physics.TimePerSecond = "1 Second";
        simController.IsRewinding = false;

        ui.Q<Button>("SlowTime").clicked += OnSlowTimeClicked;

        ui.Q<Button>("SpeedTime").clicked += OnSpeedTimeClicked;
    }

    // plays or pauses the simulation
    public void OnPlayClicked()
    {
        if (simController.IsRunning)
        {
            simController.IsRunning = false;

            ui.Q<VisualElement>("TimeList").Q<Button>("Play")
                .Q<VisualElement>("Icon").style.backgroundImage 
                = Resources.Load<Texture2D>("Icons/Toolbox/Time/Play");
        }
        else
        {
            simController.IsRunning = true;

            ui.Q<VisualElement>("TimeList").Q<Button>("Play")
                .Q<VisualElement>("Icon").style.backgroundImage
                = Resources.Load<Texture2D>("Icons/Toolbox/Time/Pause");
        }
    }

    // toggles whether time is moving forward or backwards 
    public void OnFlowClicked()
    {
        simController.IsRewinding = !simController.IsRewinding;

        if (simController.IsRewinding)
        {
            ui.Q<Button>("Flow").Q<VisualElement>("Icon").style.backgroundImage
                = Resources.Load<Texture2D>("Icons/Toolbox/Time/Reverse");
        }
        else
        {
            ui.Q<Button>("Flow").Q<VisualElement>("Icon").style.backgroundImage
                = Resources.Load<Texture2D>("Icons/Toolbox/Time/Forward");
        }
    }

    // slows down time by "one metric" on click (e.g., hour -> minute -> second)
    public void OnSlowTimeClicked()
    {
        List<string> keys = new(UnitReference.TIME_UNITS.Keys);
        int keyIndex = keys.IndexOf(simController.physics.TimePerSecond);
        keyIndex -= 1;

        if (keyIndex >= 0)
        {
            simController.physics.TimePerSecond = keys[keyIndex];
        }

        Debug.Log(simController.physics.TimePerSecond);
        Debug.Log(UnitReference.TIME_UNITS[simController.physics.TimePerSecond]);

        UpdateTimeLabel();
    }

    // speeds up time by "one metric" on click (e.g., second -> minute -> hour)
    public void OnSpeedTimeClicked()
    {
        List<string> keys = new(UnitReference.TIME_UNITS.Keys);
        int keyIndex = keys.IndexOf(simController.physics.TimePerSecond);
        keyIndex += 1;

        if (keyIndex < keys.Count)
        {
            simController.physics.TimePerSecond = keys[keyIndex];
        }

        Debug.Log(simController.physics.TimePerSecond);
        Debug.Log(UnitReference.TIME_UNITS[simController.physics.TimePerSecond]);

        UpdateTimeLabel();
    }

    #endregion Time Selection


    #region Unit Methods

    // updates the zoom label with the current scale and distance
    private void UpdateScaleLabel()
    {
        string zoomText = string.Format("<b>Scale:</b> 1 {0} x {1:0.000}", 
            simController.renderer.Distance,
            1 / Math.Pow(10, simController.renderer.ZoomFactor));

        ui.Q<Label>("Scale").text = zoomText;
    }

    // updates the time label with the current time per second
    private void UpdateTimeLabel()
    {
        string timeText = string.Format("<b>Time:</b> {0} / s", // "unit" already included in key
            simController.physics.TimePerSecond.ToLower());  // stylistic

        ui.Q<Label>("Time").text = timeText;
    }

    #endregion Unit Methods

    
    #region Explorer

    // prepares functionality for the explorer window
    public void InitialiseExplorer()
    {
        ui.Q<Button>("Explorer").clicked += OnExplorerClicked;
        InitialiseExplorerSearchBar();
        ui.Q("Explorer").Q<Button>("Close").clicked += OnExplorerCloseClicked;
        RefreshExplorer();

        SetExplorerVisible(false);
    }

    // sets whether the explorer is visible or not
    public void SetExplorerVisible(bool visible)
    {
        if (visible)
        {
            ui.Q<VisualElement>("Explorer").style.display = DisplayStyle.Flex;
            MarkAsSelected(ui.Q<VisualElement>("Toolbox").Q<Button>("Explorer"));
        }
        else
        {
            ui.Q<VisualElement>("Explorer").style.display = DisplayStyle.None;
            MarkAsDeselected(ui.Q<VisualElement>("Toolbox").Q<Button>("Explorer"));
        }
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

        List<Body> bodies = simController.simulation.bodies;

        if (bodies.Count > 0)
        {
            ui.Q<Label>("NoBodies").style.display = DisplayStyle.None;

            foreach (Body body in bodies)
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
            if (simController.SelectedBody.ShowTrail)
            {
                MarkAsSelected(bodyTools.Q<Button>("Trail"));
            }
            else
            {
                MarkAsDeselected(bodyTools.Q<Button>("Trail"));
            }

            if (simController.SelectedBody.ShowLabel)
            {
                MarkAsSelected(bodyTools.Q<Button>("Label"));
            }
            else
            {
                MarkAsDeselected(bodyTools.Q<Button>("Label"));
            }

            if (simController.SelectedBody.Anchored)
            {
                MarkAsSelected(bodyTools.Q<Button>("Anchor"));
            }
            else
            {
                MarkAsDeselected(bodyTools.Q<Button>("Anchor"));
            }
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

            if (visible)
            {
                MarkAsSelected(bodyTools.Q<Button>("Trail"));
            }
            else
            {
                MarkAsDeselected(bodyTools.Q<Button>("Trail"));
            }

            // keep the trail hidden if all labels are hidden
            if (!simController.ShowTrails)
            {
                simController.SelectedBody.SetTrailVisibility(false);
            }
        };

        // toggles the body's label on click
        bodyTools.Q<Button>("Label").clicked += () => {
            if (simController.SelectedBody == null) return;
            
            bool visible = !simController.SelectedBody.ShowLabel;
            simController.SelectedBody.ShowLabel = visible;

            if (visible)
                        {
                MarkAsSelected(bodyTools.Q<Button>("Label"));
            }
            else
            {
                MarkAsDeselected(bodyTools.Q<Button>("Label"));
            }

            // keep the label hidden if all labels are hidden
            if (!simController.ShowLabels)
            {
                simController.SelectedBody.SetLabelVisibility(false);
            }
        };

        // toggles the body's anchor on click (affected by gravity or not)
        bodyTools.Q<Button>("Anchor").clicked += () => {
            if (simController.SelectedBody == null) return;

            bool anchored = !simController.SelectedBody.Anchored;
            simController.SelectedBody.Anchored = anchored;

            if (anchored)
            {
                MarkAsSelected(bodyTools.Q<Button>("Anchor"));
            }
            else
            {
                MarkAsDeselected(bodyTools.Q<Button>("Anchor"));
            }
        };

        // opens a prompt to save the selected body as a template
        bodyTools.Q<Button>("Template").clicked += () => {
            if (simController.SelectedBody == null) return;

            if (simController.IsRunning)
                OnPlayClicked(); // pause

            uiController.ShowSaveBodyPopup(ui, simController.SelectedBody);
        };

        // deletes the body on click
        bodyTools.Q<Button>("Delete").clicked += () => {
            if (simController.SelectedBody == null) return;

            simController.RemoveBody(simController.SelectedBody);
        };

        SetBodyToolsVisible(false);
    }

    #endregion Body Tools


    #region Properties

    // prepares functionality for the properties window
    public void InitialiseProperties()
    {
        InitialisePropertyFields();
        ui.Q<Button>("Close").clicked += OnPropertiesCloseClicked;
        UpdateProperties(null); // hide fields too

        SetPropertiesVisible(false);
    }
    
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
            
            lastVelocity = Vector3.zero;
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
            float distanceScale = simController.renderer.Scale;
            float timeScale = simController.physics.TimeCoefficient / 0.02f;
            float DTScale = distanceScale * timeScale;

            properties.Q<FloatField>("Speed_custom").value = body.Velocity.magnitude * DTScale;
            properties.Q<FloatField>("Acceleration_custom").value = (lastVelocity - body.Velocity).magnitude * DTScale;
            properties.Q<FloatField>("Rotations_custom").value = body.AngularVelocity / (2 * MathF.PI) * timeScale;

            lastVelocity = body.Velocity;

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
