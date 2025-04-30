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

    private readonly string type = "Star";
    public override string Type { get => type; }

    protected float luminosity;
    public float Luminosity {
        get => luminosity;
        set => luminosity = value;
    }

    // mass is already a property of Body - this scales luminosity with mass instead
    public override float Mass {
        get => mass;
        set {
            base.Mass = mass; // use body's "Mass" property

            luminosity = CalculateLuminosity(mass);
            surfaceTemp = CalculateSurfaceTemperature(radius, luminosity);
        }
    }

    // ^ ditto for surface temperature
    public override float SurfaceTemp {
        get => surfaceTemp;
        set => surfaceTemp = value;
    }

    #endregion Attributes


    #region Constructors

    // like the sun by default
    public Star() : base()
    {
        name = "Star";
        mass = (float) UnitReference.MASS_UNITS["Sun"];
        radius = (float) UnitReference.DISTANCE_UNITS["Solar Radius"];
        surfaceTemp = CalculateSurfaceTemperature(radius, luminosity);
        angularVelocity = 2.7e-6f;

        luminosity = (float) UnitReference.LUMINOSITY_UNITS["Sun"];

        density = GetDensityFromMassAndVolume();
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


    #region Entity Methods

        // returns a new generic Body entity
    public override void CreateEntity() {
        base.CreateEntity();

        GameObject lightPrefab = Resources.Load<GameObject>("Prefabs/Light");
        
        GameObject light = GameObject.Instantiate(lightPrefab);
        light.name = "Light";
        light.transform.SetParent(entity.transform, false);
    }

    // class-wide method to return star luminosity by mass
    // * source: https://en.wikipedia.org/wiki/Mass%E2%80%93luminosity_relation
    public static float CalculateLuminosity(float mass)
    {
        float luminosity;

        float SUN_MASS = (float) UnitReference.MASS_UNITS["Sun"];
        float SUN_LUMINOSITY = (float) UnitReference.LUMINOSITY_UNITS["Sun"];

        if (mass < 0.43 * SUN_MASS)
        {
            luminosity = 0.23f * Mathf.Pow(SUN_MASS / mass, 2.3f) * SUN_LUMINOSITY;
        }
        else if (mass < 2 * SUN_MASS)
        {
            luminosity = Mathf.Pow(mass / SUN_MASS, 4) * SUN_LUMINOSITY;
        }
        else if (mass < 55 * SUN_MASS)
        {
            luminosity = 1.4f * Mathf.Pow(mass / SUN_MASS, 3.5f) * SUN_LUMINOSITY;
        }
        else
        {
            luminosity = 32_000 * mass / SUN_MASS;
        }

        return luminosity;
    }

    // class-wide method to return star surface temperature from luminosity amd radius
    // * source: https://en.wikipedia.org/wiki/Effective_temperature
    public static float CalculateSurfaceTemperature(float radius, float luminosity)
    {
        float temperature = Mathf.Pow(
            luminosity / (4 * Mathf.PI) / Mathf.Pow(radius, 2) / (float) UnitReference.STEFAN_BOLTZMANN, 
            0.25f);

        return temperature;
    }

    #endregion Entity Methods
}
