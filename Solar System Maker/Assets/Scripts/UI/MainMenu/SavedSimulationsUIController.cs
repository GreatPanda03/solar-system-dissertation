// Handles the Main Menu interface


// LIBRARIES
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


// CLASS DEFINITION
public static class SavedSimulationsUIController
{
    public static VisualElement ui;
    public static VisualElement saveList;
    public static VisualTreeAsset saveEntryTemplate;
    public static TemplateContainer selectedEntry;
    public static Simulation saveToLoad;


    #region UI Setup

    // initialises the Saved Simulations interface
    public static void Initialise()
    {
        ui = GameObject.Find("SavedSimulationsUI").GetComponent<UIDocument>().rootVisualElement;
        saveList = ui.Q<ScrollView>("SaveList");
        saveEntryTemplate = GameObject.Find("SaveEntryUI").GetComponent<UIDocument>().visualTreeAsset;

        saveList.contentContainer.style.flexDirection = FlexDirection.Row;
        saveList.contentContainer.style.flexWrap = Wrap.Wrap;

        ui.Q<Button>("Load").clicked += OnLoadSimulationClicked;
        ui.Q<Button>("Delete").clicked += OnDeleteSimulationClicked;

        ui.Q<Button>("CloseWindow").clicked += OnCloseWindowClicked;

        Debug.Log("[SAVED SIMS] SUCCESS: UI Loaded");
    }

    #endregion UI Setup


    #region General UI

    // shows the Saved Simulations interface
    public static void ShowInterface()
    {
        LoadSaveEntries();
        ui.Q("Root").style.display = DisplayStyle.Flex;
    }

    // hides the Saved Simulations interface
    public static void HideInterface()
    {
        ui.Q("Root").style.display = DisplayStyle.None;
        ClearSaveEntries();
    }

    // closes the new saves window and re-displays the main menu
    private static void OnCloseWindowClicked()
    {
        HideInterface();
        MainMenuUIController.ShowInterface();
    }

    #endregion General UI


    #region Save Management

    // load all saved simulations - one entry per save
    public static void LoadSaveEntries()
    {
        List<Simulation> saves = DataManager.simulationSaves;

        foreach (Simulation save in saves)
        {
            // create an entry of the save - copy a template entry
            // .GetVisualElement / RootElement or smth like that
            TemplateContainer entry = CreateSaveEntry(save);
            EnableSaveEntry(entry, save);
            saveList.Add(entry);
        }
    }

    // creates an entry for the given Simulation save
    public static TemplateContainer CreateSaveEntry(Simulation save)
    {
        TemplateContainer newEntry = saveEntryTemplate.Instantiate();
        newEntry.Q<Label>("Title").text = save.settings.SimulationName;
        return newEntry;
    }

    // provides the functionality for the given Save Entry
    // * displays the save info panel
    public static void EnableSaveEntry(TemplateContainer entry, Simulation save)
    {
        Button button = entry.Q<Button>(); // the entry button itself
        button.clicked += () => {
            SelectSaveEntry(entry);
            UpdateSaveViewer(save);
        };
    }

    // selects the given Save Entry
    public static void SelectSaveEntry(TemplateContainer entry)
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
    public static void UpdateSaveViewer(Simulation save)
    {
        // updating the save viewer panel accordingly
        VisualElement saveViewer = ui.Q<VisualElement>("SaveViewer");
        VisualElement saveDetails = ui.Q<VisualElement>("SaveDetails");
        Label title = saveViewer.Q<Label>("Title");
        Label description = saveViewer.Q<Label>("Description");
        Label noSelectedSave = saveViewer.Q<Label>("NoSelectedSave");

        // displays the selected entry's details
        if (selectedEntry != null) 
        {
            string simName = save.settings.SimulationName;
            title.text = simName;
            description.text = $"This is the save file for the simulation called '{simName}'";

            noSelectedSave.style.display = DisplayStyle.None;
            saveDetails.style.display = DisplayStyle.Flex;

            saveToLoad = save;
        }
        else // indicate that there is no selected save file
        {
            saveDetails.style.display = DisplayStyle.None;
            noSelectedSave.style.display = DisplayStyle.Flex;

            saveToLoad = null;
        }
    }

    // removes all functionality from the given Save 
    /*
    public static void DisableSaveEntry(TemplateContainer entry)
    {
        Button button = entry.Q<Button>();
        button.clickable = null;
    }
    */

    // loads the simulation to load when clicked (if any)
    public static void OnLoadSimulationClicked()
    {
        if (saveToLoad != null) LoadSimulation(saveToLoad);
    }

    // loads the given save file
    public static void LoadSimulation(Simulation saveRef)
    {
        // constructing a new simulation
        Simulation save = saveRef.GetCopy();

        // hiding the entire Main Menu interface
        HideInterface();

        // displaying the entire Simulator interface
        SimulatorUIController simUIController = GameObject.Find("Simulator").GetComponent<SimulatorUIController>();
        simUIController.ShowInterface();
        simUIController.EnableInterface();
        simUIController.SetTitle(save.settings.SimulationName);

        SimulatorController simController = GameObject.Find("Simulator").GetComponent<SimulatorController>();
        simController.LoadSimulation(save);
        simController.EnableSimulator();

        // adjusting the camera        
        Camera.main.GetComponent<CameraController>()
            .SetScene(CameraController.Mode.Simulator);
    }

    // deletes the selected save entry when clicked
    public static void OnDeleteSimulationClicked()
    {
        DeleteSelectedEntry();
    }

    // deletes the selected save entry
    public static void DeleteSelectedEntry()
    {
        // TO DO: remove entry directly instead of list refresh!
        if (selectedEntry != null)
        {
            // selectedEntry.Destroy() // how to destroy?
            selectedEntry = null;
        }

        if (saveToLoad != null)
            DataManager.DeleteSimulationSave(saveToLoad.id);

        UpdateSaveViewer(null);
        ClearSaveEntries();
        LoadSaveEntries();   
    }

    // removes all entries from the Save List
    public static void ClearSaveEntries()
    {
        saveList.Clear();
    }

    #endregion Save Management
}


