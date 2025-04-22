// Manages the Simulator interface
// * the Simulator UI acts as a medium to interact with the Simulator
// * it itself does NOT simulate physics (etc.)


/* REFERENCES:
* [1] - Game Dev Beginner, "How to convert the mouse position to world space in Unity", www.youtube.com [Online]. Available at: https://www.youtube.com/watch?v=5NTmxDSKj-Q
*/


// LIBRARIES
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;


// CLASS DEFINITION
public class SimulatorUIController : MonoBehaviour
{

    public enum Mode { FreePlacement, PlanePlacement, OrbitPlacement };
    private Mode mode;

    private SimulatorController simController;
    public VisualElement ui;

    // general mode
    private bool isActive = false;
    public GameObject simulatorSpace;
    private string selectedBodyItem;
    private Body bodyPreview;
    private Body selectedBody;

    // free placement mode
    private Vector3? startVelocityPoint; // positional point, not velocity
    private float distanceFromCamera = 30f;

    // plane placement mode
    private Plane placementPlane = new Plane(Vector3.up, Vector3.zero);


    #region UI Setup

    // initialises basic Simulator functions
    public void Initialise()
    {
        SetMode(Mode.FreePlacement);

        simController = GameObject.Find("Simulator").GetComponent<SimulatorController>();
        ui = GameObject.Find("SimulatorUI").GetComponent<UIDocument>().rootVisualElement;

        ui.Q<Button>("SaveSimulation").clicked += OnSaveSimulationClicked;
        ui.Q<Button>("ExitSimulator").clicked += OnExitSimulatorClicked;

        ui.Q<Button>("Explorer").clicked += OnExplorerClicked;
        ui.Q<Button>("ExplorerClose").clicked += OnExplorerCloseClicked;
        ui.Q<VisualElement>("Explorer").style.display = DisplayStyle.None;

        ui.Q<Button>("Modes").clicked += OnModesClicked;
        ui.Q<Button>("FreePlacement").clicked += OnFreePlacementClicked;
        ui.Q<Button>("PlanePlacement").clicked += OnPlanePlacementClicked;
        ui.Q<Button>("OrbitPlacement").clicked += OnOrbitPlacementClicked;
        ui.Q<VisualElement>("ModeList").style.display = DisplayStyle.None;
        SelectModeItem("FreePlacement");

        ui.Q<Button>("Distance").clicked += OnDistanceClicked;
        ui.Q<Button>("Centimetre").clicked += OnCentimetreClicked;
        ui.Q<Button>("Metre").clicked += OnMetreClicked;
        ui.Q<Button>("Kilometre").clicked += OnKilometreClicked;
        ui.Q<Button>("Mile").clicked += OnMileClicked;
        ui.Q<Button>("LunarDistance").clicked += OnLunarDistanceClicked;
        ui.Q<Button>("AU").clicked += OnAUClicked;
        ui.Q<VisualElement>("DistanceList").style.display = DisplayStyle.None;
        SelectDistanceItem("Kilometre");

        UpdateZoomScaleLabel();

        ui.Q<Button>("Bodies").clicked += OnBodiesClicked;
        ui.Q<Button>("AddStar").clicked += OnAddStarClicked;
        ui.Q<Button>("AddPlanet").clicked += OnAddPlanetClicked;
        ui.Q<Button>("AddMoon").clicked += OnAddMoonClicked;
        ui.Q<VisualElement>("BodyList").style.display = DisplayStyle.None;

        ui.Q<Button>("TogglePlay").clicked += OnTogglePlayClicked;
        simController.isRunning = true;

        InitialisePropertyFields();
        ui.Q<Button>("PropertiesClose").clicked += OnPropertiesCloseClicked;
        HideProperties();

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
        OnFreePlacementClicked(); // select Free mode by default
    }

