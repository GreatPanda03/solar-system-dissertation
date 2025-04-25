// generates solar systems

using System.Collections.Generic;
using UnityEngine;

public class SystemGenerator
{
    public static List<Body> GenerateMenuSystem(float G)
    {
        List<Body> bodies = new();
        PhysicsHandler physics = new();

        // star
        Star star = new()
        {
            Name = "Star",
            Mass = Random.Range(1_000f, 2_000f),
            Position = new Vector3(0,0,0),
            Density = Random.Range(10f, 20f)
        };

        bodies.Add(star);

        // planets
        for (int p = 1; p <= Random.Range(3, 10); p++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * 50f;
            Vector3 position = star.Position 
                + new Vector3(offset.x, 0, offset.y) * Mathf.Pow(p, 1.25f)
                + new Vector3(0, Random.Range(-10f, 10f), 0);

            Planet planet = new()
            {
                Name = "Planet " + p.ToString(),
                Mass = Random.Range(5, 10f),
                Position = position,
                Density = Random.Range(1f, 2f)
            };

            planet.Velocity = physics.GetStableOrbitVelocity(star, planet, G);

            bodies.Add(planet);
        }

        return bodies;
    }
}