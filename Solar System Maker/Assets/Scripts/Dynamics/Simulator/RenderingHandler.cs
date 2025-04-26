// manages rendering operations

using System;
using TMPro;
using UnityEngine;

public class RenderingHandler
{

    #region Attributes

    SimulationController simController;
    
    public readonly float UNITS_PER_WORLDSPACE = 1;
    
    private string distance;
    public string Distance {
        get => distance;
        set {
            distance = value;
            scale = CalculateScale();
        }
    }

    private float zoomFactor;
    public float ZoomFactor {
        get => zoomFactor;
        set {
            zoomFactor = value;
            scale = CalculateScale();
        }
    }
    
    private float scale;
    public float Scale {
        get => scale;
        private set => scale = value;
    }

    #endregion Attributes


    #region General Methods

    public RenderingHandler(SimulationController simController)
    {
        this.simController = simController;
        distance = "Kilometre";
        zoomFactor = 0;
        scale = CalculateScale();
    }

    #endregion General Methods


    #region Scaling Methods

    // updates the scale with the current scale metric and zoom factor
    public float CalculateScale()
    {
        return (float) (1 / UnitReference.DISTANCE_UNITS[distance] 
            * Math.Pow(10, zoomFactor) * UNITS_PER_WORLDSPACE);
    }

    // extra: applies the current scale to the given body
    public void ApplyScale(Body body)
    {
        body.Entity.transform.position = (float) scale * body.Position;
        body.Entity.transform.localScale = (float) (body.Radius * 2) * scale * new Vector3(1,1,1);
    }

    #endregion Scaling Methods


    #region Label Methods

    // renders the given body's label
    // * positions the label at "top right" of body, relative to the camera
    public void RenderLabel(Body body)
    {
        Transform origin = Camera.main.transform;
        Vector3 bodyPosition = body.Entity.transform.position;
        Vector3 bodyEdge = body.Radius * scale * (origin.right + origin.up); // parallel to camera forward
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(bodyPosition + bodyEdge);

        body.Label.transform.position = screenPosition;
        body.Label.GetComponent<TextMeshProUGUI>().enabled = screenPosition.z >= 0;

        // ^ technically has a screen position even when BEHIND the camera
        // * this check stops the label from showing up when behind the camera
    }

    #endregion Label Methods


    #region Path Drawing

    // supplies the given body's entity with a line renderer for use
    public void GiveLineRenderer(Body body)
    {
        if (body.Entity == null) return;

        LineRenderer pathDrawer = body.Entity.AddComponent<LineRenderer>();
        pathDrawer.material = new Material(Shader.Find("Sprites/Default"));
        pathDrawer.widthMultiplier = 0.2f;

        Gradient gradient = new();

        gradient.SetKeys( // line that fades out
            new GradientColorKey[] {new(Color.white, 0), new(Color.white, 1)},
            new GradientAlphaKey[] {new(1, 0), new(0, 1)});

        pathDrawer.colorGradient = gradient;
    }

    // draws a trajectory for the given body based on the given vector
    public void DrawTrajectory(Body body, Vector3 trajectory)
    {
        Vector3 scaledOrigin = body.Position * scale;
        Vector3 scaledTrajectory = trajectory * scale;

    	LineRenderer pathDrawer = body.Entity.GetComponent<LineRenderer>();
	    pathDrawer.positionCount = 2;

        pathDrawer.SetPosition(0, scaledOrigin);
        pathDrawer.SetPosition(1, scaledOrigin + scaledTrajectory);
    }

    // draws the orbit of one body around another
    public void DrawOrbitBetweenBodies(Body parent, Body child, Vector3 childVelocity)
    {
        Vector3 centre = parent.Position;
        Vector3 edge = child.Position;

        int resolution = 100;
        // float step = 1f; // time step
        Vector3[] points = new Vector3[resolution + 1];

        LineRenderer pathDrawer = child.Entity.GetComponent<LineRenderer>();
	    pathDrawer.positionCount = resolution;

        Vector3 scaledCentre = (float) scale * centre;
        Vector3 scaledEdge = (float) scale * edge;
        Vector3 direction = scaledCentre - scaledEdge;
        float scaledRadius = direction.magnitude;
        float startingAngle = Vector3.SignedAngle(direction, -Vector3.right, Vector3.up) * Mathf.Deg2Rad;

        for (int p = 0; p <= resolution; p++)
        {
            // equation-based path prediction
            float progress = (float) p / resolution;
            float radians = startingAngle + (progress * 2 * Mathf.PI);

            float x = Mathf.Cos(radians) * scaledRadius;
            float z = Mathf.Sin(radians) * scaledRadius;

            points[p] = scaledCentre + new Vector3(x, 0, z);
        }

        pathDrawer.SetPositions(points);
    }

    #endregion Path Drawing


    #region Trail Drawing

    // supplies the given body with a trail renderer to show where it has passed
    public void GiveTrailRenderer(Body body)
    {
        if (body.Entity == null) return;

        TrailRenderer trailDrawer = body.Entity.AddComponent<TrailRenderer>();
        trailDrawer.material = new Material(Shader.Find("Sprites/Default"));
        trailDrawer.widthMultiplier = 0.2f;

        Gradient gradient = new();
        
        gradient.SetKeys( // trail that fades out
            new GradientColorKey[] {new(Color.white, 0), new(Color.white, 1)},
            new GradientAlphaKey[] {new(1, 0), new(0, 1)});

        trailDrawer.colorGradient = gradient;
        trailDrawer.time = 60;
    }

    #endregion Trail Drawing
}
