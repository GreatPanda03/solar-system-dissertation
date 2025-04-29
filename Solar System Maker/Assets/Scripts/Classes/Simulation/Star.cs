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

    #region Attributes

    readonly string type = "Star";
    public override string Type { get => type; }

    protected float luminosity;
    public float Luminosity {
        get => luminosity;
        set => luminosity = value;
    }

    #endregion Attributes


    #region Constructors

    public Star() : base()
    {
        name = "Star";
        mass = 1000f;
        luminosity = 1f;
        radius = GetRadiusFromMassAndDensity();
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

    #endregion Constructors


    #region Methods

    public override Body GetShallowCopy()
    {
        return (Star) MemberwiseClone();
    }

    public override Body GetDeepCopy()
    {
        Star copy = (Star) MemberwiseClone();
        copy.entity = entity != null ? UnityEngine.Object.Instantiate(entity) : null;
        copy.label = label != null ? UnityEngine.Object.Instantiate(label) : null; 

        return copy;
    }

    #endregion Methods
}
