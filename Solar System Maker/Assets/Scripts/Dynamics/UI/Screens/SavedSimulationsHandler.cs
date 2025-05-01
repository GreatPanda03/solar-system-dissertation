// creates instances of the "Saved Simulations" interface
// * the interface displays the user's saved simulations which can be opened and run


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class SavedSimulationsHandler
{

    #region Attributes

    public VisualElement ui;
    public UIController uiController;
    public VisualTreeAsset saveEntry;

    public VisualElement selectedEntry;
    public Simulation selectedSave;

    #endregion Attributes


    #region Constructor

    // prepares the given UI as a "Saved Simulations" interface
    public SavedSimulationsHandler(
        VisualElement ui,
        UIController uiController, 
        VisualTreeAsset saveEntry)
    {
        this.ui = ui;
        this.uiController = uiController;
        this.saveEntry = saveEntry;

        ScrollView list = ui.Q<ScrollView>("List");
        list.contentContainer.style.flexDirection = FlexDirection.Row;
        list.contentContainer.style.flexWrap = Wrap.Wrap;

        ui.Q<Button>("Load").clicked += OnLoadClicked;
        ui.Q<Button>("Delete").clicked += OnDeleteClicked;
        ui.Q<Button>("Close").clicked += OnCloseClicked;

        LoadSaves();
    }

    #endregion Constructor


    #region UI Methods

    // loads the currently selected saved on click
    public void OnLoadClicked()
    {
        if (selectedSave != null)
            LoadSimulation(selectedSave);
    }

    // deletes the selected simulation on click
    private void OnDeleteClicked()
    {
        DeleteSave(selectedSave);
    }

    // returns to the "Main Menu" interface on click
    private void OnCloseClicked()
    {
        uiController.ShowMainMenu();
    }

    #endregion UI Methods


    #region Entry Management

    // load all saves of the user as entries
    public void LoadSaves()
    {
        List<Simulation> saves = DataHandler.SimulationSaves;

        foreach (Simulation save in saves)
        {
            VisualElement entry = CreateEntry(save);
            EnableEntry(entry, save);
            ui.Q<ScrollView>("List").Add(entry);
        }
    }

    // creates an entry for the given save
    public VisualElement CreateEntry(Simulation save)
    {
        VisualElement entry = saveEntry.Instantiate();
        entry.Q<Label>("Title").text = save.settings.Name;

        return entry;
    }

    // allows the save associated with the given entry to be interacted with
    public void EnableEntry(VisualElement entry, Simulation save)
    {
        Button button = entry.Q<Button>(); // the entry button itself

        button.clicked += () => {
            SelectEntry(entry);
            UpdateViewer(save);
        };
    }

    // selects the given Save Entry
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

    // updates the save viewer with the given save's details
    public void UpdateViewer(Simulation save)
    {
        // updating the save viewer panel accordingly
        VisualElement saveViewer = ui.Q<VisualElement>("Viewer");
        VisualElement saveDetails = ui.Q<VisualElement>("Details");
        Label title = saveViewer.Q<Label>("Title");
        Label description = saveViewer.Q<Label>("Description");
        Label noSelectedSave = saveViewer.Q<Label>("NoSelectedSave");

        if (save != null) 
        {
            title.text = save.settings.Name;
            description.text = save.settings.Description;

            noSelectedSave.style.display = DisplayStyle.None;
            saveDetails.style.display = DisplayStyle.Flex;

            selectedSave = save;
        }
        else
        {
            saveDetails.style.display = DisplayStyle.None;
            noSelectedSave.style.display = DisplayStyle.Flex;

            selectedSave = null;
        }
    }

    // loads the given save file
    public void LoadSimulation(Simulation saveRef)
    {
        Simulation save = saveRef.GetCopy();

        SimulationController simController = GameObject.Find("Simulator").GetComponent<SimulationController>();
        simController.LoadSimulation(save);
        simController.FullEnable();
        simController.IsRunning = false;

        uiController.ShowSimulator();

        CameraController.CameraMode = CameraController.Mode.Free;
    }

    // deletes the given save using its ID
    public void DeleteSave(Simulation save)
    { 
        DataHandler.DeleteSimulationSave(save.id);

        UpdateViewer(null);
        ClearList();
        LoadSaves();
    }

    // removes all entries from the saves list
    public void ClearList()
    {
        ui.Q<ScrollView>("List").Clear();
    }

    #endregion Entry Management
}


