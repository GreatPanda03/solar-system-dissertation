// generates solar systems

using System.Collections.Generic;
using UnityEngine;

public class SystemGenerator
{
    public static List<Body> GenerateMenuSystem(double G) // actual G
    {
        List<Body> bodies = new();
        PhysicsHandler physics = new();

        // star
        Star star = new()
        {
            Name = "Star",
            Position = new Vector3d(0,0,0),
        };
        
        bodies.Add(star);

        // planets
        for (int p = 1; p <= Random.Range(3, 10); p++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized 
                * (float) UnitReference.DISTANCE_UNITS["AU"];

            double elevation = (double) UnitReference.DISTANCE_UNITS["AU"]
                * Random.Range(-0.1f, 0.1f);

            Vector3d position = star.Position 
                + new Vector3d(offset.x, 0, offset.y) * System.Math.Pow(p, 1.25f)
                + new Vector3d(0, elevation, 0);

            Planet planet = new()
            {
                Name = "Planet " + p.ToString(),
                Position = position,
            };

            planet.Velocity = physics.GetStableOrbitVelocity(star, planet, G);

            bodies.Add(planet);
        }

        return bodies;
    }
}