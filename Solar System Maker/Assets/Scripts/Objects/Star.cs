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
    [SerializeField]
    protected float luminosity;
    public float Luminosity {
        get { return luminosity;  }
        set { luminosity = value; }
    }

    public Star()
    {
        type = "Star";
        name = "Star";
        mass = 1000f;
        luminosity = 1f;
        radius = GetRadiusFromMass();
    }

    public Star(float mass, Vector3 position, Vector3 velocity)
        : base(mass, position, velocity)
    {
        type = "Star";
        name = "Star";
        luminosity = 1f;
    }

    public Star(float mass, Vector3 position, Vector3 velocity, float luminosity)
        : base(mass, position, velocity)
    {
        type = "Star";
        name = "Star";
        this.luminosity = luminosity;
    }
}
