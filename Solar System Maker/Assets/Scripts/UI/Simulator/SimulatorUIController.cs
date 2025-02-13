// Manages the Simulator interface
// * the Simulator UI acts as a medium to interact with the Simulator
// * it itself does NOT simulate physics (etc.)


/* REFERENCES:
* [1] - Game Dev Beginner, "How to convert the mouse position to world space in Unity", www.youtube.com [Online]. Available at: https://www.youtube.com/watch?v=5NTmxDSKj-Q
*/


// LIBRARIES
using System;
using UnityEngine;
using UnityEngine.UIElements;


// CLASS DEFINITION
public class SimulatorUIController : MonoBehaviour
{
    private SimulatorController simController;
    public VisualElement ui;

    private bool isActive = false;

    public GameObject simulatorSpace;
    private string selectedBodyItem;
    private Body bodyPreview;
    private Vector3? startVelocityPoint; // positional point, not velocity
    private float distanceFromCamera = 30f;


    #region UI Setup

    // initialises basic Simulator functions
    public void Initialise()
    {
        simController = GameObject.Find("Simulator").GetComponent<SimulatorController>();
        ui = GameObject.Find("SimulatorUI").GetComponent<UIDocument>().rootVisualElement;

        ui.Q<Button>("SaveSimulation").clicked += OnSaveSimulationClicked;
        ui.Q<Button>("ExitSimulator").clicked += OnExitSimulatorClicked;

        ui.Q<Button>("AddStar").clicked += OnAddStarClicked;
        ui.Q<Button>("AddPlanet").clicked += OnAddPlanetClicked;

        Debug.Log("[SIMULATOR] SUCCESS: UI Loaded");
    }

    #endregion UI Setup


    #region General UI

    // displays the Simulator interface
    public void ShowInterface()
    {
        ui.style.display = DisplayStyle.Flex;
    }

    // hides the Simulator interface
    public void HideInterface()
    {
        ui.style.display = DisplayStyle.None;
    }

    // enables the interface
    public void EnableInterface()
    {
        isActive = true;
    }

    // disables the interface and reverts all values to their defaults
    public void DisableInterface()
    {
        isActive = false;
        selectedBodyItem = null;
        if (bodyPreview != null) 
        {
            Destroy(bodyPreview.Entity);
            bodyPreview = null;
        }
        startVelocityPoint = new();
        distanceFromCamera = 30f;
    }

    // sets the title of the interface at the top of the screen
    public void SetTitle(string title)
    {
        ui.Q<Label>("Title").text = title;
    }

    // saves the simulation
    private void OnSaveSimulationClicked()
    {
        Simulation simulation = simController.simulation;;
        bool updated = DataManager.UpdateSimulationSave(simulation.id, simulation);
        if (!updated) DataManager.AddSimulationSave(simulation);
    }

    // clears the simulator and returns to the main menu
    private void OnExitSimulatorClicked()
    {
        simController.DisableSimulator();
        simController.ClearSimulator();

        DisableInterface();
        HideInterface();
        MainMenuUIController.ShowInterface();

        Camera.main.GetComponent<CameraController>()
            .SetScene(CameraController.Mode.MainMenu);
    }

    #endregion General UI


    #region Body Selection & Preview

    private void OnAddStarClicked() { OnBodyItemClicked("Star"); }
    private void OnAddPlanetClicked() { OnBodyItemClicked("Planet"); }

    private void OnBodyItemClicked(string bodyType)
    {
        SelectBodyItem(bodyType);
        RemoveEntityPreview();
        if (selectedBodyItem == null) return;
        SetEntityPreview(bodyType);
    }


