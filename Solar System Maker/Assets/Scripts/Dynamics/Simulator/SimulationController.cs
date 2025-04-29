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
    public new RenderingHandler renderer;   // reserved keyword? hence new?
    public PhysicsHandler physics;
    
    public GameObject simulatorSpace;       // general placement
    public GameObject simulatorPlane;
    private Vector3? startVelocityPoint;    // fire by dragging
    private Plane placementPlane 
        = new(Vector3.up, Vector3.zero);    // plane placement
    public bool planeEnabled = false;
    public bool stickToPlane = false;
    public bool orbitEnabled = false;      // orbit placement
    public float eccentricity = 0;
    public float orbitDirection = -1;
    public float lastScale; // only used to clear trails when scaled
    
    public event Action<Body> OnBodyAdded;
    public event Action<Body> OnBodyRemoved;
    public event Action<Body> OnBodySelected;
    public event Action<Body> OnBodyDeselected;
    public event Action<float> OnScaleChanged;
    public event Action OnPhysicsUpdate;
    public event Action<float, Body> OnOrbitChanged;

    public List<SpaceSnapshot> snapshots;

    private Body selectedBody;
    public Body SelectedBody { 
        get => selectedBody; 
        private set => selectedBody = value; 
    }
    // ^ NB: "Body" is serialisable, SO KEEP THIS PRIVATE
    //   (otherwise Unity defaults it to new Body() - bad if there isn't actually a body)

    private Body bodyPreview;
    public Body BodyPreview { 
        get => bodyPreview; 
        private set => bodyPreview = value;
    }

    private bool isActive = false;
    public bool IsActive {
        get => isActive;
        set => isActive = value;
    }

    private bool isInteractable = false;
    public bool IsInteractable {
        get => isInteractable;
        set => isInteractable = value;
    }

    private bool showLabels = true;
    public bool ShowLabels {
        get => showLabels;
        set {
            showLabels = value;

            if (showLabels)
            {
                foreach (Body body in simulation.bodies)
                {
                    body.SetLabelVisibility(body.ShowLabel);
                }

                if (bodyPreview?.Label != null)
                {
                    bodyPreview.SetLabelVisibility(true);
                }
            }
            else
            {
                foreach (Body body in simulation.bodies)
                {
                    body.SetLabelVisibility(false);
                }

                if (bodyPreview?.Label != null)
                {
                    bodyPreview.SetLabelVisibility(false);
                }
            }
        }
    }

    private bool showTrails = true;
    public bool ShowTrails {
        get => showTrails;
        set {
            showTrails = value;
            
            if (showTrails)
            {
                foreach (Body body in simulation.bodies)
                {
                    body.SetTrailVisibility(body.ShowTrail);
                }
            }
            else
            {
                foreach (Body body in simulation.bodies)
                {
                    body.SetTrailVisibility(false);
                }
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
            body.CreateEntity();
            renderer.GiveTrailRenderer(body);
            OnBodyAdded?.Invoke(body);
        }

        snapshots = new List<SpaceSnapshot>();
    }

    // clears all bodies in the Simulator
    public void ClearSimulator()
    {
        foreach (Body body in simulation.bodies)
        {
            body.DestroyEntity();
        }

        snapshots = null;
    }

    public void FullEnable()
    {
        IsActive = true;
        IsInteractable = true;
        IsRunning = true;
        ShowLabels = true;
        ShowTrails = true;
    }

    public void FullDisable()
    {
        IsActive = false;
        IsInteractable = false;
        IsRunning = false;
        ShowLabels = false;
        ShowTrails = false;
    }

    #endregion Simulator Setup


    #region Simulator Methods

    // adds the given body to the simulation
    public void AddBody(Body body)
    {
        body.Entity.transform.parent = simulatorSpace.transform;
        
        simulation.bodies.Add(body);

        OnBodyAdded?.Invoke(body);
    }

    // removes the given body from the simulation
    public void RemoveBody(Body body)
    {
        if (body == null) return;

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
        DisablePlane();

        FullDisable();
        
        if (bodyPreview != null) 
        {
            bodyPreview.DestroyEntity();
            bodyPreview = null;
        }
        
        startVelocityPoint = null;

        CameraController.SetSubject(null);
    }

    #endregion Simulator Methods


    #region Input Loop

    void Update()
    {
        if (!isActive) return;

        // keep track of whether to clear trails
        float initialScale = renderer.Scale;

        // run keyboard / mouse controls
        if (IsInteractable)
        {
            RunControls();

            // body placement
            if (bodyPreview != null && bodyPreview.Entity != null)
            {
                RunPreviewBearings();
                renderer.ApplyScale(bodyPreview);
                RunPreviewPlacement();

                if (showLabels)
                {
                    renderer.RenderLabel(bodyPreview);
                }
            }

            // body selection
            RunBodySelection();
        }

        // label rendering
        if (showLabels)
        {
            foreach(Body body in simulation.bodies)
            {
                if (body.ShowLabel)
                {
                    renderer.RenderLabel(body);
                }
            }
        }

        // TEMPORARY: re-render trails if scale was changed
        if (renderer.Scale != initialScale)
        {
            if (showTrails) // clear trails to remove streaks
            {
                foreach (Body body in simulation.bodies)
                {
                    body.Entity.GetComponent<TrailRenderer>().Clear();
                }
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

        // key-sensitive controls (to not interfere with actual typing)
        if (UIController.FocusedField == null)
        {
            // when "WASD" pressed - switch back to simulator mode from follow mode
            Vector3 input = new(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        
            if (input.x != 0 || input.z != 0)
            {
                CameraController.SetCameraMode(CameraController.CameraMode.Simulator);
                CameraController.SetSubject(null);
            }

            // when "F" pressed - toggle following for the selected body (if any)
            if (Input.GetKeyDown(AppControls.keybinds["Follow"]))
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
            if (Input.GetKeyDown(AppControls.keybinds["Adjust Scale"]))
            {
                if (CameraController.GetCameraMode() == CameraController.CameraMode.Following)
                {
                    ScaleToFit(selectedBody);
                }
            }

            // plane mode specific keybinds
            if (planeEnabled)
            {
                // when "U" pressed - move plane down
                if (Input.GetKey(AppControls.keybinds["Lower Plane"]))
                {
                    TranslatePlane(new Vector3(0, -1, 0));
                }

                // when "I" pressed - raise the plane
                if (Input.GetKey(AppControls.keybinds["Raise Plane"]))
                {
                    TranslatePlane(new Vector3(0, 1, 0));
                }

                // when "J" pressed - rotate the plane left
                if (Input.GetKey(AppControls.keybinds["Turn Plane Left"]))
                {
                    TurnPlane(new Vector3(-1, 0, 0));
                }

                // when "K" pressed - rotate the plane right
                if (Input.GetKey(AppControls.keybinds["Turn Plane Right"]))
                {
                    TurnPlane(new Vector3(1, 0, 0));
                }

                // when "N" pressed - rotate the plane backwards
                if (Input.GetKey(AppControls.keybinds["Turn Plane Backwards"]))
                {
                    TurnPlane(new Vector3(0, 0, -1));
                }

                // when "M" pressed - rotate the plane forward
                if (Input.GetKey(AppControls.keybinds["Turn Plane Forwards"]))
                {
                    TurnPlane(new Vector3(0, 0, 1));
                }

                // when "O" pressed - reset plane to origin
                if (Input.GetKey(AppControls.keybinds["Reset Plane"]))
                {
                    SetPlanePositionAndRotation(Vector3.zero, Quaternion.Euler(90, 0, 0));
                }
            }

            // orbit mode specific keybinds
            if (planeEnabled)
            {
                // when "Z" pressed - decrease eccentricity (only to 0)
                if (Input.GetKey(AppControls.keybinds["Decrease Eccentricity"]))
                {
                    eccentricity = Mathf.Max(eccentricity - 0.001f, 0);
                }

                // when "X" pressed - increase eccentricity (only to 0.999)
                if (Input.GetKey(AppControls.keybinds["Increase Eccentricity"]))
                {
                    eccentricity = Mathf.Min(eccentricity + 0.001f, 0.99f); // to avoid zero-division
                }

                // when "C" pressed - swap orbit direction
                if (Input.GetKeyDown(AppControls.keybinds["Flip Orbit"]))
                {
                    orbitDirection *= -1;
                }
            }
        }
    }

    #endregion Input Loop


    #region Simulation Loop

    // main physics loop - applies body interactions to each other
    // * FixedUpdate happens at a constant rate, regardless of framerate - use it!
    void FixedUpdate()
    {
        if (!isActive) return;

        if (isRunning)
        {
            ApplyPhysics(); // includes scaling
        }
        
        foreach (Body body in simulation.bodies)
        {
            renderer.ApplyScale(body);
        }

        // refresh trails if scale has changed
        if (lastScale != renderer.Scale)
        {
            lastScale = renderer.Scale;

            foreach (Body body in simulation.bodies)
            {
                body.Entity.GetComponent<TrailRenderer>().Clear();
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
                    simulation.settings.GravitationalConstant * (float) UnitReference.G);

                if (!bodyA.Anchored) 
                {
                    bodyA.ApplyForce(force);
                }

                bodyB.ApplyForce(-force);
            }

            if (!bodyA.Anchored)
            {
                bodyA.Update(physics.TimeCoefficient);
            }
        }
    }
    
    #endregion Simulation Loop


    #region Scale Management

    // scales the simulation to fit the given body
    public void ScaleToFit(Body body)
    {
        if (body == null) return;

        float targetDiameter = 5;

        double value = targetDiameter * UnitReference.DISTANCE_UNITS[renderer.Distance] 
            / (2 * body.Radius * renderer.UNITS_PER_WORLDSPACE);

        renderer.ZoomFactor = (float) Math.Log10(value);

        OnScaleChanged?.Invoke(renderer.ZoomFactor);

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

    #endregion Scale Management


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
            Debug.Log($"ERROR: '{bodyType}' is not a valid Body type - defaulting to Body");
            bodyPreview = new Body();
        }

        bodyPreview.Name = "New " + type.ToString();

        PreparePreviewVisuals();
    }

    // creates a new preview instance from the given body
    public void SetEntityPreviewToTemplate(BodyTemplate template)
    {
        bodyPreview = template.Body;
        bodyPreview.Name = template.Name;

        PreparePreviewVisuals();
    }

    // auxiliary: prepares the visuals for the body preview
    public void PreparePreviewVisuals()
    {
        if (bodyPreview == null) return;

        bodyPreview.CreateEntity();
        bodyPreview.Entity.transform.parent = simulatorSpace.transform;

        renderer.GiveLineRenderer(bodyPreview);

        Material m = bodyPreview.Entity.GetComponent<MeshRenderer>().material;
        m.color = new Color(m.color.r, m.color.g, m.color.b, 0.25f);
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

    // gathers the REAL position and velocity to set the preview body at
    // * "REAL" is relative to space, not the scene
    public void RunPreviewBearings()
    {
        // determine position
        if (stickToPlane)
        {
            bodyPreview.Position = RunPlanePlacement();
        }
        else
        {
            bodyPreview.Position = RunFreePlacement();
        }

        // determine velocity
        if (orbitEnabled)
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

            if (attractor != null) // also restricts to beyond attractor's radius
            {
                float G = simulation.settings.GravitationalConstant * (float) UnitReference.G;

                bodyPreview.Position = RestrictToRadiusOfTarget(bodyPreview, attractor);
                
                bodyPreview.Velocity = physics.GetStableOrbitVelocity(
                    attractor, bodyPreview, G, eccentricity, orbitDirection);
                
                renderer.DrawOrbitBetweenBodies(
                    attractor, bodyPreview, eccentricity, orbitDirection);
            }

            OnOrbitChanged?.Invoke(eccentricity, attractor);
        }
        else
        {
            if (stickToPlane)
            {
                bodyPreview.Velocity = GetPlaneVelocityDrag();    
            }
            else
            {
                bodyPreview.Velocity = GetFreeVelocityDrag();
            }
            
            renderer.DrawTrajectory(bodyPreview, bodyPreview.Velocity);
        }
    }

    // places an instance of the selected body down (if any)
    private void RunPreviewPlacement()
    {
        if (Input.GetKeyDown(AppControls.keybinds["Place Body"]) && UIController.FocusedField == null)
        {
            Body body = bodyPreview.GetShallowCopy(); // incl. Position & Velocity
            body.CreateEntity();
            body.Name = body.Type;
            AddBody(body);
            renderer.ApplyScale(body);

            renderer.GiveTrailRenderer(body);

            if (showLabels) renderer.RenderLabel(body);
        }
    }

    #endregion Preview Placement


    #region Free Placement

    // returns a space position relative to a distance from the user's camera
    private Vector3 RunFreePlacement()
    {
        if (startVelocityPoint == null)
        {
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = Camera.main.nearClipPlane + 30;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

            return worldPosition / renderer.Scale;
        }
        else
        {
            return bodyPreview.Position;
        }
    }
    
    // returns a space velocity relative to the user's "drag and release"
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

            return (Vector3) (startVelocityPoint - endVelocityPoint) / renderer.Scale;
        }

        return Vector3.zero;
    }

    #endregion Free Placement


    #region Plane Placement

    // enables the plane
    public void EnablePlane()
    {
        planeEnabled = true;
        simulatorPlane.GetComponent<Renderer>().enabled = true;
    }

    // disables the plane
    public void DisablePlane()
    {
        simulatorPlane.GetComponent<Renderer>().enabled = false;
        planeEnabled = false;
    }

    // enables sticking to the plane
    public void EnableStickToPlane()
    {
        stickToPlane = true;
    }

    // disables sticking to the plane
    public void DisableStickToPlane()
    {
        stickToPlane = false;
    }

    // returns the world point where the user's mouse meets the plane
    private Vector3? GetMouseOnPlane()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (placementPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return null;
    }

    // returns the space position of the mouse's position along the plane (intersection)
    private Vector3 RunPlanePlacement()
    {
        if (startVelocityPoint == null)
        {
            Vector3? position = GetMouseOnPlane();
            
            if (position != null) 
            {
                return (Vector3) position / renderer.Scale;
            }
        }
        
        return bodyPreview.Position;
    }

    // returns the space velocity relative to the user's "drag and release" along the plane
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

            return endVelocityPoint != null 
                ? (Vector3) (startVelocityPoint - endVelocityPoint) / renderer.Scale
                : Vector3.zero;
        }

        return Vector3.zero;
    }

    // updates the placement plane to the given position and rotation
    public void SetPlanePositionAndRotation(
        Vector3 position, Quaternion rotation)
    {
        simulatorPlane.transform.position = position;
        simulatorPlane.transform.rotation = rotation;

        placementPlane.SetNormalAndPosition(
            simulatorPlane.transform.up, simulatorPlane.transform.position);
    }

    // adjusts the plane by the given vector
    public void TranslatePlane(Vector3 shift)
    {
        SetPlanePositionAndRotation(
            simulatorPlane.transform.position + shift,
            simulatorPlane.transform.rotation);
    }

    // turns the plane by the given rotation
    public void TurnPlane(Vector3 rotation)
    {
        simulatorPlane.transform.Rotate(rotation);

        SetPlanePositionAndRotation(
            simulatorPlane.transform.position,
            simulatorPlane.transform.rotation);
    }

    #endregion Plane Placement


    #region Orbit Placement

    // enables orbit mode
    public void EnableOrbit()
    {
        orbitEnabled = true;
        startVelocityPoint = null; // no dragging for orbits
        // * ^ MUST do this, otherwise locked out of positioning
    }

    // disables orbit mode
    public void DisableOrbit()
    {
        orbitEnabled = false;
    }

    // returns a space position where the subject lies at least at the edge of the target
    private Vector3 RestrictToRadiusOfTarget(Body subject, Body target)
    {
        float radiiSum = target.Radius + bodyPreview.Radius;
        Vector3 vector = subject.Position - target.Position; // parent -> bodyPreview

        if (vector.magnitude > radiiSum)
        {
            return subject.Position;
        }
        else
        {
            Vector3 direction = vector.normalized;
            return target.Position + (radiiSum * direction);
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