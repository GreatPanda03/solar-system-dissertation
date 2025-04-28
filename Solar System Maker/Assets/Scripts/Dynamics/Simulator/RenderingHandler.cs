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
            new GradientAlphaKey[] {new(1, 0), new(0.1f, 1f)});

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
    // * source (ellipse in 3D):    https://math.stackexchange.com/questions/3994666/parametric-equation-of-an-ellipse-in-the-3d-space#:~:text=with%20c%3D(c1%2C,v2%2Cv3).
    // * source (orbital velocity): https://www.youtube.com/watch?v=Wy2Sihu2dWk
    // * LOTS of maths...
    public void DrawOrbitBetweenBodies(
        Body parent, 
        Body child, 
        float eccentricity = 0,
        float orbitDirection = -1)
    {

        // measurements
        Vector3 centre = parent.Position;
        Vector3 edge = child.Position;

        Vector3 scaledCentre = (float) scale * centre;
        Vector3 scaledEdge = (float) scale * edge;
        Vector3 scaledVector = scaledEdge - scaledCentre; // from child to parent
        float scaledDistance = scaledVector.magnitude;
        
        // "degrees diff" is where the path draws from based on child position relative to parent
        // * "abs" so the path doesn't "flip", "offset" to account for inclination
        float degreesDiff = Mathf.Abs(Vector3.SignedAngle(scaledVector, Vector3.up, Vector3.forward));
        float scaledXZ = Mathf.Sqrt(Mathf.Pow(scaledVector.x, 2) + Mathf.Pow(scaledVector.z, 2));
        float radiansOffset = Mathf.Atan2(scaledVector.y, scaledXZ); // accounts for 0 too!
        float startingRadians = (degreesDiff * Mathf.Deg2Rad) - radiansOffset;

        // full distance of "a", with its scale restored (since the above is ratio-based)
        float a = scaledDistance / (1 - eccentricity);
        float b = Mathf.Sqrt(Mathf.Pow(a, 2) - Mathf.Pow(a * eccentricity, 2));

        // ellipse components (A and B are vertex and co-vertex respectively)
        Vector3 A = a * scaledVector.normalized;
        Vector3 B = b * Vector3.Cross(scaledVector, Vector3.up).normalized;
        Vector3 orbitCentre = scaledEdge - A;

        // calculations (for reference - figured out with desmos and intuition + sources above!):
        // * NOTE: the orbit is considered a VERTICAL ellipse!
            // a_p = 1 - E              -- ratio (proportion of major-axis covered by pericentre)
            // a_r = 1 + E              -- ratio (proportion of major-axis covered by apocentre)
            // a = 0.5(a_p + a_r) * r   -- actual semi-major axis
            // a = a * 1 / (1 - E)      -- scale back to original size
            // b = root(a^2 - (ae)^2)
            // :. (b * cos(t), ae + asin(t))

            // where "r" is the distance between the PARENT and CHILD bodies (NOT "a")

        // path drawing
        int resolution = 100;
        Vector3[] points = new Vector3[resolution + 1];

        LineRenderer pathDrawer = child.Entity.GetComponent<LineRenderer>();
	    pathDrawer.positionCount = resolution;

        for (int p = 0; p <= resolution; p++)
        {
            // equation-based path prediction
            float progress = (float) p / resolution;
            float radians = startingRadians + orbitDirection * (progress * 2 * Mathf.PI);

            // * METHOD 1: plain circle along plane Y = 0
            // float x = Mathf.Cos(radians) * scaledRadius;
            // float z = Mathf.Sin(radians) * scaledRadius;
            // points[p] = scaledCentre + new Vector3(x, 0, z);
            
            // * METHOD 2: plain circle along any axis
            // Vector3 U = scaledDistance * Mathf.Cos(radians);
            // Vector3 V = scaledDistance * Mathf.Sin(radians);
            // points[p] = orbitCentre + U + V;

            // * METHOD 3: ellipse along any axis  
            Vector3 U = B * Mathf.Cos(radians);
            Vector3 V = A * Mathf.Sin(radians); // adding aE doesn't work here - not sure why...
            points[p] = orbitCentre + U + V;
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
