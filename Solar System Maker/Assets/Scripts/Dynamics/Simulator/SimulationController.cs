// Manages the core Simulation functionality
// * the Simulator acts as a container which runs the physics for all bodies
// * it itself does NOT manage UI directly

using UnityEngine;
using System.Collections.Generic;
using System;

public class SimulationController : MonoBehaviour
{

    #region Attributes

    public Simulation simulation;
    public new RenderingHandler renderer; // reserved library? hence new
    public PhysicsHandler physics;

    public enum PlacementMode { FreePlacement, PlanePlacement, OrbitPlacement }
    public PlacementMode placementMode;
    public GameObject simulatorSpace;
    public GameObject simulatorPlane;
    
    private Body selectedBody;
    public Body SelectedBody { 
        get => selectedBody; 
        private set => selectedBody = value; 
    }
    // ^ NB: "Body" is serialisable, SO KEEP THIS PRIVATE
    //   (otherwise Unity defaults it to new Body())

    private Body bodyPreview;
    public Body BodyPreview { 
        get => bodyPreview; 
        private set => bodyPreview = value;
    }
    
    private Vector3? startVelocityPoint;
    private Plane placementPlane = new(Vector3.up, Vector3.zero);
    
    public event Action<Body> OnBodyAdded;
    public event Action<Body> OnBodyRemoved;
    public event Action<Body> OnBodySelected;
    public event Action<Body> OnBodyDeselected;
    public event Action<float> OnScaleChanged;
    public event Action OnPhysicsUpdate;

    public List<SpaceSnapshot> snapshots;

    private bool isActive = false;
    public bool IsActive {
        get => isActive;
        set => isActive = value;
    }

    private bool showLabels = true;
    public bool ShowLabels {
        get => showLabels;
        set {
            showLabels = value;

            foreach (Body body in simulation.bodies)
            {
                body.SetLabelVisibility(showLabels);
            }

            if (bodyPreview != null && bodyPreview.Label != null)
            {
                bodyPreview.SetLabelVisibility(showLabels);
            }
        }
    }

    private bool isRunning = false;
    public bool IsRunning {
        get => isRunning;
        set => isRunning = value;
    }

    private bool isRewinding = false;
    public bool IsRewinding {
        get => isRewinding;
        set => isRewinding = value;
    }

    #endregion Attributes


    #region Simulator Setup

    public void Start()
    {
        renderer = new(this);
        physics = new(this);
        selectedBody = null;
    }

    // loads the Simulator with the given simulation
    // * also creates a physical entity for each body in case they don't have one
    public void LoadSimulation(Simulation simulation)
    {
        this.simulation = simulation;

        foreach (Body body in simulation.bodies)
        {
            if (body.Entity == null) body.CreateEntity();
        }

        snapshots = new List<SpaceSnapshot>();
    }

    // clears all bodies in the Simulator
    public void ClearSimulator()
    {
        foreach (Body body in GetBodies())
        {
            body.DestroyEntity();
        }

        snapshots = null;
    }

    #endregion Simulator Setup


    #region Simulator Methods

    // returns all bodies in the simulation
    public List<Body> GetBodies()
    {
        return simulation.bodies;
    }

    // adds the given body to the simulation
    public void AddBody(Body body)
    {
        body.Entity.transform.SetParent(simulatorSpace.transform);
        
        simulation.bodies.Add(body);

        OnBodyAdded?.Invoke(body);
    }

    // removes the given body from the simulation
    public void RemoveBody(Body body)
    {
        if (selectedBody == body)
        {
            DeselectBody();
            CameraController.SetSubject(null);
            CameraController.SetCameraMode(CameraController.CameraMode.Simulator);
        }

        simulation.bodies.Remove(body);

        OnBodyRemoved?.Invoke(body);
        
        body.DestroyEntity();
    }

    public void DisableSimulator()
    {
        SetPlaneVisibility(false);

        IsActive = false;
        
        if (bodyPreview != null) 
        {
            bodyPreview.DestroyEntity();
            bodyPreview = null;

            // TODO: change camera mode to Main Menu?
        }
        
        startVelocityPoint = null;
    }

    #endregion Simulator Methods


    #region Input Loop

    void Update()
    {
        if (!isActive) return;

        // run keyboard / mouse controls
        RunControls();

        // body placement
        if (bodyPreview != null && bodyPreview.Entity != null)
        {
            RunPreviewBearings();
            renderer.ApplyScale(bodyPreview);
            RunPreviewPlacement();

            if (showLabels) renderer.RenderLabel(bodyPreview);
        }

        // body selection
        RunBodySelection();

        // label rendering
        if (showLabels)
        {
            foreach(Body body in simulation.bodies)
            {
                renderer.RenderLabel(body);
            }
        }
    }

