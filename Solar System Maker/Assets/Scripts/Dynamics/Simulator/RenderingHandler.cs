// handles rendering operations
// * this class is standalone and can be used to calculate / apply operations
//     given the appropriate bodies, values, etc.

using System;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Animations;

public class RenderingHandler
{

    #region Attributes
    
    public readonly double UNITS_PER_WORLDSPACE = 1;

    private string distance;
    public string Distance {
        get => distance;
        set {
            distance = value;
            scale = CalculateScale();
        }
    }

    private double zoomFactor;
    public double ZoomFactor {
        get => zoomFactor;
        set {
            zoomFactor = value;
            scale = CalculateScale();
        }
    }
    
    private double scale;
    public double Scale {
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
    public double CalculateScale()
    {
        return (double) (1 / UnitReference.DISTANCE_UNITS[distance] 
            * Math.Pow(10, zoomFactor) * UNITS_PER_WORLDSPACE);
    }

    #endregion Scaling Methods


    #region Trajectory Methods


    // draws a trajectory for the given body based on the given vector
    public void DrawTrajectory(RenderableBody rBody, Vector3d trajectory)
    {
        if (rBody.Line == null) return;

        Vector3d scaledOrigin = rBody.Body.Position * scale;
        Vector3d scaledTrajectory = trajectory * scale;

	    rBody.Line.positionCount = 2;
        rBody.Line.SetPosition(0, (Vector3) scaledOrigin);
        rBody.Line.SetPosition(1, (Vector3) (scaledOrigin + scaledTrajectory));
    }

    // draws the orbit of one body around another
    // * source (ellipse in 3D):    https://math.stackexchange.com/questions/3994666/parametric-equation-of-an-ellipse-in-the-3d-space#:~:text=with%20c%3D(c1%2C,v2%2Cv3).
    // * source (orbital velocity): https://www.youtube.com/watch?v=Wy2Sihu2dWk
    // * LOTS of maths...
    public void DrawOrbitAroundPoint(
        RenderableBody satellite, 
        Vector3d point, 
        double eccentricity = 0,
        double orbitDirection = 1)
    {
        if (satellite.Line == null) return;

        // measurements
        Vector3d centre = point;
        Vector3d edge = satellite.Body.Position;

        Vector3d scaledCentre = (double) scale * centre;
        Vector3d scaledEdge = (double) scale * edge;
        Vector3d scaledVector = scaledEdge - scaledCentre; // from child to parent
        double scaledDistance = scaledVector.magnitude;
        
        // "degrees diff" is where the path draws from based on child position relative to parent
        // * "abs" so the path doesn't "flip", "offset" to account for inclination
        double degreesDiff = Math.Abs(Vector3.SignedAngle((Vector3) scaledVector, Vector3.up, Vector3.forward));
        double scaledXZ = Math.Sqrt(Math.Pow(scaledVector.x, 2) + Math.Pow(scaledVector.z, 2));
        double radiansOffset = Math.Atan2(scaledVector.y, scaledXZ); // proportion of Pi / 2 to offset
        double startingRadians = (degreesDiff * Mathf.Deg2Rad) + radiansOffset;

        // NB: "Vector3d" class doesn't include "Vector3.SignedAngle", so have to settle on floats (which is fine)

        // full distance of "a", with its scale restored (since the above is ratio-based)
        double a = scaledDistance / (1 - eccentricity);
        double b = Math.Sqrt(Math.Pow(a, 2) - Math.Pow(a * eccentricity, 2));

        // ellipse components (A and B are vertex and co-vertex respectively)
        Vector3d A = a * scaledVector.normalized;
        Vector3d B = b * Vector3d.Cross(scaledVector, Vector3d.up).normalized;
        Vector3d orbitCentre = scaledEdge - A;

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
            double progress = (double) p / resolution;
            double radians = startingRadians + orbitDirection * (progress * 2 * Math.PI);

            // * METHOD 1: plain circle along plane Y = 0
            // double x = Math.Cos(radians) * scaledRadius;
            // double z = Math.Sin(radians) * scaledRadius;
            // points[p] = scaledCentre + new Vector3d(x, 0, z);
            
            // * METHOD 2: plain circle along any axis
            // Vector3d U = scaledDistance * Math.Cos(radians);
            // Vector3d V = scaledDistance * Math.Sin(radians);
            // points[p] = orbitCentre + U + V;

            // * METHOD 3: ellipse along any axis  
            Vector3d U = B * Math.Cos(radians);
            Vector3d V = A * Math.Sin(radians); // adding aE doesn't work here - not sure why...
            points[p] = (Vector3) (orbitCentre + U + V);
        }

        satellite.Line.SetPositions(points);
    }

    #endregion Trajectory Methods
}