    // disables the interface and reverts all values to their defaults
    public void DisableInterface()
    {
        GameObject.Find("GridPlane").GetComponent<Renderer>().enabled = false;

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

    #endregion General UI


    #region Toolbox Functions

    public void OnExplorerClicked()
    {
        VisualElement explorer = ui.Q<VisualElement>("Explorer");
        explorer.style.display = explorer.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;
    }

    public void OnTogglePlayClicked()
    {
        if (simController.isRunning)
        {
            simController.isRunning = false;
            ui.Q<Button>("TogglePlay").Q<VisualElement>("Icon").style.backgroundImage 
                = Resources.Load<Texture2D>("Icons/Toolbox/Options/Playing");
        }
        else
        {
            simController.isRunning = true;
            ui.Q<Button>("TogglePlay").Q<VisualElement>("Icon").style.backgroundImage
                = Resources.Load<Texture2D>("Icons/Toolbox/Options/Paused");
        }
    }

    #endregion Toolbox Functions


    #region Explorer

    // creates a new entry in the explorer for the given body
    public void RefreshExplorer()
    {
        ui.Q<ScrollView>("ExplorerList").Clear();

        foreach (Body body in simController.GetBodies())
        {
            CreateExplorerEntry(body);
        }
    }

    // creates an entry for the given body
    public TemplateContainer CreateExplorerEntry(Body body)
    {
        VisualTreeAsset explorerEntryTemplate = GameObject.Find("ExplorerUI").GetComponent<UIDocument>().visualTreeAsset;
        TemplateContainer explorerEntry = explorerEntryTemplate.Instantiate();

        explorerEntry.Q<Label>("Type").text = body.Type;
        explorerEntry.Q<Label>("Name").text = body.Name;
        explorerEntry.Q<Button>("Delete").visible = false;

        ui.Q<ScrollView>("ExplorerList").Add(explorerEntry);

        explorerEntry.RegisterCallback<ClickEvent>(e =>
        {
            selectedBody = body;
            Camera.main.GetComponent<CameraController>().SetSubject(selectedBody);
            Camera.main.GetComponent<CameraController>().SetScene(CameraController.Mode.Following);
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
            DeleteEntry(explorerEntry, body);
        };

        return explorerEntry;
    }

    // deletes the body associated with the given entry (and the entry itself)
    public void DeleteEntry(TemplateContainer entry, Body body)
    {
        ui.Q<ScrollView>("ExplorerList").Remove(entry);
        Destroy(body.Entity);
        simController.simulation.bodies.Remove(body);
    }

    public void OnExplorerCloseClicked()
    {
        ui.Q<VisualElement>("Explorer").style.display = DisplayStyle.None;
    }

    #endregion Explorer


    #region Mode Selection

    public void SetMode(Mode mode)
    {
        this.mode = mode;
    }

    public void OnModesClicked()
    {
        VisualElement modeList = ui.Q<VisualElement>("ModeList");
        modeList.style.display = modeList.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void OnFreePlacementClicked()    { SelectModeItem("FreePlacement"); }
    private void OnPlanePlacementClicked()   { SelectModeItem("PlanePlacement"); }
    private void OnOrbitPlacementClicked()   { SelectModeItem("OrbitPlacement"); }

    public void SelectModeItem(string modeName)
    {
        if (Enum.TryParse(modeName, out Mode newMode))
        {
            Button oldButton = ui.Q<Button>(mode.ToString());
            oldButton.RemoveFromClassList("selectedButton");

            Button newButton = ui.Q<Button>(modeName);
            newButton.AddToClassList("selectedButton");
            
            mode = newMode;

            GameObject.Find("GridPlane").GetComponent<Renderer>().enabled
                = newMode == Mode.PlanePlacement;
        }
    }

    #endregion Mode Selection


    #region Distance Selection

    
    public void OnDistanceClicked()
    {
        VisualElement distanceList = ui.Q<VisualElement>("DistanceList");
        distanceList.style.display = distanceList.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void OnCentimetreClicked()      { SelectDistanceItem("Centimetre"); }
    private void OnMetreClicked()           { SelectDistanceItem("Metre"); }
    private void OnKilometreClicked()       { SelectDistanceItem("Kilometre"); }
    private void OnMileClicked()            { SelectDistanceItem("Mile"); }
    private void OnLunarDistanceClicked()   { SelectDistanceItem("Lunar Distance"); }
    private void OnAUClicked()              { SelectDistanceItem("AU"); }

    public void SelectDistanceItem(string distanceName)
    {
        string oldDistanceName = SimulatorController.currentScale.Replace(" ", "");
        string newDistanceName = distanceName.Replace(" ", "");

        Button oldButton = ui.Q<Button>(oldDistanceName);
        oldButton.RemoveFromClassList("selectedButton");

        Button newButton = ui.Q<Button>(newDistanceName);
        newButton.AddToClassList("selectedButton");

        SimulatorController.SetScale(distanceName);
        
        UpdateZoomScaleLabel();
    }

    #endregion Distance Selection


    #region Body Selection & Preview

    public Body GetSelectedBody()
    {
        return selectedBody;
    }

    public void OnBodiesClicked()
    {
        VisualElement bodyList = ui.Q<VisualElement>("BodyList");
        bodyList.style.display = bodyList.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void OnAddStarClicked()     { OnBodyItemClicked("Star"); }
    private void OnAddPlanetClicked()   { OnBodyItemClicked("Planet"); }
    private void OnAddMoonClicked()     { OnBodyItemClicked("Moon"); }

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
        Type type = Type.GetType(bodyType);

        // assigning the correct class for the new body
        if (type != null && typeof(Body).IsAssignableFrom(type))
        {
            bodyPreview = (Body) Activator.CreateInstance(type);
        } 
        else 
        {
            Debug.LogWarning($"[SIM UI] ERROR: '{bodyType}' is not a valid Body type!");
        }

        bodyPreview.CreateEntity();
        bodyPreview.Entity.transform.name = "Preview";

        LineRenderer pathDrawer = bodyPreview.Entity.AddComponent<LineRenderer>();
        pathDrawer.material = new Material(Shader.Find("Sprites/Default"));
        pathDrawer.widthMultiplier = 0.2f;

        // line that fades out
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {new(Color.white, 0), new(Color.white, 1)},
            new GradientAlphaKey[] {new(1, 0), new(0, 1)});
        pathDrawer.colorGradient = gradient;

        Material m = bodyPreview.Entity.GetComponent<MeshRenderer>().material;
        m.color = new Color(m.color.r, m.color.g, m.color.b, 0.25f);   
    }

    public void RemoveEntityPreview()
    {
        if (bodyPreview != null)
        {
            if (bodyPreview.Entity != null) 
            {
                bodyPreview.Entity.GetComponent<LineRenderer>().enabled = false;
                Destroy(bodyPreview.Entity);
            }
            bodyPreview = null;
        }
    }

    #endregion Body Selection & Preview


    #region Inputs Loop

    // input loop - manages body placement and other inputs
    void Update()
    {
        if (!isActive) return;

        // system scaling
        float scroll = Input.mouseScrollDelta.y;
        float zoomAdjustment = scroll > 0 ? 0.01f : (scroll < 0 ? -0.01f : 0);

        if (zoomAdjustment != 0)
        {
            double oldScale = SimulatorController.GetScaleCoefficient();
            SimulatorController.AdjustZoomFactor(zoomAdjustment);
            double newScale = SimulatorController.GetScaleCoefficient();
            UpdateZoomScaleLabel();

            // applying the main zoom offset
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 direction = mouseRay.direction * Math.Sign(zoomAdjustment);

            Vector3 spacePosition = (Camera.main.transform.position + direction) / (float) oldScale;
            Camera.main.transform.position = spacePosition * (float) newScale;
        }

        // explorer visibility
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            VisualElement explorer = ui.Q<VisualElement>("Explorer");
            bool visible = explorer.style.display == DisplayStyle.Flex;
            explorer.style.display = visible ? DisplayStyle.None : DisplayStyle.Flex;
        }

        // property updates
        if (selectedBody != null) ShowProperties();

        // body selection & placement
        RunBodySelection();

        if (bodyPreview != null && bodyPreview.Entity != null)
        {
            double scale = SimulatorController.GetScaleCoefficient();

            if (mode == Mode.FreePlacement)         // visual position & visual velocity
            {
                bodyPreview.Position = RunFreePlacement() / (float) scale;      
                bodyPreview.Velocity = GetFreeVelocityDrag() / (float) scale;
                PathDrawer.DrawTrajectory(bodyPreview, bodyPreview.Velocity);
            }
            else if (mode == Mode.PlanePlacement)   // visual position & velocity
            {
                bodyPreview.Position = RunPlanePlacement() / (float) scale;     
                bodyPreview.Velocity = GetPlaneVelocityDrag() / (float) scale;
                PathDrawer.DrawTrajectory(bodyPreview, bodyPreview.Velocity);
            }
            else if (mode == Mode.OrbitPlacement)   // visual position, but TRUE velocity
            {
                Body attractor = null;

                if (selectedBody != null)
                {
                    attractor = selectedBody;
                }
                else
                {
                    Body closestBody = GetBodyClosestToPreview();

                    if (closestBody != null)
                    {
                        attractor = closestBody;
                    }
                }

                if (attractor != null) // placement around the body ("capped" at attractor's radius)
                {
                    bodyPreview.Position = RunOrbitPlacement(attractor) / (float) scale;
                    bodyPreview.Velocity = GetStableOrbitVelocity(attractor, bodyPreview);
                    PathDrawer.DrawOrbitBetweenBodies(attractor, bodyPreview, bodyPreview.Velocity);
                }
                else // standard plane placement
                {
                    bodyPreview.Position = RunPlanePlacement() / (float) scale;
                    bodyPreview.Velocity = Vector3.zero;
                }
            }

            SimulatorController.ApplyScale(bodyPreview);

            RunBodyPlacement();
        }
    }

    // updates the zoom label with the current selected distance and scale
    private void UpdateZoomScaleLabel()
    {
        string zoomText = string.Format("<b>Scale:</b> 1 {0} x {1:0.000}", 
            SimulatorController.currentScale, 1 / Math.Pow(10, SimulatorController.zoomFactor));

        ui.Q<Label>("Scale").text = zoomText;
    }

    #endregion Inputs Loop


    #region Placement & Tools

    // selects and follows the clicked body (if any)
    private void RunBodySelection()
    {
        if (Input.GetMouseButtonDown(0) && !GeneralUI.IsPointerOverAnyUI())
        {
            bool bodyFound = false;
            Vector3 screenPosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            
            // temporarily "blacklists" grid plane & body preview from consuming hits
            GameObject.Find("GridPlane").GetComponent<Collider>().enabled = false;

            if (bodyPreview != null && bodyPreview.Entity != null)
            {
                bodyPreview.Entity.GetComponent<Collider>().enabled = false;
            }

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null)
                {
                    GameObject hitPart = hit.collider.gameObject;
                    Body body = GetBodyFromEntity(hitPart);

                    if (body != null)
                    {
                        if (selectedBody != null)
                        {
                            if (body.Entity != selectedBody.Entity)
                            {
                                bodyFound = true;
                            }
                        }
                        else
                        {
                            bodyFound = true;
                        }
                        
                        if (bodyFound)
                        {
                            selectedBody = body;
                            Camera.main.GetComponent<CameraController>().SetSubject(selectedBody);
                            Camera.main.GetComponent<CameraController>().SetScene(CameraController.Mode.Following);
                            bodyFound = true;
                        }
                    }
                }
            }

            GameObject.Find("GridPlane").GetComponent<Collider>().enabled = true;
            
            if (bodyPreview != null && bodyPreview.Entity != null)
            {
                bodyPreview.Entity.GetComponent<Collider>().enabled = false;
            }
            
            if (!bodyFound)
            {
                selectedBody = null;
                Camera.main.GetComponent<CameraController>().SetScene(CameraController.Mode.Simulator);
                Camera.main.GetComponent<CameraController>().SetSubject(selectedBody);
                HideProperties();
            }
        }

        // reverts to an "empty hand"
        if (selectedBodyItem != null && Input.GetKeyDown(KeyCode.E))
        {
            SelectBodyItem(selectedBodyItem); // as if the button is being toggled off
            RemoveEntityPreview();
        }
    }

