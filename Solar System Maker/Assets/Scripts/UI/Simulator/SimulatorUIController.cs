// Manages the Simulator interface
// * the Simulator UI acts as a medium to interact with the Simulator
// * it itself does NOT simulate physics (etc.)


/* REFERENCES:
* [1] - Game Dev Beginner, "How to convert the mouse position to world space in Unity", www.youtube.com [Online]. Available at: https://www.youtube.com/watch?v=5NTmxDSKj-Q
*/


// LIBRARIES
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
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

    private Body selectedBody;


    #region UI Setup

    // initialises basic Simulator functions
    public void Initialise()
    {
        simController = GameObject.Find("Simulator").GetComponent<SimulatorController>();
        ui = GameObject.Find("SimulatorUI").GetComponent<UIDocument>().rootVisualElement;

        ui.Q<Button>("SaveSimulation").clicked += OnSaveSimulationClicked;
        ui.Q<Button>("ExitSimulator").clicked += OnExitSimulatorClicked;

        ui.Q<Button>("Bodies").clicked += OnBodiesClicked;
        ui.Q<Button>("Explorer").clicked += OnExplorerClicked;

        ui.Q<Button>("AddStar").clicked += OnAddStarClicked;
        ui.Q<Button>("AddPlanet").clicked += OnAddPlanetClicked;

        InitialisePropertyFields();
        ui.Q<Button>("PropertiesClose").clicked += OnPropertiesCloseClicked;

        Debug.Log("[SIMULATOR] SUCCESS: UI Loaded");
    }

    #endregion UI Setup


    #region General UI

    // displays the Simulator interface
    public void ShowInterface()
    {
        ui.Q("Root").style.display = DisplayStyle.Flex;
    }

    // hides the Simulator interface
    public void HideInterface()
    {
        ui.Q("Root").style.display = DisplayStyle.None;
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

            // TODO: change camera mode to Main Menu?
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

        DataManager.SaveAllData();
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

    // hides the properties window when the close button is clicked
    public void OnPropertiesCloseClicked()
    {
        HideProperties();
    }

    #endregion General UI


    #region Toolbox Functions

    public void OnBodiesClicked()
    {
        VisualElement bodyList = ui.Q<VisualElement>("BodyList");
        bodyList.style.display = bodyList.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;
    }

    public void OnExplorerClicked()
    {
        // VisualElement explorer = ui.Q<VisualElement>("ExplorerList");
        // explorer.style.display = explorer.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;
    }

    #endregion


    #region Body Selection & Preview

    public Body GetSelectedBody()
    {
        return selectedBody;
    }

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

        // BODY ITEM SELECTION TOGGLE
        //   removes the current selected body to have an "empty hand"
        if (selectedBodyItem != null && Input.GetKeyDown(KeyCode.E))
        {
            SelectBodyItem(selectedBodyItem); // as if the button is being toggled off
            RemoveEntityPreview();
        }

        // BODY PREVIEW
        //   displays a "ghost" representation of the selected body
        //   at the target location (only updates position if not being fired)
        if (bodyPreview != null && bodyPreview.Entity != null && startVelocityPoint == null)
        {
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = Camera.main.nearClipPlane + distanceFromCamera;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            bodyPreview.Entity.transform.position = worldPosition;
        }

        // BODY SELECTION (in space)
        //   detecting any bodies that have been selected with LMB
        //   (only if a body preview isn't already being placed, to prevent interference)
        //   (also if it's in the world space, not UI)
        if (bodyPreview == null && Input.GetMouseButtonDown(0) && GeneralUI.IsPointerOverAnyUI())
        {
            Vector3 screenPosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null)
                {
                    // finding the Body object from the hit object (if any)
                    GameObject hitPart = hit.collider.gameObject;
                    Body body = GetBodyFromEntity(hitPart);

                    if (body != null)
                    {
                        selectedBody = body;
                        Camera.main.GetComponent<CameraController>().SetSubject(selectedBody);
                        Camera.main.GetComponent<CameraController>().SetScene(CameraController.Mode.Following);
                        ShowProperties();
                    }
                }
            }
            else
            {
                selectedBody = null;
                Camera.main.GetComponent<CameraController>().SetScene(CameraController.Mode.Simulator);
                Camera.main.GetComponent<CameraController>().SetSubject(selectedBody);
                ShowProperties();
            }
        }

        if (bodyPreview == null && selectedBody == null)
        {
            Camera.main.transform.position += Input.mouseScrollDelta.y * 3 * Camera.main.transform.forward;
        }

        // BODY TRAJECTORY (start)
        //   set the starting velocity point to the mouse's position in the world on LMB click
        //   * don't really need to check for previewEntity since no direct operations...
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = Camera.main.nearClipPlane + distanceFromCamera;
            startVelocityPoint = Camera.main.ScreenToWorldPoint(screenPosition);
        }

        // BODY TRAJECTORY (end)
        //   stop tracking the starting velocity on LMB lift (marks that 0 velocity will be applied)
        if (Input.GetMouseButtonUp(0))
        {
            startVelocityPoint = null;
        }

        // SPAWN DISTANCE
        //   updating spawn distance from camera with scrolling
        float cameraOffset = distanceFromCamera - Input.mouseScrollDelta.y * 3;
        distanceFromCamera = Math.Max(0, cameraOffset);

        // BODY PLACEMENT
        //   place the selected body down on input
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

            RemoveEntityPreview();
            SetEntityPreview(selectedBodyItem);
        }
    }

    #endregion Inputs Loop


    #region Properties

    
    // changes the selected body's properties when any property field is changed
    private void InitialisePropertyFields()
    {
        VisualElement propertiesFrame = ui.Q<VisualElement>("Properties");
        
        VisualElement bodyFields = propertiesFrame.Q("BodyFields");
        foreach (VisualElement field in bodyFields.Children()) // all elements will be fields
        {
            string name = field.name;
            string property = name[(name.IndexOf("_") + 1)..]; // the part after the "_"

            if (field is FloatField floatField)
            {
                InitialisePropertyField(floatField, property);
            }
            else if (field is TextField textField)
            {
                InitialisePropertyField(textField, property);
            }
            else if (field is DropdownField dropdownField)
            {
                InitialisePropertyField(dropdownField, property);
            }
        }

        Debug.Log("Property Fields Initialised");
    }

    // sets up the given property field so that, when enter is pressed,
    //   the corresponding property is updated with the value provided
    private void InitialisePropertyField<T>(BaseField<T> field, string property)
    {   
        // Debug.Log(property);

        field.RegisterCallback<KeyDownEvent>((e) =>{
            if (e.keyCode == KeyCode.Return && selectedBody != null)
            {
                var value = field.value;
                PropertyInfo info = selectedBody.GetType().GetProperty(property);
                info.SetValue(selectedBody, field.value);
                field.Blur(); // unfocuses the field
            }
        }, TrickleDown.TrickleDown);

        // weirdly - by default, two key presses are required to work
        //   * "TrickleDown.TrickleDown" only requires one key press
        //   * no idea why!
    }

    // shows the properties window
    public void ShowProperties()
    {
        VisualElement properties = ui.Q<VisualElement>("Properties");

        if (selectedBody == null)
        {
            properties.Q<VisualElement>("PropertyGroups").style.display = DisplayStyle.None;
            properties.Q<Label>("NoSelectedBody").style.display = DisplayStyle.Flex;
        }
        else 
        {
            properties.Q<Label>("NoSelectedBody").style.display = DisplayStyle.None;
            properties.Q<VisualElement>("PropertyGroups").style.display = DisplayStyle.Flex;

            // general body properties
            VisualElement bodyProperties = properties.Q<VisualElement>("Properties_Body");
            bodyProperties.style.display = DisplayStyle.Flex;

            bodyProperties.Q<TextField>("Body_Name").value = selectedBody.Name;
            bodyProperties.Q<FloatField>("Body_Mass").value = selectedBody.Mass;
            bodyProperties.Q<FloatField>("Body_Radius").value = selectedBody.Radius;
            bodyProperties.Q<FloatField>("Body_Speed").value = selectedBody.Velocity.magnitude;
            bodyProperties.Q<FloatField>("Body_SurfaceTemp").value = selectedBody.SurfaceTemp;
            bodyProperties.Q<FloatField>("Body_Density").value = selectedBody.Density;

            // Debug.Log(selectedBody.GetType().GetProperty("Radius").GetValue(selectedBody, null));

            // initially hiding all "particular" property groups
            VisualElement starProperties = properties.Q<VisualElement>("Properties_Star");
            VisualElement planetProperties = properties.Q<VisualElement>("Properties_Planet");

            starProperties.style.display = DisplayStyle.None;
            planetProperties.style.display = DisplayStyle.None;

            // star properties
            if (selectedBody is Star star)
            {
                starProperties.style.display = DisplayStyle.Flex;
                starProperties.Q<FloatField>("Star_Luminosity").value = star.Luminosity;

            } // planet properties
            else if (selectedBody is Planet planet)
            {
                planetProperties.style.display = DisplayStyle.Flex;
                planetProperties.Q<FloatField>("Planet_WaterContent").value = planet.WaterContent;
                planetProperties.Q<FloatField>("Planet_Habitability").value = planet.Habitability;
            }
        }

        properties.style.display = DisplayStyle.Flex;
    }

    // hides the properties window
    public void HideProperties()
    {
        ui.Q<VisualElement>("Properties").style.display = DisplayStyle.None;
    }

    #endregion Properties


    #region Explorer List Functions 

    // creates a new entry in the explorer for the given body
    public void AddBodyEntry(Body body)
    {
        // TODO: create entry
    }

    #endregion


    #region Auxiliary

    // returns the body from the list of bodies which matches the give entity
    public Body GetBodyFromEntity(GameObject entity)
    {
        foreach (Body body in simController.simulation.bodies)
        {
            if (body.Entity == entity) return body;
        }
        return null;
    }

    #endregion
}
