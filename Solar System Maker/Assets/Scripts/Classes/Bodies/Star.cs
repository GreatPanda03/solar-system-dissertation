// Creating instances of Stars
// * all other body types extend from this class

/* REFERENCES:
* [1] - Andrew Raphael Lukasik, "BFN", www.github.com [Online]. Available at: https://github.com/andrew-raphael-lukasik/BFN
*/

using System;
using UnityEngine;

[Serializable]
public class Star : Body
{
    readonly string type = "Star";
    public override string Type { get => type; }

    protected float luminosity;
    public float Luminosity {
        get { return luminosity;  }
        set { luminosity = value; }
    }

    public Star() : base()
    {
        name = "Star";
        mass = 1000f;
        luminosity = 1f;
        radius = GetRadiusFromMass();
    }

    public Star(float mass, Vector3 position, Vector3 velocity)
        : base(mass, position, velocity)
    {
        name = "Star";
        luminosity = 1f;
    }

    public Star(float mass, Vector3 position, Vector3 velocity, float luminosity)
        : base(mass, position, velocity)
    {
        name = "Star";
        this.luminosity = luminosity;
    }

    // returns a copy of the Star
    public override Body GetCopy()
    {
        return new Star()
        {
            name = name,
            mass = mass,
            position = position,
            velocity = velocity,
            acceleration = acceleration,
            surfaceTemp = surfaceTemp,
            density = density,
            radius = radius,

            luminosity = luminosity
        };
    }
}