    #endregion Placement & Tools


    #region Free Placement

    // allows the user to freely place bodies relative to the camera
    // * position is based on the mouse's position on the screen
    private Vector3 RunFreePlacement()
    {
        /*
        if (!GeneralUI.IsPointerOverAnyUI())
        {
            distanceFromCamera = Math.Max(0, distanceFromCamera - Input.mouseScrollDelta.y * 3);
        }
        */

        if (startVelocityPoint == null)
        {
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = Camera.main.nearClipPlane + distanceFromCamera;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

            return worldPosition;
        }
        else
        {
            return bodyPreview.Entity.transform.position;
        }
    }
    
    // returns a velocity based on the distance the mouse has moved after right click (if any)
    // * velocity is based on the mouse's position on the screen
    private Vector3 GetFreeVelocityDrag()
    {
        // starting the trajectory
        if (Input.GetMouseButtonDown(0) && !GeneralUI.IsPointerOverAnyUI())
        {
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = Camera.main.nearClipPlane + distanceFromCamera;
            startVelocityPoint = Camera.main.ScreenToWorldPoint(screenPosition);
        }

        // ending the trajectory
        if (Input.GetMouseButtonUp(0))
        {
            startVelocityPoint = null;
            return Vector3.zero;
        }
        else if (startVelocityPoint != null)
        {
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = Camera.main.nearClipPlane + distanceFromCamera;
            Vector3 endVelocityPoint = Camera.main.ScreenToWorldPoint(screenPosition);

            return (Vector3) (startVelocityPoint - endVelocityPoint) / 10f;
        }

        return Vector3.zero;
    }

