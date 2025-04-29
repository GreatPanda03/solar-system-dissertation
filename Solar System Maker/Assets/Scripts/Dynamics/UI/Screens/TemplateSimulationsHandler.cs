// creates instances of the "Templates Simulations" interface
// * the interface shows all templates the user has which they can load a copy of and run

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TemplateSimulationsHandler
{
    
    #region Attributes

    public VisualElement ui;
    public UIController uiController;
    public VisualTreeAsset templateEntry;

    public VisualElement selectedEntry;
    public Simulation selectedTemplate;

    #endregion Attributes


    #region Constructor

    // prepares the given interface as a "Template Simulations" interface
    public TemplateSimulationsHandler(
        VisualElement ui,
        UIController uiController,
        VisualTreeAsset templateEntry)
    {
        this.ui = ui;
        this.uiController = uiController;
        this.templateEntry = templateEntry;

        ScrollView list = ui.Q<ScrollView>("List");
        list.contentContainer.style.flexDirection = FlexDirection.Row;
        list.contentContainer.style.flexWrap = Wrap.Wrap;

        ui.Q<Button>("Load").clicked += OnLoadClicked;
        ui.Q<Button>("Delete").clicked += OnDeleteClicked;
        ui.Q<Button>("Close").clicked += OnCloseClicked;

        LoadEntries();
    }

    #endregion Constructor


    #region UI Methods

    // loads the currently selected saved on click
    public void OnLoadClicked()
    {
        if (selectedTemplate != null)
            LoadSimulation(selectedTemplate);
    }
    
    // deletes the selected simulation on click
    private void OnDeleteClicked()
    {
        DeleteSave(selectedTemplate);
    }

    // returns to the "Main Menu" interface on click
    private void OnCloseClicked()
    {
        uiController.ShowMainMenu();
    }

    #endregion UI Methods


    #region Entry Management

    // load all saves of the user as entries
    public void LoadEntries()
    {
        List<Simulation> templates = DataHandler.SimulationTemplates;

        foreach (Simulation template in templates)
        {
            VisualElement entry = CreateEntry(template);
            EnableEntry(entry, template);
            ui.Q<ScrollView>("List").Add(entry);
        }
    }

    // creates an entry for the given template
    public VisualElement CreateEntry(Simulation template)
    {
        VisualElement entry = templateEntry.Instantiate();
        entry.Q<Label>("Title").text = template.settings.Name;

        return entry;
    }

    // allows the item associated with the given entry to be interacted with
    public void EnableEntry(VisualElement entry, Simulation template)
    {
        Button button = entry.Q<Button>(); // the entry button itself

        button.clicked += () => {
            SelectEntry(entry);
            UpdateViewer(template);
        };
    }

    // selects the given entry
    public void SelectEntry(VisualElement entry)
    {
        if (selectedEntry == null)
        {
            selectedEntry = entry;
            selectedEntry.AddToClassList("selectedEntry");
        }
        else
        {
            if (selectedEntry == entry)
            {
                selectedEntry.RemoveFromClassList("selectedEntry");
                selectedEntry = null;
            }
            else
            {
                selectedEntry.RemoveFromClassList("selectedEntry");
                selectedEntry = entry;
                selectedEntry.AddToClassList("selectedEntry");
            }
        }
    }

    // updates the viewer with the given item's details
    public void UpdateViewer(Simulation template)
    {
        // updating the save viewer panel accordingly
        VisualElement viewer = ui.Q<VisualElement>("Viewer");
        VisualElement details = ui.Q<VisualElement>("Details");
        Label title = viewer.Q<Label>("Title");
        Label description = viewer.Q<Label>("Description");
        Label noSelection = viewer.Q<Label>("NoSelection");

        if (template != null) 
        {
            title.text = template.settings.Name;
            description.text = template.settings.Description;

            noSelection.style.display = DisplayStyle.None;
            details.style.display = DisplayStyle.Flex;

            selectedTemplate = template;
        }
        else
        {
            details.style.display = DisplayStyle.None;
            noSelection.style.display = DisplayStyle.Flex;

            selectedTemplate = null;
        }
    }

    // loads the given template
    public void LoadSimulation(Simulation templateRef)
    {
        Simulation template = templateRef.GetCopy();

        // create a new SAVE out of the template (to not overwrite the original template)
        Simulation newSimulation = DataHandler.GetNextSimulationSaveSlot();
        newSimulation.settings = template.settings;
        newSimulation.bodies = template.bodies;

        SimulationController simController = GameObject.Find("Simulator").GetComponent<SimulationController>();
        simController.LoadSimulation(newSimulation);
        simController.FullEnable();
        simController.IsRunning = false;

        uiController.ShowSimulator();

        CameraController.SetCameraMode(CameraController.CameraMode.Simulator);
    }

    // deletes the given item using its ID
    public void DeleteSave(Simulation template)
    { 
        DataHandler.DeleteSimulationSave(template.id);

        UpdateViewer(null);
        ClearList();
        LoadEntries();
    }

    // removes all entries from the saves list
    public void ClearList()
    {
        ui.Q<ScrollView>("List").Clear();
    }

    #endregion Entry Management
}