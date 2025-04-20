
using UnityEngine;

public static class PathDrawer
{
    private static SimulatorController simController 
        = GameObject.Find("Simulator").GetComponent<SimulatorController>();

    // draws a trajectory for the given body based on the given vector
    public static void DrawTrajectory(Body body, Vector3 trajectory)
    {
        Vector3 origin = body.Entity.transform.position;

    	LineRenderer pathDrawer = body.Entity.GetComponent<LineRenderer>();
	    pathDrawer.positionCount = 2;

        pathDrawer.SetPosition(0, origin);
        pathDrawer.SetPosition(1, origin + trajectory);
    }

    // draws the orbit of one body around another
    public static void DrawOrbitBetweenBodies(Body parent, Body child, Vector3 childVelocity)
    {
        Vector3 centre = parent.Position;
        Vector3 edge = child.Position;

        float G = simController.simulation.settings.GravityConstant;
        float M = parent.Mass;

        int resolution = 100;
        float step = 1f; // time step
        Vector3[] points = new Vector3[resolution];

        LineRenderer pathDrawer = child.Entity.GetComponent<LineRenderer>();
	    pathDrawer.positionCount = resolution;

        double scale = SimulatorController.GetScaleCoefficient();

        for (int p = 0; p < resolution; p++)
        {
            points[p] = edge * (float) scale;

            // same formula used to calculate forces between bodies!
            Vector3 direction = (centre - edge).normalized;
            float rSquared = (centre - edge).sqrMagnitude;
            Vector3 acceleration = direction * (G * M / rSquared);

            childVelocity += acceleration * step;
            edge += childVelocity / (float) scale * step;
        }

        pathDrawer.SetPositions(points);
    }
}