    // runs basic controls / keybinds
    public void RunControls()
    {
        // on mouse wheel - zoom in / out
        float scroll = Input.mouseScrollDelta.y;
        float zoomAdjustment = scroll > 0 ? 0.01f : (scroll < 0 ? -0.01f : 0);

        if (zoomAdjustment != 0)
        {
            float oldScale = renderer.Scale;
            renderer.ZoomFactor += zoomAdjustment;
            float newScale = renderer.Scale;

            // applying the main zoom offset
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 direction = mouseRay.direction * Math.Sign(zoomAdjustment);

            Vector3 spacePosition = (Camera.main.transform.position + direction) / (float) oldScale;
            Camera.main.transform.position = spacePosition * (float) newScale;

            OnScaleChanged?.Invoke(newScale);
        }

        // when "WASD" pressed - switch back to simulator mode from follow mode
        Vector3 input = new(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        
        if (input.x != 0 || input.z != 0)
        {
            CameraController.SetCameraMode(CameraController.CameraMode.Simulator);
            CameraController.SetSubject(null);
        }

        // when "F" pressed - toggle following for the selected body (if any)
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (CameraController.GetCameraMode() == CameraController.CameraMode.Simulator)
            {
                if (selectedBody != null)
                {
                    CameraController.SetSubject(selectedBody);
                    CameraController.SetCameraMode(CameraController.CameraMode.Following);
                }   
            }
            else
            {
                CameraController.SetSubject(null);
                CameraController.SetCameraMode(CameraController.CameraMode.Simulator);
            }
        }

        // when "V" pressed - set an appropriate scale to view the subject (if in follow mode)
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (CameraController.GetCameraMode() == CameraController.CameraMode.Following)
            {
                float targetDiameter = 5;

                double value = targetDiameter * UnitReference.DISTANCE_UNITS[renderer.Distance] 
                    / (2 * selectedBody.Radius * renderer.UNITS_PER_WORLDSPACE);

                renderer.ZoomFactor = (float) Math.Log10(value);

                // NB: the mathemagics behind this so i don't forget

                // * adjusts the zoom factor so that the diameter of the body meets the
                //     target diameter in world space based on the current scale metrics

                // 1. from the renderer, to scale bodies, we have:

                //    > float scaledRadius = body.Radius * scale
                //    > body.Entity.transform.localScale = 2 * scaledRadius * new Vector3(1,1,1);

                // so the target diameter would be:

                //      ~ D = 2 * body.Radius * scale
                //  ~ scale = D / (2 * body.Radius)                                   (1) 

                // 2. from the renderer, we also have the following to determine scale:

                //    > float scale = 1 / UnitReference.DISTANCE_UNITS[distance]      (2)
                //         * Math.Pow(10, zoomFactor) * UNITS_PER_WORLDSPACE;                

                // 3. substituting (2) into (1), we have:

                //    ~ 1 / U * Math.Pow(10, zoomFactor) * Uws = D / (2 * body.Radius)
                
                // -- where "U" is the unit of distance and "Uws" is UNITS_PER_WORLDSPACE (for shorthand)

                // 4. rearranging the equation to make "zoomFactor" the subject, we get:

                //    ~ zoomFactor = log10(D * U / (2 * body.Radius * Uws))
            }
        }
    }

    #endregion Input Loop


    #region Simulation Loop

    // main physics loop - applies planet forces between each other
    void FixedUpdate()
    {
        if (!isActive) return;

        if (simulation.settings.GravityAlgorithm == "Newtonian Physics")
        {
            if (isRunning)
            {
                ApplyPhysics(); // includes scaling
            }
            else
            {
                foreach (Body body in GetBodies())
                    renderer.ApplyScale(body);
            }
        }

        OnPhysicsUpdate?.Invoke();
    }
    

    // algorithm v1: applies forced-based gravity between all bodies
    // * calculations are done body-by-body
    // * per body A, apply that body's force to body Bs,
    //     and the forces of body Bs to itself
    // * then, because body A is done, update its forces for this round
    public void ApplyPhysics()
    {
        List<Body> bodies = simulation.bodies;

        for (int a = 0; a < bodies.Count; a++)
        {
            Body bodyA = bodies[a];

            for (int b = a + 1; b < bodies.Count; b++)
            {
                Body bodyB = bodies[b];

                Vector3 force = physics.GetGravitationalForce(bodyA, bodyB,
                    simulation.settings.GravityConstant);

                bodyA.ApplyForce(force);
                bodyB.ApplyForce(-force);
            }

            bodyA.Update();

            renderer.ApplyScale(bodyA);
        }
    }
    
    #endregion Simulation Loop


    #region Preview Management

    // creates a new preview instance of the given type
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
            Debug.LogWarning($"ERROR: '{bodyType}' is not a valid Body type!");
        }

        bodyPreview.CreateEntity();
        bodyPreview.Name = "New " + type.ToString();
        bodyPreview.Entity.transform.SetParent(simulatorSpace.transform);

        renderer.GiveStreak(bodyPreview);
    }

    // removes the current body preview
    public void RemoveEntityPreview()
    {
        if (bodyPreview != null)
        {
            if (bodyPreview.Entity != null) 
            {
                bodyPreview.Entity.GetComponent<LineRenderer>().enabled = false;
                bodyPreview.DestroyEntity();
            }
            bodyPreview = null;
        }
    }

    #endregion Preview Management


    #region Preview Placement

    // sets the method to use when placing a body preview
    public void SetPlacementMode(string modeName)
    {
        if (Enum.TryParse(modeName, out PlacementMode newMode))
        {   
            placementMode = newMode;
            SetPlaneVisibility(newMode == PlacementMode.PlanePlacement);
        }
        else
        {
            Debug.Log($"[ERROR] '{modeName}' is an invalid mode!");
        }
    }

    // gathers the REAL position and velocity to set the preview body at
    // * "REAL" is relative to space, not the scene
    public void RunPreviewBearings()
    {
        float scale = renderer.Scale;

        if (placementMode == PlacementMode.FreePlacement)         // visual position & visual velocity
        {
            bodyPreview.Position = RunFreePlacement() / scale;      
            bodyPreview.Velocity = GetFreeVelocityDrag() / scale;
            renderer.DrawTrajectory(bodyPreview, bodyPreview.Velocity);
        }
        else if (placementMode == PlacementMode.PlanePlacement)   // visual position & velocity
        {
            bodyPreview.Position = RunPlanePlacement() / scale;     
            bodyPreview.Velocity = GetPlaneVelocityDrag() / scale;
            renderer.DrawTrajectory(bodyPreview, bodyPreview.Velocity);
        }
        else if (placementMode == PlacementMode.OrbitPlacement)   // visual position, but TRUE velocity
        {
            startVelocityPoint = null; // no dragging for fixed orbits!

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
                float G = simulation.settings.GravityConstant;
                bodyPreview.Position = RunOrbitPlacement(attractor) / scale;
                bodyPreview.Velocity = physics.GetStableOrbitVelocity(attractor, bodyPreview, G);
                renderer.DrawOrbitBetweenBodies(attractor, bodyPreview, bodyPreview.Velocity);
            }
            else // standard plane placement
            {
                bodyPreview.Position = RunPlanePlacement() / scale;
                bodyPreview.Velocity = Vector3.zero;
            }
        }
    }

    // places an instance of the selected body down (if any)
    private void RunPreviewPlacement()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Body body = bodyPreview.GetCopy(); // incl. Position & Velocity
            body.CreateEntity();
            body.Name = body.Type;
            AddBody(body);
            renderer.ApplyScale(body);

            if (showLabels) renderer.RenderLabel(body);
        }
    }

    #endregion Preview Placement


    #region Free Placement

    // allows the user to freely place bodies relative to the camera
    // * position is based on the mouse's position on the screen
    private Vector3 RunFreePlacement()
    {
        if (startVelocityPoint == null)
        {
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = Camera.main.nearClipPlane + 30;
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
        if (Input.GetMouseButtonDown(0)
            && !GameObject.Find("UI").GetComponent<UIController>().IsMouseOverUI())
        {
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = Camera.main.nearClipPlane + 30;
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
            screenPosition.z = Camera.main.nearClipPlane + 30;
            Vector3 endVelocityPoint = Camera.main.ScreenToWorldPoint(screenPosition);

            return (Vector3) (startVelocityPoint - endVelocityPoint) / 10f;
        }

        return Vector3.zero;
    }

    #endregion Free Placement


    #region Plane Placement

    // sets the visibility of the plane
    public void SetPlaneVisibility(bool visible)
    {
        simulatorPlane.GetComponent<Renderer>().enabled = visible;
    }

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
        if (Input.GetMouseButtonDown(0) 
            && !GameObject.Find("UI").GetComponent<UIController>().IsMouseOverUI())
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
        double scale = renderer.Scale;
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
        List<Body> bodies = simulation.bodies;

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

    #endregion Orbit Placement


    #region Body Selection

    // explicitly selects the given body
    public void SelectBody(Body body)
    {
        selectedBody = body;
        OnBodySelected?.Invoke(body);
    }

    // deselects any currently selected body
    public void DeselectBody()
    {
        OnBodyDeselected?.Invoke(selectedBody);
        selectedBody = null;
    }

    // selects and follows the clicked body (if any)
    private void RunBodySelection()
    {
        if (Input.GetMouseButtonDown(0) 
            && !GameObject.Find("UI").GetComponent<UIController>().IsMouseOverUI())
        {
            Vector3 screenPosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            
            // temporarily "blacklists" grid plane & body preview from consuming hits
            simulatorPlane.GetComponent<Collider>().enabled = false;

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
                        SelectBody(body);
                        CameraController.SetSubject(selectedBody);
                        CameraController.SetCameraMode(CameraController.CameraMode.Following);
                    }
                }
            }

            simulatorPlane.GetComponent<Collider>().enabled = true;
            
            if (bodyPreview != null && bodyPreview.Entity != null)
            {
                bodyPreview.Entity.GetComponent<Collider>().enabled = false;
            }
        }
    }

    #endregion Body Selection


    
    #region Auxiliary

    // returns the body from the list of bodies which matches the give entity
    public Body GetBodyFromEntity(GameObject entity)
    {
        foreach (Body body in simulation.bodies)
        {
            if (body.Entity == entity) return body;
        }

        return null;
    }

    #endregion Auxiliary
}