    #endregion Free Placement


    #region Plane Placement

    // allows the user to place bodies on a fixed plane
    // * position is based on the mouse's position on the screen
    private Vector3 RunPlanePlacement()
    {
        if (startVelocityPoint == null)
        {
            Vector3? position = GetMouseOnPlane();
            if (position != null) return (Vector3) position;
        }
        
        return bodyPreview.Entity.transform.position;
    }

    // returns a velocity trajectory for plane mode from the mouse positions
    // * velocity is based on the mouse's position on the screen
    private Vector3 GetPlaneVelocityDrag()
    {
        // starting the trajectory
        if (Input.GetMouseButtonDown(0) && !GeneralUI.IsPointerOverAnyUI())
        {
            startVelocityPoint = GetMouseOnPlane();
        }

        // ending the trajectory
        if (Input.GetMouseButtonUp(0))
        {
            startVelocityPoint = null;
            return Vector3.zero;
        }
        else if (startVelocityPoint != null)
        {
            Vector3? endVelocityPoint = GetMouseOnPlane();
            return endVelocityPoint != null ? (Vector3) (startVelocityPoint - endVelocityPoint) / 10f : Vector3.zero;
        }

        return Vector3.zero;
    }

    // returns the point where the user's mouse intersects the plane Y = 0 (if any)
    // * position is based on the mouse's position on the screen
    private Vector3? GetMouseOnPlane()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (placementPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return null;
    }

