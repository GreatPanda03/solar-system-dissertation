
using UnityEngine;

public static class PathDrawer
{
    private static SimulatorController simController 
        = GameObject.Find("Simulator").GetComponent<SimulatorController>();

    // draws a trajectory for the given body based on the given vector
    public static void DrawTrajectory(Body body, Vector3 trajectory)
    {
        double scale = SimulatorController.GetScaleCoefficient();
        Vector3 scaledOrigin = body.Position * (float) scale;
        Vector3 scaledTrajectory = trajectory * (float) scale;

    	LineRenderer pathDrawer = body.Entity.GetComponent<LineRenderer>();
	    pathDrawer.positionCount = 2;

        pathDrawer.SetPosition(0, scaledOrigin);
        pathDrawer.SetPosition(1, scaledOrigin + scaledTrajectory);
    }

    // draws the orbit of one body around another
    public static void DrawOrbitBetweenBodies(Body parent, Body child, Vector3 childVelocity)
    {
        Vector3 centre = parent.Position;
        Vector3 edge = child.Position;

        float G = simController.simulation.settings.GravityConstant;
        float M = parent.Mass;

        int resolution = 100;
        // float step = 1f; // time step
        Vector3[] points = new Vector3[resolution + 1];

        LineRenderer pathDrawer = child.Entity.GetComponent<LineRenderer>();
	    pathDrawer.positionCount = resolution;

        double scale = SimulatorController.GetScaleCoefficient();

        Vector3 scaledCentre = (float) scale * centre;
        Vector3 scaledEdge = (float) scale * edge;
        Vector3 direction = scaledCentre - scaledEdge;
        float scaledRadius = direction.magnitude;
        float startingAngle = Vector3.SignedAngle(direction, -Vector3.right, Vector3.up) * Mathf.Deg2Rad;

        for (int p = 0; p <= resolution; p++)
        {
            /*
            points[p] = (float) scale * edge;

            // same formula used to calculate forces between bodies!
            Vector3 direction = (centre - edge).normalized;
            float rSquared = (centre - edge).sqrMagnitude;
            Vector3 acceleration = direction * (G * M / rSquared);

            childVelocity += (float) scale * step * acceleration;
            edge += (float) scale * step * childVelocity;
            */

            float progress = (float) p / resolution;
            float radians = startingAngle + (progress * 2 * Mathf.PI);

            float x = Mathf.Cos(radians) * scaledRadius;
            float z = Mathf.Sin(radians) * scaledRadius;

            points[p] = scaledCentre + new Vector3(x, 0, z);
        }

        pathDrawer.SetPositions(points);
    }
}
