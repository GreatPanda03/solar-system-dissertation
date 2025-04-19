// The master controller for the entire application's interface
// * primarily used to determine where the app starts 
//     - it doesn't actually control a particular UI


// LIBRARIES
using UnityEngine;


// CLASS DEFINITION
public class MasterController : MonoBehaviour
{
    #region App Start

    void Start()
    {
        Debug.Log("Starting App...");

        Debug.Log("Loading Data...");

        DataManager.LoadAllData();

        Debug.Log("Loading UI...");

        // Main Menu UI components (show menu panel, hide all other windows)
        MainMenuUIController.Initialise();
        NewSimulationUIController.Initialise();
        SavedSimulationsUIController.Initialise();
        TemplateSimulationsUIController.Initialise();
        // TutorialUIController.Initialise();               // WIP

        MainMenuUIController.ShowInterface();
        NewSimulationUIController.HideInterface();
        SavedSimulationsUIController.HideInterface();
        TemplateSimulationsUIController.HideInterface();
        // TutorialUIController.HideInterface();            // WIP

        // Simulator UI components
        SimulatorUIController simUIController = GameObject.Find("Simulator").GetComponent<SimulatorUIController>();
        simUIController.Initialise();
        simUIController.HideInterface();

        Debug.Log("[MASTER] SUCCESS: Master Loaded - ready to roll");
    }

    #endregion App Start


    #region App End

    // when the application is closed, save all data
    void OnApplicationQuit()
    {
        DataManager.SaveAllData();
    }

    #endregion App End
}
