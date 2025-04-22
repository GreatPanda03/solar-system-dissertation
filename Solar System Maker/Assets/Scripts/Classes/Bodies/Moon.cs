// class for Moon instances (inherits from Body)

using System;
using UnityEngine;

[Serializable]
public class Moon : Body
{
    readonly string type = "Moon";
    public override string Type { get => type; }

    public Moon() : base()
    {
        name = "Moon";
        mass = 1f;
        radius = GetRadiusFromMassAndDensity();
    }

    public Moon(float mass, Vector3 position, Vector3 velocity)
        : base(mass, position, velocity)
    {
        name = "Moon";
    }

    public override Body GetCopy()
    {
        return new Moon()
        {
            name = name,
            mass = mass,
            position = position,
            velocity = velocity,
            acceleration = acceleration,
            surfaceTemp = surfaceTemp,
            density = density,
            radius = radius,
        };
    }
}
