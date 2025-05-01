// handles rendering operations
// * this class is standalone and can be used to calculate / apply operations
//     given the appropriate bodies, values, etc.

using System;
using TMPro;
using UnityEngine;

public class RenderingHandler
{

    #region Attributes
    
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

    public RenderingHandler()
    {
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
    public void RenderBody(RenderableBody rBody)
    {
        if (rBody.Entity == null) return;

        Transform transform = rBody.Entity.transform;
        transform.position = (float) scale * rBody.Body.Position;
        transform.localScale = (float) (rBody.Body.Radius * 2) * scale * new Vector3(1,1,1);
        transform.rotation = Quaternion.Euler(0, rBody.Body.Rotation * Mathf.Rad2Deg, 0);
    }

    #endregion Scaling Methods


    #region Label Methods

    // renders the given body's label
    // * positions the label at "top right" of body, relative to the camera
    public void RenderLabel(RenderableBody rBody)
    {
        if (rBody.Label == null) return;

        Transform origin = Camera.main.transform;
        Vector3 position = rBody.Entity.transform.position;
        float size = rBody.Entity.transform.localScale.magnitude / 2; // radius

        Vector3 edge = size * (origin.right + origin.up); // parallel to camera forward
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(position + edge);

        rBody.Label.transform.position = screenPoint;
        rBody.Label.GetComponent<TextMeshProUGUI>().enabled = screenPoint.z >= 0;

        // ^ technically has a screen position even when BEHIND the camera
        // * this check stops the label from showing up when behind the camera
    }

    #endregion Label Methods


    #region Trajectory Methods


    // draws a trajectory for the given body based on the given vector
    public void DrawTrajectory(RenderableBody rBody, Vector3 trajectory)
    {
        if (rBody.Line == null) return;

        Vector3 scaledOrigin = rBody.Body.Position * scale;
        Vector3 scaledTrajectory = trajectory * scale;

	    rBody.Line.positionCount = 2;
        rBody.Line.SetPosition(0, scaledOrigin);
        rBody.Line.SetPosition(1, scaledOrigin + scaledTrajectory);
    }

    // draws the orbit of one body around another
    // * source (ellipse in 3D):    https://math.stackexchange.com/questions/3994666/parametric-equation-of-an-ellipse-in-the-3d-space#:~:text=with%20c%3D(c1%2C,v2%2Cv3).
    // * source (orbital velocity): https://www.youtube.com/watch?v=Wy2Sihu2dWk
    // * LOTS of maths...
    public void DrawOrbitAroundPoint(
        RenderableBody satellite, 
        Vector3 point, 
        float eccentricity = 0,
        float orbitDirection = 1)
    {
        if (satellite.Line == null) return;

        // measurements
        Vector3 centre = point;
        Vector3 edge = satellite.Body.Position;

        Vector3 scaledCentre = (float) scale * centre;
        Vector3 scaledEdge = (float) scale * edge;
        Vector3 scaledVector = scaledEdge - scaledCentre; // from child to parent
        float scaledDistance = scaledVector.magnitude;
        
        // "degrees diff" is where the path draws from based on child position relative to parent
        // * "abs" so the path doesn't "flip", "offset" to account for inclination
        float degreesDiff = Mathf.Abs(Vector3.SignedAngle(scaledVector, Vector3.up, Vector3.forward));
        float scaledXZ = Mathf.Sqrt(Mathf.Pow(scaledVector.x, 2) + Mathf.Pow(scaledVector.z, 2));
        float radiansOffset = Mathf.Atan2(scaledVector.y, scaledXZ); // proportion of Pi / 2 to offset
        float startingRadians = (degreesDiff * Mathf.Deg2Rad) + radiansOffset;

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

	    satellite.Line.positionCount = resolution;

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

        satellite.Line.SetPositions(points);
    }

    #endregion Trajectory Methods
}
