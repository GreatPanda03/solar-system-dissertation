// generates solar systems

using System.Collections.Generic;
using UnityEngine;

public class SystemGenerator
{
    public static List<Body> GenerateMenuSystem(float G) // actual G
    {
        List<Body> bodies = new();
        PhysicsHandler physics = new();

        // star
        Star star = new()
        {
            Name = "Star",
            Position = new Vector3(0,0,0),
        };
        
        bodies.Add(star);

        // planets
        for (int p = 1; p <= Random.Range(3, 10); p++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized 
                * (float) UnitReference.DISTANCE_UNITS["AU"];

            float elevation = (float) UnitReference.DISTANCE_UNITS["Lunar Distance"]
                * Random.Range(-10f, 10f);

            Vector3 position = star.Position 
                + new Vector3(offset.x, 0, offset.y) * Mathf.Pow(p, 1.25f)
                + new Vector3(0, elevation, 0);

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