// handles physics operations
// * this class is standalone and can be used to calculate / apply operations
//     given the appropriate bodies, values, etc.

using System;
using UnityEngine;


public class PhysicsHandler
{

    #region Attributes

    public enum CollisionQuality { None, Low, High }
    public CollisionQuality collisionQuality = CollisionQuality.Low;
    
    private string timePerSecond;
    public string TimePerSecond {
        get => timePerSecond;
        set {
            timePerSecond = value;
            timeCoefficient = Time.fixedDeltaTime * UnitReference.TIME_UNITS[timePerSecond];
        }
    }

    private double timeCoefficient;
    public double TimeCoefficient {
        get => timeCoefficient;
        set => timeCoefficient = value;
    }

    #endregion Attributes


    #region Constructor

    public PhysicsHandler() {
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
    public void Update(Body body, double dt)
    {
        body.Rotation = (body.Rotation - body.AngularVelocity * dt) % (2 * Math.PI);
        body.Velocity += body.Acceleration * dt;
        body.Position += body.Velocity * dt;
        body.Acceleration = Vector3d.zero;
    }

    #endregion General


    #region Forces
    
    // returns the gravitational force exerted by A on B (so B moves towards A)
    // * F1 = F2 = G * (m1 * m2) / r^2
    // * NOTE: the force's magnitude is the same, but the direction is important
    public Vector3d GetGravitationalForce(Body A, Body B, double G) // actual G
    {
        Vector3d vector = B.Position - A.Position;
        Vector3d direction = vector.normalized;
        double distance = Math.Max(vector.magnitude, A.Radius + B.Radius);
        double magnitude = G * (A.Mass * B.Mass) / (distance * distance);

        Vector3d force = magnitude * direction;

        return force;
    }

    // applies the given force to the given body
    // * F = m * a   :.   a = F / m
    public void ApplyForce(Body body, Vector3d force)
    {
        body.Acceleration += force / body.Mass;
    }

    #endregion Forces


    #region Velocity

    // returns the velocity required for the given child body to have a stable orbit around the given parent body
    // * velocity is based on the true positions of each body, which is then scaled accordingly later

    // source: 
    public Vector3d GetStableOrbitVelocity(
        Body parent, 
        Body child, 
        double G, 
        double eccentricity = 0,
        double orbitDirection = 1)
    {
        Vector3d centre = parent.Position;
        Vector3d edge = child.Position;
        double r = Vector3d.Distance(centre, edge);

        Vector3d gravityDirection = (edge - centre).normalized;
        Vector3d velocityDirection = Vector3d.Cross(gravityDirection, Vector3d.up * -orbitDirection);

        // * METHOD 1: fixed radius (constant velocity)
        // * formula: v = root(G * M * r)
        // double M = parent.Mass;
        // Vector3d velocity = parent.Velocity + (velocityDirection * Math.Sqrt(G * M / r));


        // * METHOD 2: eccentric orbit (changing velocity)
        // * calculates the velocity at the periapsis - same idea as in RenderingHandler
        // * formula: v_per = root( (1 + e) x mu / (1 - e) / a )
        double a = r / (1 - eccentricity);
        double numerator = (1 + eccentricity) * G * (parent.Mass + child.Mass);
        double denominator = (1 - eccentricity) * a;

        Vector3d velocity = parent.Velocity 
            + (velocityDirection * Math.Sqrt(numerator / denominator));

        return velocity;
    }

    #endregion Velocity


    #region Momentum

    // returns the momentum of the given body
    public Vector3d GetMomentum(Body body)
    {
        return body.Mass * body.Velocity;
    }

    // returns the angular momentum of the given body
    public Vector3d GetAngularMomentum(Body body)
    {
        return (Vector3d) (body.Tilt * Quaternion.Euler(0, (float) -body.Rotation, 0) 
            * new Vector3(0, (float) (body.Radius * body.AngularVelocity), 0));
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
            double distance = (bodyA.Position - bodyB.Position).magnitude;
            double tolerance = 0.75f;
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
        double totalMass = bodyA.Mass + bodyB.Mass;
        double totalVolume = bodyA.GetVolume() + bodyB.GetVolume();
        Vector3d totalMomentum = GetMomentum(bodyA) + GetMomentum(bodyB);
        Vector3d totalAngularMomentum = GetAngularMomentum(bodyA) - GetAngularMomentum(bodyB);
        
        // ^ subtraction - if A and B are rotating in the same direction,
        //   they'd be opposing each other when they touch (and the opposite if in opposite directions)
        
        bodyA.Mass = totalMass;
        bodyA.Radius = Math.Pow(0.75f * totalVolume / Math.PI, 1f / 3f);
        if (!bodyA.Anchored) ApplyForce(bodyA, totalMomentum / totalMass);
        bodyA.AngularVelocity = totalAngularMomentum.magnitude / (bodyA.Radius + bodyB.Radius);

        // ^ "instantaneous acceleration" will be processed as velocity when "velocity += acceleration"
    }

    #endregion Collisions


    #region Temperature

    // returns the flux of the given star against the given body
    public double GetFlux(Star star, Body body)
    {
        double d = (star.Position - body.Position).magnitude;
        return star.Luminosity / (4 * Math.PI * d*d);
    }

    // returns the effective temperature of the given body the incoming flux
    public double GetEffectiveTemperature(SubBody body, double flux)
    {
        double numerator = flux * (1 - body.Albedo);
        double denominator = 4 * UnitReference.STEFAN_BOLTZMANN;
        return Math.Pow(numerator / denominator, 0.25);
    }

    #endregion Temperature
}