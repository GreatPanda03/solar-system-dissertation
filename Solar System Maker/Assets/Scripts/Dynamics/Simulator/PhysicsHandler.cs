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
            timeCoefficient = Time.fixedDeltaTime * (float) UnitReference.TIME_UNITS[timePerSecond];
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


    #region General

    public void SetCollisionQuality(string newQuality)
    {
        if (Enum.TryParse(newQuality, out CollisionQuality quality ))
        {   
            collisionQuality = quality;
        }
    }

    // updates the given body's physics over the given timeframe
    public void Update(Body body, float dt)
    {
        if (body.Entity != null)
        {
            body.Entity.transform.rotation // will automatically rotate according to tilt
                *= Quaternion.Euler(0, -body.AngularVelocity * Mathf.Rad2Deg * dt, 0);
        }

        body.Velocity += body.Acceleration * dt;
        body.Position += body.Velocity * dt;
        body.Acceleration = Vector3.zero;

        // special case for sub-bodies
        if (body is SubBody subBody)
        {
            float powerAbsorbed = subBody.PowerAbsorbed;
            float powerRadiated = GetPowerRadiated(subBody);

            subBody.SurfaceTemp = Mathf.Pow(powerAbsorbed - powerRadiated, 0.25f)
                - (float) UnitReference.TEMPERATURE_UNITS["Celsius"];

            subBody.PowerAbsorbed = 0;
        }
    }

    #endregion General


    #region Forces
    
    // returns the gravitational force exerted by A on B (so B moves towards A)
    // * F1 = F2 = G * (m1 * m2) / r^2
    // * NOTE: the force's magnitude is the same, but the direction is important
    public Vector3 GetGravitationalForce(Body A, Body B, float G) // actual G
    {
        Vector3 vector = B.Position - A.Position;
        Vector3 direction = vector.normalized;
        float distance = Mathf.Max(vector.magnitude, A.Radius + B.Radius);
        float magnitude = G * (A.Mass * B.Mass) / (distance * distance);

        Vector3 force = magnitude * direction;

        return force;
    }

    // applies the given force to the given body
    // * F = m * a   :.   a = F / m
    public void ApplyForce(Body body, Vector3 force)
    {
        body.Acceleration += force / body.Mass;
    }

    #endregion Forces


    #region Velocity

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

    #endregion Velocity


    #region Momentum

    // returns the momentum of the given body
    public Vector3 GetMomentum(Body body)
    {
        return body.Mass * body.Velocity;
    }

    // returns the angular momentum of the given body
    public Vector3 GetAngularMomentum(Body body)
    {
        return body.Radius * body.AngularVelocity * body.Tilt;
    }

    #endregion Momentum


    #region Collisions

    // returns the larger of the two bodies colliding, if any collision
    public Body IsColliding(Body bodyA, Body bodyB)
    {
        if (collisionQuality == CollisionQuality.None)
        {
            return null;
        }
        else
        {
            float distance = (bodyA.Position - bodyB.Position).magnitude;
            float tolerance = 0.75f;
            // the threshold for how deep the "minor" body must be
            //   to trigger a colllsion (0 = touching, 1 = completely in)

            Body majorBody;
            Body minorBody;

            if (bodyA.Radius > bodyB.Radius)
            {
                majorBody = bodyA;
                minorBody = bodyB;
            }
            else
            {
                majorBody = bodyB;
                minorBody = bodyA;
            }

            bool colliding = distance < majorBody.Radius + minorBody.Radius * (1 - tolerance);

            return colliding ? majorBody : null;
        }
    }

    // applies the forces of Body B onto Body A as if they were colliding
    public void ApplyCollision(Body bodyA, Body bodyB)
    {
        float totalMass = bodyA.Mass + bodyB.Mass;
        float totalVolume = bodyA.GetVolume() + bodyB.GetVolume();
        Vector3 totalMomentum = GetMomentum(bodyA) + GetMomentum(bodyB);
        Vector3 totalAngularMomentum = GetAngularMomentum(bodyA) - GetAngularMomentum(bodyB);
        
        // ^ subtraction - if A and B are rotating in the same direction,
        //   they'd be opposing each other when they touch (and the opposite if in opposite directions)
        
        bodyA.Mass = totalMass;
        bodyA.Radius = Mathf.Pow(0.75f * totalVolume / Mathf.PI, 1f / 3f);
        if (!bodyA.Anchored) ApplyForce(bodyA, totalMomentum / totalMass);
        bodyA.AngularVelocity = totalAngularMomentum.magnitude / (bodyA.Radius + bodyB.Radius);

        // ^ "instantaneous acceleration" will be processed as velocity when "velocity += acceleration"
    }

    #endregion Collisions


    #region Temperature

    // returns the power absorbed by the given sub body from the given star
    //   based on luminosity, planet radius, distance, and albedo
    public float GetPowerReceived(Star star, SubBody body)
    {
        float r = body.Radius;
        float D = (star.Position - body.Position).magnitude;
        
        float numerator = star.Luminosity * (r*r); // albedo not considered here - see "Apply Power"
        float denominator = 4 * D*D;

        float powerReceived = numerator / denominator;

        return powerReceived;
    }

    // returns the power being emitted by the given body based on 
    //   its current temperature and emissivity
    public float GetPowerRadiated(SubBody body)
    {
        float r = body.Radius;

        float power = body.Emissivity * (float) UnitReference.STEFAN_BOLTZMANN 
            * Mathf.Pow(body.SurfaceTemp + (float) UnitReference.TEMPERATURE_UNITS["Kelvin"], 4);

        float surfaceArea = 4 * Mathf.PI * (r*r);
        
        float powerRadiated = power * surfaceArea;
        powerRadiated = powerRadiated == float.NaN ? float.MaxValue : powerRadiated;

        return powerRadiated;
    }

    // applies the given power directly to the given sub-body
    public void ApplyPower(SubBody body, float power)
    {
        body.PowerAbsorbed += power * (1 - body.Albedo);
    }

    #endregion Temperature
}