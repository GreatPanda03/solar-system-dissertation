// Manages the simulator interface

/* REFERENCES:
* [1] - Game Dev Beginner, "How to convert the mouse position to world space in Unity", www.youtube.com [Online]. Available at: https://www.youtube.com/watch?v=5NTmxDSKj-Q
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SimulatorUIScript : MonoBehaviour
{
    #region Attributes

    public VisualElement mainMenuUI;
    public VisualElement simulatorUI;

    public GameObject StarTemplate;
    public GameObject PlanetTemplate;
    private GameObject simulatorSpace;
    private bool simulatorActive;

    private string selectedBodyItem;
    private GameObject selectedBodyEntity;
    private Vector3? startVelocityPoint; // positional point, not velocity
    private float distanceFromCamera = 30f;

    private List<Body> bodies;
    public List<Body> Bodies {
        get => bodies;
        set => bodies = value;
    }

    private Settings settings;
    public Settings Settings {
        get => settings;
        set 
        {
            settings = value;
            // physically apply the settings...
        }
    }

    #endregion Attributes


    #region UI Management Methods

    // preparing starter variables
    private void Awake()
    {
        simulatorSpace = GameObject.Find("SimulatorSpace");
        mainMenuUI = GameObject.Find("MainMenuUI").GetComponent<UIDocument>().rootVisualElement;
        simulatorUI = GetComponent<UIDocument>().rootVisualElement;
        simulatorActive = false;
    }

    // initialises the interface when the enabled
    private void OnEnable()
    {
        simulatorUI.Q<Button>("ExitSimulator").clicked += OnExitSimulatorClicked;

        simulatorUI.Q<Button>("AddStar").clicked += OnAddStarClicked;
        simulatorUI.Q<Button>("AddPlanet").clicked += OnAddPlanetClicked;

        Debug.Log("[SIMULATOR UI] SUCCESS: UI Loaded");
    }

    // clears the simulator and returns to the main menu
    private void OnExitSimulatorClicked()
    {
        simulatorActive = false;
        ClearSimulator();

        mainMenuUI.style.display = DisplayStyle.Flex;
        simulatorUI.style.display = DisplayStyle.None;

        var sampleSimulationScript = GameObject.Find("SampleSimulation").GetComponent<SampleSimulationScript>();
        sampleSimulationScript.LoadSampleSimulation();

        var cameraScript = Camera.main.GetComponent<CameraScript>();
        cameraScript.SetScene(CameraScript.Scenes.Main_Menu);
    }

    // clears all bodies in the simulator
    public void ClearSimulator()
    {
        GameObject container = GameObject.Find("SimulatorSpace");

        for( int c = container.transform.childCount - 1; c >= 0; c--)
        {
            Destroy(container.transform.GetChild(c).gameObject);
        }
    }

    #endregion UI Management Methods


    #region Loading the Simulator
    
    // starts the simulator with no particular bodies and default settings
    public void LoadSimulator()
    {
        simulatorActive = true;
        bodies = new List<Body>();
        settings = new Settings();
    }

    // starts the simulator with the given bodies
    public void LoadSimulator(List<Body> bodies)
    {
        simulatorActive = true;
        this.bodies = bodies;
        settings = new Settings();
    }

    // starts the simulator with the given settings
    public void LoadSimulator(Settings settings)
    {
        simulatorActive = true;
        bodies = new List<Body>();
        this.settings = settings;
    }

    // starts the simulator with the given bodies and settings
    public void LoadSimulator(List<Body> bodies, Settings settings)
    {
        simulatorActive = true;
        this.bodies = bodies;
        this.settings = settings;
    }

    public void SetSettings(Settings newSettings)
    {
        settings = newSettings;
    }

    public Settings GetSettings()
    {
        return settings;
    }

    #endregion Loading the Simulator

    
    #region Body Adding Methods

    private void OnAddStarClicked()
    {
        SelectBodyItem("Star");
        UpdateSelectedBodyEntity();
    }

    private void OnAddPlanetClicked()
    {
        SelectBodyItem("Planet");
        UpdateSelectedBodyEntity();
    }

    // sets the selected item from the body list and sets a visual indicator
    private void SelectBodyItem(string bodyName)
    {
        if (selectedBodyItem == null) // no item selected / fresh item
        {
            Button newButton = simulatorUI.Q<Button>("Add" + bodyName);
            newButton.AddToClassList(".selectedButton");
            selectedBodyItem = bodyName;
        } 
        else // disable the current selection - item is being switched
        {
            Button oldButton = simulatorUI.Q<Button>("Add" + selectedBodyItem);
            oldButton.RemoveFromClassList(".selectedButton");

            if (selectedBodyItem == bodyName) // the same item is selected
            {   
                selectedBodyItem = null;
            }
            else // a different item is selected
            {
                Button newButton = simulatorUI.Q<Button>("Add" + bodyName);
                newButton.AddToClassList(".selectedButton");
                selectedBodyItem = bodyName;
            }
        }
    }

    public void UpdateSelectedBodyEntity()
    {
        if (selectedBodyEntity != null) // clearing any existing representation
        {
            Destroy(selectedBodyEntity);
            selectedBodyEntity = null;
        }
        
        // making a new reperesentation
        if (selectedBodyItem == null) return;

        if (selectedBodyItem == "Star") {
            selectedBodyEntity = CreateEntityForBody(new Star(), StarTemplate);
        } else if (selectedBodyItem == "Planet") {
            selectedBodyEntity = CreateEntityForBody(new Planet(), PlanetTemplate);
        }

        Material m = selectedBodyEntity.GetComponent<MeshRenderer>().material;
        m.color = new Color(m.color.r, m.color.g, m.color.b, 0.25f);   
    }

    // returns a new body entity from the given template
    public GameObject CreateEntityForBody(Body body, GameObject template)
    {
        body.Entity = Instantiate(template);
        body.Entity.transform.position = body.Position;
        body.Entity.transform.parent = simulatorSpace.transform;

        return body.Entity;
    }

    #endregion Body Adding Methods


    #region Input Loop

    // input loop - manages body placement and other inputs
    public void Update()
    {
        if (!simulatorActive) return;

        // removes the current selected body to have an "empty hand"
        if (selectedBodyItem != null && Input.GetKeyDown(KeyCode.E))
        {
            SelectBodyItem(selectedBodyItem);
            UpdateSelectedBodyEntity();
        }

        // displays a "ghost" representation of the selected body
        //   at the target location (only updates position if not being fired)
        if (selectedBodyEntity != null && startVelocityPoint == null)
        {
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = Camera.main.nearClipPlane + distanceFromCamera;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            selectedBodyEntity.transform.position = worldPosition;
        }

        // set the starting velocity point to the mouse's position in the world on click
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = Camera.main.nearClipPlane + distanceFromCamera;
            startVelocityPoint = Camera.main.ScreenToWorldPoint(screenPosition);
        }

        // stop tracking the starting velocity (marks that 0 velocity will be applied)
        if (Input.GetMouseButtonUp(0))
        {
            startVelocityPoint = null;
        }

        // updating spawn distance from camera with scrolling
        float cameraOffset = distanceFromCamera - Input.mouseScrollDelta.y * 3;
        distanceFromCamera = cameraOffset >= 30f ? cameraOffset : 30f;

        // place the selected body down on input
        if (selectedBodyEntity != null && Input.GetKeyDown(KeyCode.Q))
        {
            Vector3 position = selectedBodyEntity.transform.position;
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
                Star star = new Star();
                star.Position = position;
                star.Velocity = velocity;
                CreateEntityForBody(star, StarTemplate);
                bodies.Add(star);
            }
            else if (selectedBodyItem == "Planet")
            {
                Planet planet = new Planet();
                planet.Position = position;
                planet.Velocity = velocity;
                CreateEntityForBody(planet, PlanetTemplate);
                bodies.Add(planet);
            }
        }
    }

    #endregion Input Loop


    #region Physics Loop

    // main physics loop - applies planet forces between each other
    private void FixedUpdate()
    {
        if (!simulatorActive) return;

        if (settings.GravityAlgorithm == "Newtonian Physics")
        {
            ApplyNewtonianPhysics_v1();
        }
    }

    // algorithm v1: applies forced-based gravity between all bodies
    // * calculations are done body-by-body
    // * per body A, apply that body's force to body Bs,
    //     and the forces of body Bs to itself
    // * then, because body A is done, update its forces for this round
    public void ApplyNewtonianPhysics_v1()
    {
        for (int a = 0; a < bodies.Count; a++)
        {
            Body bodyA = bodies[a];
            
            for (int b = a + 1; b < bodies.Count; b++)
            {
                Body bodyB = bodies[b];

                Vector3 force = bodyA.GetForceBetween(bodyB, settings.GravityConstant);
                bodyA.ApplyForce(force);
                bodyB.ApplyForce(-force);
            }

            bodyA.Update();
        }
    }

    // algorithm v2: applies forced-based gravity between all bodies
    // * more "extractive" with less calcs - calculations done outside class
    // * WIP...
    public void ApplyNewtonianPhysics_v2()
    {
        for (int a = 0; a < bodies.Count; a++)
        {
            Body bodyA = bodies[a];
            float Gm = settings.GravityConstant * bodyA.Mass;

            
            for (int b = a + 1; b < bodies.Count; b++)
            {
                Body bodyB = bodies[b];

                Vector3 force = bodyA.GetForceBetween(bodyB, settings.GravityConstant);
                bodyA.ApplyForce(force);
                bodyB.ApplyForce(-force);
            }

            bodyA.Update();
        }
    }

    #endregion Physics Methods
}
