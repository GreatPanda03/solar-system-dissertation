// controls core simulation functionality
// * the Simulator acts as a container which runs the physics for all bodies
// * it itself does NOT manage UI directly

using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class SimulationController : MonoBehaviour
{

    #region Attributes

    public Simulation simulation;
    public new RenderingHandler renderer;   // reserved keyword? hence new?
    public PhysicsHandler physics;

    public List<RenderableBody> rBodies;

    public GameObject simulatorSpace;       // general placement
    public GameObject simulatorPlane;
    private Vector3? startVelocityPoint;    // fire by dragging
    private Plane placementPlane 
        = new(Vector3.up, Vector3.zero);    // plane placement
    public bool planeEnabled = false;
    public bool stickToPlane = false;
    public bool orbitEnabled = false;       // orbit placement
    public double eccentricity = 0;
    public double orbitDirection = -1;

    // these two are just to refresh trails when scaling
    private double lastZoomFactor;
    private string lastDistance;
    
    public event Action<RenderableBody> OnBodyAdded;
    public event Action<RenderableBody> OnBodyRemoved;
    public event Action<RenderableBody> OnBodySelected;
    public event Action<RenderableBody> OnBodyDeselected;
    public event Action<double> OnScaleChanged;
    public event Action OnPhysicsUpdate;
    public event Action<double, Body> OnOrbitChanged;

    public List<SpaceSnapshot> snapshots;

    private RenderableBody selectedBody;
    public RenderableBody SelectedBody { 
        get => selectedBody; 
        private set => selectedBody = value; 
    }
    // ^ NB: "Body" is serialisable, SO KEEP THIS PRIVATE
    //   (otherwise Unity defaults it to new Body() - bad if there isn't actually a body)

    private RenderableBody preview;
    public RenderableBody Preview { 
        get => preview; 
        private set => preview = value; // separate method
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
            
            if (value == true)
            {
                ShowAllLabels();
            }
            else
            {
                HideAllLabels();
            }
        }
    }

    private bool showTrails = true;
    public bool ShowTrails {
        get => showTrails;
        set {
            showTrails = value;
            
            if (value == true)
            {
                ShowAllTrails();
            }
            else
            {
                HideAllTrails();
            }
        }
    }

    private bool showHabitableZones = false;
    public bool ShowHabitableZones {
        get => showHabitableZones;
        set {
            showHabitableZones = value;

            if (value == true)
            {
                ShowAllHabitableZones();
            }
            else
            {
                HideAllHabitableZones();
            }
        }
    }

    private float trailTime = 60f;
    public float TrailTime {
        get => trailTime;
        set {
            trailTime = value;
            ReadjustAllTrails();
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
        rBodies = new();

        renderer = new();
        physics = new();

        selectedBody = null;
    }

    // loads the Simulator with the given simulation
    // * also creates a physical entity for each body in case they don't have one
    public void LoadSimulation(Simulation simulation)
    {
        this.simulation = simulation;

        rBodies = new();

        foreach (Body body in simulation.bodies)
        {
            RenderableBody rBody = new()
            {
                Body = body
            };

            rBody.CreateEntity();
            rBody.CreateLabel();
            rBody.CreateTrail();

            // slightly different to "AddBody" below
            rBody.Entity.transform.SetParent(simulatorSpace.transform);
            rBodies.Add(rBody);
            OnBodyAdded?.Invoke(rBody);
        }

        snapshots = new List<SpaceSnapshot>();
    }

    // clears all bodies in the Simulator
    public void ClearSimulator()
    {
        foreach (RenderableBody rBody in rBodies)
        {
            rBody.DestroyEntity();
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

    // adds the given renderable body to the simulation
    public void AddBody(RenderableBody rBody)
    {
        rBody.Entity.transform.SetParent(simulatorSpace.transform);
        
        simulation.bodies.Add(rBody.Body);
        rBodies.Add(rBody);

        OnBodyAdded?.Invoke(rBody);
    }

    // removes the given renderable body from the simulation
    public void RemoveBody(RenderableBody rBody)
    {
        if (rBody == null) return;

        if (selectedBody == rBody)
        {
            DeselectBody();
            CameraController.SetToUnfollow();
        }

        simulation.bodies.Remove(rBody.Body);
        rBodies.Remove(rBody);

        OnBodyRemoved?.Invoke(rBody);
        
        rBody.DestroyEntity();
    }

    // disables the simulator entirely
    public void DisableSimulator()
    {
        DisablePlane();

        FullDisable();
        
        if (preview != null) 
        {
            preview.DestroyEntity();
            preview = null;
        }
        
        startVelocityPoint = null;

        CameraController.Subject = null;
    }

    
    // returns the body from the list of bodies which matches the give entity
    public RenderableBody GetRenderableBodyFromEntity(GameObject entity)
    {
        foreach (RenderableBody rBody in rBodies)
        {
            if (rBody.Entity == entity) 
            {
                return rBody;
            }
        }

        return null;
    }

    #endregion Simulator Methods


    #region Input Loop

    void Update()
    {
        // simulatorPlane.GetComponent<MeshRenderer>().material.SetVector(
        //     "_VirtualPosition", (Vector3) CameraController.VirtualPosition);

        if (!isActive) return;

        // run keyboard / mouse controls
        if (IsInteractable)
        {
            RunControls();

            // body placement
            if (preview != null && preview.Entity != null)
            {
                RunPreviewBearings();
                preview.RenderBody(Vector3d.zero, renderer.Scale);
                RunPreviewPlacement();

                if (showLabels)
                {
                    preview.RenderLabel();
                }

                if (preview is RenderableStar starPreview)
                {
                    if (starPreview.ShowHabitableZone)
                    {
                        starPreview.RenderHabitableZone(renderer.Scale);
                    }
                }
            }

            // body selection
            RunBodySelection();
        }

        // label rendering
        if (showLabels)
        {
            foreach(RenderableBody rBody in rBodies)
            {
                if (rBody.ShowLabel)
                {
                    rBody.RenderLabel();
                }
            }
        }

        // habitable zone rendering - special case
        if (showHabitableZones)
        {
            foreach (RenderableBody rBody in rBodies)
            {
                if (rBody is RenderableStar rStar)
                {
                    if (rStar.ShowHabitableZone)
                    {
                        rStar.RenderHabitableZone(renderer.Scale);
                    }
                }
            }
        }
    }

    // runs basic controls / keybinds
    public void RunControls()
    {
        // on mouse wheel - zoom in / out
        double scroll = Input.mouseScrollDelta.y;
        double zoomAdjustment = scroll > 0 ? 0.01f : (scroll < 0 ? -0.01f : 0);

        if (zoomAdjustment != 0)
        {
            double oldScale = renderer.Scale;
            SetZoomFactor(renderer.ZoomFactor + zoomAdjustment);
            double newScale = renderer.Scale;

            // applying the main zoom offset
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 direction = mouseRay.direction * Math.Sign(zoomAdjustment);

            Vector3d spacePosition = (Vector3d) (Camera.main.transform.position + direction) / (float) oldScale;
            Camera.main.transform.position = (Vector3) (spacePosition * (double) newScale);

            // also moves the subject to make zooming a followed body more seamless
            if (CameraController.Subject != null)
            {
                CameraController.Subject.RenderBody(Vector3d.zero, newScale);
                CameraController.Subject.Trail.Clear();
            }

            OnScaleChanged?.Invoke(newScale);
        }

        // key-sensitive controls (to not interfere with actual typing)
        if (UIController.FocusedField == null)
        {
            // when "WASD" pressed - switch back to simulator mode from follow mode
            Vector3d input = new(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        
            if ((input.x != 0 || input.z != 0) && selectedBody != null)
            {
                CameraController.SetToUnfollow();
            }

            // when "F" pressed - toggle following for the selected body (if any)
            if (Input.GetKeyDown(AppControls.keybinds["Follow"]))
            {
                if (CameraController.CameraMode == CameraController.Mode.Free)
                {
                    if (selectedBody != null)
                    {
                        CameraController.SetToFollow(selectedBody);
                    }   
                }
                else
                {
                    CameraController.SetToUnfollow();
                }
            }

            // when "V" pressed - set an appropriate scale to view the subject (if in follow mode)
            if (Input.GetKeyDown(AppControls.keybinds["Adjust Scale"]))
            {
                if (selectedBody != null && CameraController.CameraMode == CameraController.Mode.Follow)
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
                    TranslatePlane(new Vector3d(0, -1, 0));
                }

                // when "I" pressed - raise the plane
                if (Input.GetKey(AppControls.keybinds["Raise Plane"]))
                {
                    TranslatePlane(new Vector3d(0, 1, 0));
                }

                // when "J" pressed - rotate the plane left
                if (Input.GetKey(AppControls.keybinds["Turn Plane Left"]))
                {
                    TurnPlane(new Vector3d(-1, 0, 0));
                }

                // when "K" pressed - rotate the plane right
                if (Input.GetKey(AppControls.keybinds["Turn Plane Right"]))
                {
                    TurnPlane(new Vector3d(1, 0, 0));
                }

                // when "N" pressed - rotate the plane backwards
                if (Input.GetKey(AppControls.keybinds["Turn Plane Backwards"]))
                {
                    TurnPlane(new Vector3d(0, 0, -1));
                }

                // when "M" pressed - rotate the plane forward
                if (Input.GetKey(AppControls.keybinds["Turn Plane Forwards"]))
                {
                    TurnPlane(new Vector3d(0, 0, 1));
                }

                // when "O" pressed - reset plane to origin
                if (Input.GetKey(AppControls.keybinds["Reset Plane"]))
                {
                    SetPlanePositionAndRotation(Vector3.zero, Quaternion.Euler(90, 0, 0));
                }
            }

            // orbit mode specific keybinds
            if (orbitEnabled)
            {
                // when "Z" pressed - decrease eccentricity (only to 0)
                if (Input.GetKey(AppControls.keybinds["Decrease Eccentricity"]))
                {
                    eccentricity = Math.Max(eccentricity - 0.001f, 0);
                }

                // when "X" pressed - increase eccentricity (only to 0.999)
                if (Input.GetKey(AppControls.keybinds["Increase Eccentricity"]))
                {
                    eccentricity = Math.Min(eccentricity + 0.001f, 0.99f); // to avoid zero-division
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
            ApplyPhysics();
        }
        
        foreach (RenderableBody rBody in rBodies)
        {
            // use only xz offset here, since y follows the camera anyway
            // Vector3d offset = new(CameraController.VirtualPosition.x, 0, CameraController.VirtualPosition.z);
            // Vector3d offset = CameraController.VirtualPosition;
            rBody.RenderBody(Vector3d.zero, renderer.Scale);
        }

        if (lastZoomFactor != renderer.ZoomFactor || lastDistance != renderer.Distance)
        {
            lastZoomFactor = renderer.ZoomFactor;
            lastDistance = renderer.Distance;
            ReadjustAllTrails();
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
        List<RenderableBody> bodiesToRemove = new();

        // * playing with increasing simulation calculations / update, rather than rate
        // time loop - simulates up to "t" seconds at a time
        // for (int t = 1; t <= UnitReference.TIME_UNITS[physics.TimePerSecond]; t++)
        // { 
            // main loop - runs physics
            for (int a = 0; a < rBodies.Count; a++)
            {
                RenderableBody rBodyA = rBodies[a];
                Body bodyA = rBodyA.Body;

                // gravity / collisions loop - mutual attraction, so "cut in half"
                for (int b = a + 1; b < rBodies.Count; b++)
                {
                    RenderableBody rBodyB = rBodies[b];
                    Body bodyB = rBodyB.Body;

                    // collision handling - stop here if colliding
                    Body majorBody = physics.IsColliding(bodyA, bodyB);

                    if (majorBody != null)
                    {
                        RenderableBody rMajorBody = majorBody == bodyA ? rBodyA : rBodyB;
                        RenderableBody rMinorBody = majorBody == bodyA ? rBodyB : rBodyA;

                        physics.ApplyCollision(majorBody, rMinorBody.Body);
                        bodiesToRemove.Add(rMinorBody);

                        if (selectedBody == rMinorBody) // swap over to dominating body
                        {
                            CameraController.SetToFollow(rMajorBody);
                        }

                        continue;
                    }
                    
                    // if no collisions, continue with general forces
                    Vector3d gravity = physics.GetGravitationalForce(bodyA, bodyB,
                        simulation.settings.GravitationalConstant * (double) UnitReference.G);

                    if (!bodyA.Anchored) 
                    {
                        physics.ApplyForce(bodyA, gravity);
                    }

                    if (!bodyB.Anchored)
                    {
                        physics.ApplyForce(bodyB, -gravity);
                    }
                }

                // temperature loop - special case for star / sub-body interaction
                if (bodyA is Star star)
                {
                    foreach (RenderableBody rBody in rBodies)
                    {
                        if (rBody.Body is SubBody subBody)
                        {
                            double power = physics.GetPowerReceived(star, subBody);
                            physics.ApplyPower(subBody, power);
                        }
                    }
                }

                // final update
                physics.Update(bodyA, physics.TimeCoefficient);
                
                // bodyA.Update(Time.deltaTime);
            }

            // removes any bodies marked for removal by physics
            foreach (RenderableBody body in bodiesToRemove)
            {
                RemoveBody(body);
            }
       ///}
    }
    
    #endregion Simulation Loop


    #region Scale Management

    // sets the zoom factor of the controller's renderer to the given value
    public void SetZoomFactor(double zoomFactor)
    {
        renderer.ZoomFactor = zoomFactor;
        ReadjustAllTrails();
    }

    // sets the distance unit of the controller's renderer to the given value
    public void SetDistanceScale(string distance)
    {
        renderer.Distance = distance;
        ReadjustAllTrails();
    }

    // scales the simulation to fit the given body
    public void ScaleToFit(RenderableBody rBody)
    {
        if (rBody == null) return;

        double targetDiameter = 5;

        double value = targetDiameter * UnitReference.DISTANCE_UNITS[renderer.Distance] 
            / (2 * rBody.Body.Radius * renderer.UNITS_PER_WORLDSPACE);

        SetZoomFactor((double) Math.Log10(value));

        OnScaleChanged?.Invoke(renderer.ZoomFactor);

        // NB: the mathemagics behind this so i don't forget

        // * adjusts the zoom factor so that the diameter of the body meets the
        //     target diameter in world space based on the current scale metrics

        // 1. from the renderer, to scale bodies, we have:

        //    > double scaledRadius = body.Radius * scale
        //    > body.Entity.transform.localScale = 2 * scaledRadius * new Vector3d(1,1,1);

        // so the target diameter would be:

        //      ~ D = 2 * body.Radius * scale
        //  ~ scale = D / (2 * body.Radius)                                   (1) 

        // 2. from the renderer, we also have the following to determine scale:

        //    > double scale = 1 / UnitReference.DISTANCE_UNITS[distance]      (2)
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
    public void SetPreviewByType(string bodyType)
    {       
        Type type = Type.GetType(bodyType);
        Body body;

        // assigning the correct class for the new body
        if (type != null && typeof(Body).IsAssignableFrom(type))
        {
            body = (Body) Activator.CreateInstance(type);
        } 
        else 
        {
            Debug.Log($"ERROR: '{bodyType}' is not a valid Body type - defaulting to Body");
            body = new Body();
        }

        if (body is Star)
        {
            preview = new RenderableStar() { Body = body };
        }
        else
        {
            preview = new() { Body = body };
        }

        preview.Body.Name = "New " + type.ToString();

        PreparePreviewVisuals();
    }

    // creates a new preview instance from the given body
    public void SetPreviewByTemplate(BodyTemplate template)
    {
        RemoveEntityPreview();

        if (template.Body is Star)
        {
            preview = new RenderableStar() { Body = template.Body.GetCopy() };
        }
        else
        {
            preview = new() { Body = template.Body.GetCopy() };
        }
        
        preview.Body.Name = template.Body.Name;

        PreparePreviewVisuals();
    }

    // auxiliary: prepares the visuals for the body preview
    public void PreparePreviewVisuals()
    {
        if (preview == null) return;

        preview.CreateEntity();
        preview.CreateLabel();
        preview.CreateLine();

        if (preview is RenderableStar previewStar)
        {
            previewStar.CreateHabitableZone();
            previewStar.SetHabitableZoneVisibility(showHabitableZones);
        }

        preview.Entity.transform.parent = simulatorSpace.transform;

        Material m = preview.Entity.GetComponent<MeshRenderer>().material;
        m.color = new Color(m.color.r, m.color.g, m.color.b, 0.5f);
    }

    // removes the current body preview
    public void RemoveEntityPreview()
    {
        if (preview != null)
        {
            if (preview.Entity != null) 
            {
                preview.Entity.GetComponent<LineRenderer>().enabled = false;
                preview.DestroyEntity();
            }

            preview = null;
        }
    }

    #endregion Preview Management


    #region Preview Placement

    // gathers the REAL position and velocity to set the preview body at
    // * "REAL" is relative to space, not the scene
    public void RunPreviewBearings()
    {
        // determine position
        if (planeEnabled && stickToPlane)
        {
            preview.Body.Position = RunPlanePlacement();
        }
        else
        {
            preview.Body.Position = RunFreePlacement();
        }

        // determine velocity
        if (orbitEnabled)
        {
            RenderableBody attractor = null;

            if (selectedBody != null)
            {
                attractor = selectedBody;
            }
            else
            {
                RenderableBody closestBody = GetBodyClosestToPreview();

                if (closestBody != null)
                {
                    attractor = closestBody;
                }
            }

            if (attractor != null) // also restricts to beyond attractor's radius
            {
                double G = simulation.settings.GravitationalConstant * UnitReference.G;

                preview.Body.Position = RestrictToRadiusOfTarget(preview.Body, attractor.Body);
                
                preview.Body.Velocity = physics.GetStableOrbitVelocity(
                    attractor.Body, preview.Body, G, eccentricity, orbitDirection);
                
                renderer.DrawOrbitAroundPoint(
                    preview, attractor.Body.Position, eccentricity, orbitDirection);
            
                OnOrbitChanged?.Invoke(eccentricity, attractor.Body);
            }
            else
            {
                OnOrbitChanged?.Invoke(eccentricity, null);

                preview.Line.positionCount = 0;
            }
        }
        else
        {
            if (stickToPlane)
            {
                preview.Body.Velocity = GetPlaneVelocityDrag();    
            }
            else
            {
                preview.Body.Velocity = GetFreeVelocityDrag();
            }
            
            renderer.DrawTrajectory(preview, preview.Body.Velocity);
        }
    }

    // places an instance of the selected body down (if any)
    private void RunPreviewPlacement()
    {
        if (Input.GetKeyDown(AppControls.keybinds["Place Body"]) 
            && UIController.FocusedField == null)
        {
            // preview.Body.Position += CameraController.VirtualPosition / renderer.Scale;

            RenderableBody rBody;

            if (preview is RenderableStar)
            {
                rBody = new RenderableStar() { Body = preview.Body.GetCopy() };
            }
            else
            {
                rBody = new() { Body = preview.Body.GetCopy() };
            }
        
            rBody.CreateEntity();
            rBody.CreateLabel();
            rBody.CreateTrail();

            rBody.Trail.widthMultiplier = (float) Math.Max(rBody.Body.Radius * renderer.Scale, 0.1f);

            // use full xyz offset here
            // CameraController.VirtualPosition
            rBody.RenderBody(Vector3d.zero, renderer.Scale);

            AddBody(rBody);

            if (showLabels) 
            {
                // CameraController.VirtualPosition
                rBody.RenderLabel();
            }

            if (showHabitableZones)
            {
                if (rBody is RenderableStar rStar)
                {
                    rStar.RenderHabitableZone(renderer.Scale);
                }
            }
        }
    }

    #endregion Preview Placement


    #region Free Placement

    // returns a space position relative to a distance from the user's camera
    private Vector3d RunFreePlacement()
    {
        if (startVelocityPoint == null)
        {
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = Camera.main.nearClipPlane + 30;
            Vector3d worldPosition = (Vector3d) Camera.main.ScreenToWorldPoint(screenPosition);

            return worldPosition / renderer.Scale;
        }
        else
        {
            return preview.Body.Position;
        }
    }
    
    // returns a space velocity relative to the user's "drag and release"
    private Vector3d GetFreeVelocityDrag()
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
            return Vector3d.zero;
        }
        else if (startVelocityPoint != null)
        {
            Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = Camera.main.nearClipPlane + 30;
            Vector3 endVelocityPoint = Camera.main.ScreenToWorldPoint(screenPosition);

            return (Vector3d) (startVelocityPoint - endVelocityPoint) / renderer.Scale;
        }

        return Vector3d.zero;
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
    private Vector3d RunPlanePlacement()
    {
        if (startVelocityPoint == null)
        {
            Vector3? position = GetMouseOnPlane();

            if (position != null) 
            {
                return (Vector3d) position / renderer.Scale;
            }
        }
        
        return preview.Body.Position;
    }

    // returns the space velocity relative to the user's "drag and release" along the plane
    private Vector3d GetPlaneVelocityDrag()
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
            return Vector3d.zero;
        }
        else if (startVelocityPoint != null)
        {
            Vector3? endVelocityPoint = GetMouseOnPlane();

            return endVelocityPoint != null 
                ? (Vector3d) (startVelocityPoint - endVelocityPoint) / (float) renderer.Scale
                : Vector3d.zero;
        }

        return Vector3d.zero;
    }

    // updates the placement plane to the given position and rotation
    public void SetPlanePositionAndRotation(
        Vector3 position, Quaternion rotation)
    {
        simulatorPlane.transform.position = position;
        simulatorPlane.transform.rotation = rotation;

        placementPlane.SetNormalAndPosition(
            simulatorPlane.transform.forward, simulatorPlane.transform.position);
    }

    // adjusts the plane by the given vector
    public void TranslatePlane(Vector3d shift)
    {
        SetPlanePositionAndRotation(
            simulatorPlane.transform.position + (Vector3) shift,
            simulatorPlane.transform.rotation);
    }

    // turns the plane by the given rotation
    public void TurnPlane(Vector3d rotation)
    {
        simulatorPlane.transform.Rotate((Vector3) rotation);

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
    private Vector3d RestrictToRadiusOfTarget(Body subject, Body target)
    {
        double radiiSum = target.Radius + preview.Body.Radius;
        Vector3d vector = subject.Position - target.Position; // parent -> preview

        if (vector.magnitude > radiiSum)
        {
            return subject.Position;
        }
        else
        {
            Vector3d direction = vector.normalized;
            return target.Position + (radiiSum * direction);
        }
    }

    // auxiliary: returns the body which is closest to the body preview
    private RenderableBody GetBodyClosestToPreview()
    {
        if (rBodies.Count == 0) return null;

        Vector3d origin = preview.Body.Position;
        RenderableBody closest = rBodies[0];
        double distance = Vector3d.Distance(origin, closest.Body.Position);

        for (int b = 1; b < rBodies.Count; b++)
        {
            RenderableBody candidate = rBodies[b];
            double newDistance = Vector3d.Distance(origin, candidate.Body.Position);

            if (newDistance < distance)
            {
                closest = candidate;
                distance = newDistance;
            }
        }

        return closest;
    }

    #endregion Orbit Placement


    #region Body Selection

    // explicitly selects the given body
    public void SelectBody(RenderableBody rBody)
    {
        selectedBody = rBody;
        OnBodySelected?.Invoke(rBody);
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

            if (preview != null && preview.Entity != null)
            {
                preview.Entity.GetComponent<Collider>().enabled = false;
            }

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null)
                {
                    GameObject hitPart = hit.collider.gameObject;
                    var rBody = GetRenderableBodyFromEntity(hitPart);



                    if (rBody != null)
                    {
                        SelectBody(rBody);
                        CameraController.SetToFollow(selectedBody);
                    }
                }
            }

            simulatorPlane.GetComponent<Collider>().enabled = true;
            
            if (preview != null && preview.Entity != null)
            {
                preview.Entity.GetComponent<Collider>().enabled = false;
            }
        }
    }

    #endregion Body Selection


    #region Label Methods

    // shows all labels (depending on individual labels)
    public void ShowAllLabels()
    {
        foreach (RenderableBody rBody in rBodies)
        {
            rBody.SetLabelVisibility(rBody.ShowLabel);
        }

        preview?.SetLabelVisibility(true);
    }

    // sets the visibility of all labels (dependent on independent labels)
    public void HideAllLabels()
    {
        foreach (RenderableBody body in rBodies)
        {
            body.SetLabelVisibility(false);
        }
        
        preview?.SetLabelVisibility(false);
    }

    #endregion Label Methods


    #region Trail Methods

    // shows all trails (depending on individual trails)
    public void ShowAllTrails()
    {
        foreach (RenderableBody body in rBodies)
        {
            body.SetTrailVisibility(body.ShowTrail);
        }
    }

    public void HideAllTrails()
    {
        foreach (RenderableBody body in rBodies)
        {
            body.SetTrailVisibility(false);
        }
    }

    // readjusts trails (mainly for scale changes since they're a bit buggy)
    public void ReadjustAllTrails()
    {
        foreach (RenderableBody rBody in rBodies)
        {
            if (rBody.Trail == null) continue;

            rBody.Trail.Clear();
            rBody.Trail.widthMultiplier = (float) Math.Max(rBody.Body.Radius * renderer.Scale, 0.1f);
            rBody.Trail.time = (float) trailTime;
        }
    }

    #endregion Trail Methods


    #region Habitable Zone Methods

    // shows all habitable zones (depending on independent habitable zones)
    public void ShowAllHabitableZones()
    {
        foreach (RenderableBody body in rBodies)
        {
            if (body is RenderableStar rStar)
            {
                rStar.SetHabitableZoneVisibility(rStar.ShowHabitableZone);
            }
        }

        if (preview is RenderableStar previewStar)
        {
            previewStar.SetHabitableZoneVisibility(true);
        }
    }

    public void HideAllHabitableZones()
    {
        foreach (RenderableBody body in rBodies)
        {
            if (body is RenderableStar rStar)
            {
                rStar.SetHabitableZoneVisibility(false);
            }
        }

        if (preview is RenderableStar previewStar)
        {
            previewStar.SetHabitableZoneVisibility(false);
        }
    }

    #endregion Habitable Zone Methods
}