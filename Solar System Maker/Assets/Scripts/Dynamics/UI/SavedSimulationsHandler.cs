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
    public VisualTreeAsset saveEntryTemplate;

    public VisualElement selectedEntry;
    public Simulation selectedSave;

    #endregion Attributes


    #region Constructor

    // prepares the given UI as a "Saved Simulations" interface
    public SavedSimulationsHandler(VisualElement ui, UIController uiController, 
        VisualTreeAsset saveEntryTemplate)
    {
        this.ui = ui;
        this.uiController = uiController;
        this.saveEntryTemplate = saveEntryTemplate;

        ScrollView saveList = ui.Q<ScrollView>("List");
        saveList.contentContainer.style.flexDirection = FlexDirection.Row;
        saveList.contentContainer.style.flexWrap = Wrap.Wrap;

        ui.Q<Button>("Load").clicked += OnLoadClicked;
        ui.Q<Button>("Delete").clicked += OnDeleteClicked;
        ui.Q<Button>("Close").clicked += OnCloseClicked;
    }

    #endregion Constructor


    #region UI Methods

    // loads all the user's saves on click
    private void OnLoadSavesClicked()
    {
        LoadSaves();
    }

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
        RemoveSaveEntry(selectedEntry);
    }

    // returns to the "Main Menu" interface on click
    private void OnCloseClicked()
    {
        uiController.ShowMainMenu();
    }

    #endregion UI Methods


    #region Save Management

    // load all saves of the user as entries
    public void LoadSaves()
    {
        List<Simulation> saves = DataHandler.SimulationSaves;

        foreach (Simulation save in saves)
        {
            VisualElement entry = CreateSaveEntry(save);
            EnableSaveEntry(entry, save);
            ui.Q<ScrollView>("List").Add(entry);
        }
    }

    // creates an entry for the given save
    public VisualElement CreateSaveEntry(Simulation save)
    {
        VisualElement newEntry = saveEntryTemplate.Instantiate();
        newEntry.Q<Label>("Title").text = save.settings.SimulationName;

        return newEntry;
    }

    // allows the save associated with the given entry to be interacted with
    public void EnableSaveEntry(VisualElement entry, Simulation save)
    {
        Button button = entry.Q<Button>(); // the entry button itself

        button.clicked += () => {
            SelectSaveEntry(entry);
            UpdateViewer(save);
        };
    }

    // selects the given Save Entry
    public void SelectSaveEntry(VisualElement entry)
    {
        if (selectedEntry == null)
        {
            selectedEntry = entry;
            selectedEntry.Q<Button>().AddToClassList("selectedEntry");
        }
        else
        {
            if (selectedEntry == entry)
            {
                selectedEntry.Q<Button>().RemoveFromClassList("selectedEntry");
                selectedEntry = null;
            }
            else
            {
                selectedEntry.Q<Button>().RemoveFromClassList("selectedEntry");
                selectedEntry = entry;
                selectedEntry.Q<Button>().AddToClassList("selectedEntry");
            }
        }
    }

    // updates the save viewer with the given save's details
    public void UpdateViewer(Simulation save)
    {
        // updating the save viewer panel accordingly
        VisualElement saveViewer = ui.Q<VisualElement>("SaveViewer");
        VisualElement saveDetails = ui.Q<VisualElement>("SaveDetails");
        Label title = saveViewer.Q<Label>("Title");
        Label description = saveViewer.Q<Label>("Description");
        Label noSelectedSave = saveViewer.Q<Label>("NoSelectedSave");

        if (selectedEntry != null) 
        {
            string simName = save.settings.SimulationName;
            title.text = simName;
            description.text = $"This is the save file for the simulation called '{simName}'";

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

        uiController.ShowSimulator();

        SimulationController simController = GameObject.Find("Simulator").GetComponent<SimulationController>();
        simController.LoadSimulation(save);
        simController.IsActive = true;

        CameraController.SetScene(CameraController.Mode.Simulator);
    }

    // deletes the given save entry
    public void RemoveSaveEntry(VisualElement entry)
    {
        ui.Q<ScrollView>("List").Remove(entry);
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

    #endregion Save Management
}