    #endregion Plane Placement


    #region Orbit Placement

    // allows the user to place bodies in an orbit relative to other bodies (plane as a reference)
    // * position is based on the mouse's position on the screen
    // * position is also "capped" at the parent's and bodyPreview's combined radii
    private Vector3 RunOrbitPlacement(Body parent)
    {
        Vector3? planePosition = GetMouseOnPlane();
        Vector3 position = (Vector3) (planePosition != null ? planePosition : bodyPreview.Entity.transform.position);

        // scaled to world position
        double scale = SimulatorController.GetScaleCoefficient();
        float radiiSum = (parent.Radius * (float) scale) + (bodyPreview.Radius * (float) scale);
        Vector3 parentPos = parent.Position * (float) scale;
        Vector3 vector = position - parentPos; // parent -> bodyPreview

        if (vector.magnitude > radiiSum)
        {
            return position;
        }
        else
        {
            Vector3 direction = vector.normalized;
            return parentPos + (radiiSum * direction);
        }
    }

    // auxiliary: returns the body which is closest to the body preview
    private Body GetBodyClosestToPreview()
    {
        List<Body> bodies = simController.GetBodies();

        if (bodies.Count == 0) return null;

        Vector3 origin = bodyPreview.Position;
        Body closest = bodies[0];
        float distance = Vector3.Distance(origin, closest.Position);

        for (int b = 1; b < bodies.Count; b++)
        {
            float newDistance = Vector3.Distance(origin, bodies[b].Position);

            if (newDistance < distance)
            {
                closest = bodies[b];
                distance = newDistance;
            }
        }

        return closest;
    }

    // returns the velocity required for the given child body to have a stable orbit around the given parent body
    // * velocity is based on the true positions of each body, which is then scaled accordingly later
    private Vector3 GetStableOrbitVelocity(Body parent, Body child)
    {
        Vector3 centre = parent.Position;
        Vector3 edge = child.Position;

        float r = Vector3.Distance(centre, edge);
        Vector3 gravityDirection = (edge - centre).normalized;
        Vector3 velocityDirection = Vector3.Cross(gravityDirection, Vector3.up);

        float G = simController.simulation.settings.GravityConstant;
        float M = parent.Mass;

        Vector3 velocity = parent.Velocity + (velocityDirection * Mathf.Sqrt(G * M / r));

        return velocity;
    }

    #endregion Orbit Placement


    #region Final Placement

    // places an instance of the selected body down (if any)
    private void RunBodyPlacement()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Vector3 oldPosition = bodyPreview.Entity.transform.position;

            Body body = bodyPreview.GetCopy();
            body.Position = bodyPreview.Position;
            body.Velocity = bodyPreview.Velocity;

            CreateExplorerEntry(body);
            body.CreateEntity();
            simController.AddBody(body);
            SimulatorController.ApplyScale(body);
            RemoveEntityPreview();

            SetEntityPreview(selectedBodyItem);

            bodyPreview.Entity.transform.position = oldPosition;
        }
    }

    #endregion Final Placement


    #region Body Properties
    
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

                RefreshExplorer();
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

       // hides the properties window when the close button is clicked
    public void OnPropertiesCloseClicked()
    {
        HideProperties();

        selectedBody = null;
        Camera.main.GetComponent<CameraController>().SetScene(CameraController.Mode.Simulator);
        Camera.main.GetComponent<CameraController>().SetSubject(selectedBody);
    }

    #endregion Body Properties


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

    #endregion Auxiliary
}