    // sets the selected item from the body list and sets a visual indicator
    public void SelectBodyItem(string bodyName)
    {
        if (selectedBodyItem == null) // no item selected / fresh item
        {
            Button newButton = ui.Q<Button>("Add" + bodyName);
            newButton.AddToClassList("selectedButton");
            selectedBodyItem = bodyName;
        } 
        else // disable the current selection - item is being switched
        {
            Button oldButton = ui.Q<Button>("Add" + selectedBodyItem);
            oldButton.RemoveFromClassList("selectedButton");

            if (selectedBodyItem == bodyName) // the same item is selected
            {   
                selectedBodyItem = null;
            }
            else // a different item is selected
            {
                Button newButton = ui.Q<Button>("Add" + bodyName);
                newButton.AddToClassList("selectedButton");
                selectedBodyItem = bodyName;
            }
        }
    }

    public void SetEntityPreview(string bodyType)
    {       
        if      (bodyType == "Star")      { bodyPreview = new Star();   }
        else if (bodyType == "Planet")    { bodyPreview = new Planet(); }
        else Debug.LogWarning($"[SIM UI] ERROR: '{bodyType}' is an invalid body type!");

        bodyPreview.CreateEntity();
        bodyPreview.Entity.transform.name = "Preview";

        Material m = bodyPreview.Entity.GetComponent<MeshRenderer>().material;
        m.color = new Color(m.color.r, m.color.g, m.color.b, 0.25f);   
    }

    public void RemoveEntityPreview()
    {
        if (bodyPreview != null)
        {
            if (bodyPreview.Entity != null) Destroy(bodyPreview.Entity);
            bodyPreview = null;
        }
    }

    #endregion Body Selection & Preview


    #region Inputs Loop

    // input loop - manages body placement and other inputs
   void Update()
    {
        if (!isActive) return;

        // removes the current selected body to have an "empty hand"
        if (selectedBodyItem != null && Input.GetKeyDown(KeyCode.E))
        {
            SelectBodyItem(selectedBodyItem); // as if the button is being toggled off
            RemoveEntityPreview();
        }

        // displays a "ghost" representation of the selected body
        //   at the target location (only updates position if not being fired)
        if (bodyPreview != null && bodyPreview.Entity != null && startVelocityPoint == null)
        {
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = Camera.main.nearClipPlane + distanceFromCamera;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            bodyPreview.Entity.transform.position = worldPosition;
        }

        // set the starting velocity point to the mouse's position in the world on LMB click
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = Camera.main.nearClipPlane + distanceFromCamera;
            startVelocityPoint = Camera.main.ScreenToWorldPoint(screenPosition);
        }

        // stop tracking the starting velocity on LMB lift (marks that 0 velocity will be applied)
        if (Input.GetMouseButtonUp(0))
        {
            startVelocityPoint = null;
        }

        // updating spawn distance from camera with scrolling
        float cameraOffset = distanceFromCamera - Input.mouseScrollDelta.y * 3;
        distanceFromCamera = Math.Max(0, cameraOffset);

        // place the selected body down on input
        if (bodyPreview != null && Input.GetKeyDown(KeyCode.Q))
        {
            Vector3 position = bodyPreview.Entity.transform.position;
            Vector3 velocity = Vector3.zero;

            if (startVelocityPoint != null) 
            {
                Vector3 screenPosition = Input.mousePosition;
                screenPosition.z = Camera.main.nearClipPlane + distanceFromCamera;
                Vector3 endVelocityPoint = Camera.main.ScreenToWorldPoint(screenPosition);
                velocity = ((Vector3) startVelocityPoint - endVelocityPoint) / 100; // opposite to mouse dir.
            }

            if (selectedBodyItem == "Star")
            {
                Star star = new()
                {
                    Position = position,
                    Velocity = velocity
                };
                star.CreateEntity();
                simController.AddBody(star);
            }
            else if (selectedBodyItem == "Planet")
            {
                Planet planet = new()
                {
                    Position = position,
                    Velocity = velocity
                };
                planet.CreateEntity();
                simController.AddBody(planet);
            }
        }
    }

    #endregion Inputs Loop
}
