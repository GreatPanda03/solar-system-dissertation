// Manages the core Simulator functionality
// * the Simulator acts as a container which runs the physics for all bodies
// * it itself does NOT manage UI directly


// LIBRARIES
using UnityEngine;
using System.Collections.Generic;
using System;


// CLASS DEFINITION
public class SimulatorController : MonoBehaviour
{
    public GameObject simulatorSpace;
    private bool isActive = false;
    public Simulation simulation;

    public static double UNITS_PER_WORLDSPACE = 1;
    public static string currentScale = "Kilometre";
    public static double zoomFactor = 0f;


    #region Simulator Setup

    public void Start()
    {
        SetScale("Kilometre");
    }

    // enables the Simulator
    public void EnableSimulator()
    {
        isActive = true;
    }

    // disables the Simulator
    public void DisableSimulator()
    {
        isActive = false;
    }

    // loads the Simulator with the given simulation
    // * also creates a physical entity for each body in case they don't have one
    public void LoadSimulation(Simulation simulation)
    {
        this.simulation = simulation;

        foreach (Body body in simulation.bodies)
            if (body.Entity == null) body.CreateEntity();
    }

    // clears all bodies in the Simulator
    public void ClearSimulator()
    {
        GameObject container = GameObject.Find("SimulatorSpace");

        for( int c = container.transform.childCount - 1; c >= 0; c--)
        {
            Destroy(container.transform.GetChild(c).gameObject);
        }
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
    }

    #endregion Simulator Methods


    #region Scaling Methods

    public static void SetScale(string scale)
    {
        currentScale = scale;
        zoomFactor = 0;
    }

    public static void SetZoomFactor(double scale)
    {
        zoomFactor = scale;
    }

    public static double GetScaleCoefficient()
    {
        return 1 / UnitReference.DISTANCE_UNITS[currentScale] * Math.Pow(10, zoomFactor) * UNITS_PER_WORLDSPACE;
    }

    public static void AdjustZoomFactor(double change)
    {
        SetZoomFactor(zoomFactor + change);
    }

    public static void ApplyScale(Body body)
    {
        double scale = GetScaleCoefficient();
        body.Entity.transform.localScale = (float) (body.Radius * scale) * new Vector3(1,1,1);
        body.Entity.transform.position = (float) scale * body.Position;
    }

    #endregion Scaling Methods


    #region Physics Methods

    // main physics loop - applies planet forces between each other
    void FixedUpdate()
    {
        if (!isActive) return;

        if (simulation.settings.GravityAlgorithm == "Newtonian Physics")
        {
            ApplyPhysics();
        }
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

                Vector3 force = bodyA.GetForceBetween(bodyB, simulation.settings.GravityConstant);
                bodyA.ApplyForce(force);
                bodyB.ApplyForce(-force);
            }

            bodyA.Update();

            ApplyScale(bodyA);
        }
    }
    

    // algorithm v2: applies forced-based gravity between all bodies
    // * more "extractive" with less calcs - calculations done outside class
    // * WIP...

    /*
    public void ApplyPhysics_V2()
    {
        List<Body> bodies = simulation.bodies;

        for (int a = 0; a < bodies.Count; a++)
        {
            Body bodyA = bodies[a];
            float Gm = simulation.settings.GravityConstant * bodyA.Mass;

            // ...

            for (int b = a + 1; b < bodies.Count; b++)
            {
                Body bodyB = bodies[b];

                Vector3 force = bodyA.GetForceBetween(bodyB, simulation.settings.GravityConstant);
                bodyA.ApplyForce(force);
                bodyB.ApplyForce(-force);
            }

            bodyA.Update();
        }
    }
    */

    #endregion Physics Methods
}