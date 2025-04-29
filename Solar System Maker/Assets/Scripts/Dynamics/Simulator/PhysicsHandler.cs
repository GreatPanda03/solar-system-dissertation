// manages the physics operations

using System;
using UnityEngine;


public class PhysicsHandler
{

    #region Attributes
    public SimulationController simController;

    public enum CollisionQuality { None, Low, High }
    public CollisionQuality collisionQuality = CollisionQuality.Low;
    
    private string timePerSecond;
    public string TimePerSecond {
        get => timePerSecond;
        set {
            timePerSecond = value;
            timeCoefficient = 0.02f * (float) UnitReference.TIME_UNITS[timePerSecond];
        }
    }

    private float timeCoefficient;
    public float TimeCoefficient {
        get => timeCoefficient;
        set => timeCoefficient = value;
    }

    #endregion Attributes


    #region Constructor

    public PhysicsHandler() {}

    public PhysicsHandler(SimulationController simController)
    {
        this.simController = simController;
        timePerSecond = "1 Second";
    }

    #endregion Constructor


    #region General Methods

    public void SetCollisionQuality(string newQuality)
    {
        if (Enum.TryParse(newQuality, out CollisionQuality quality ))
        {   
            collisionQuality = quality;
        }
    }

    #endregion


    #region Physics Methods
    
    // returns the force applied between two objects
    // * F1 = F2 = G * (m1 * m2) / r^2
    // * NOTE: the force's magnitude is the same, but the direction is different
    //   > this method returns an attractive force by THIS on OTHER
    public Vector3 GetGravitationalForce(Body A, Body B, float G) // actual G
    {
        Vector3 vector = B.Position - A.Position;
        Vector3 direction = vector.normalized;
        float distance = Mathf.Max(vector.magnitude, A.Radius + B.Radius);
        float magnitude = G * (A.Mass * B.Mass) / (distance * distance);

        Vector3 force = magnitude * direction;

        return force;
    }

    // returns the velocity required for the given child body to have a stable orbit around the given parent body
    // * velocity is based on the true positions of each body, which is then scaled accordingly later

    // source: 
    public Vector3 GetStableOrbitVelocity(
        Body parent, 
        Body child, 
        float G, 
        float eccentricity = 0,
        float orbitDirection = 1)
    {
        Vector3 centre = parent.Position;
        Vector3 edge = child.Position;
        float r = Vector3.Distance(centre, edge);

        Vector3 gravityDirection = (edge - centre).normalized;
        Vector3 velocityDirection = Vector3.Cross(gravityDirection, Vector3.up * -orbitDirection);

        // * METHOD 1: fixed radius (constant velocity)
        // * formula: v = root(G * M * r)
        // float M = parent.Mass;
        // Vector3 velocity = parent.Velocity + (velocityDirection * Mathf.Sqrt(G * M / r));


        // * METHOD 2: eccentric orbit (changing velocity)
        // * calculates the velocity at the periapsis - same idea as in RenderingHandler
        // * formula: v_per = root( (1 + e) x mu / (1 - e) / a )
        float a = r / (1 - eccentricity);
        float numerator = (1 + eccentricity) * G * (parent.Mass + child.Mass);
        float denominator = (1 - eccentricity) * a;

        Vector3 velocity = parent.Velocity 
            + (velocityDirection * Mathf.Sqrt(numerator / denominator));

        return velocity;
    }

    // processes a collision between the two bodies
    public void ProcessCollision(Body bodyA, Body bodyB)
    {
        if (collisionQuality == CollisionQuality.None)
        {
            return;
        }
        else if (collisionQuality == CollisionQuality.Low)
        {
            Debug.Log("[low collision quality processing method]");
        }
        else if (collisionQuality == CollisionQuality.High)
        {
            Debug.Log("[high collision quality processing method]");
        }
    }

    #endregion Physics Methods
}