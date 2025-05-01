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

    protected double luminosity;
    public double Luminosity {
        get => luminosity;
        set => luminosity = value;
    }

    // mass is already a property of Body - this scales luminosity with mass instead
    public override double Mass {
        get => mass;
        set {
            base.Mass = mass; // use body's "Mass" property

            luminosity = CalculateLuminosity(mass);
            surfaceTemp = CalculateSurfaceTemperature(radius, luminosity);
        }
    }

    // ^ ditto for surface temperature
    public override double SurfaceTemp {
        get => surfaceTemp;
        set => surfaceTemp = value;
    }

    #endregion Attributes


    #region Constructors

    // like the sun by default
    public Star() : base()
    {
        name = "Star";
        mass = (double) UnitReference.MASS_UNITS["Sun"];
        radius = (double) UnitReference.DISTANCE_UNITS["Solar Radius"];
        surfaceTemp = CalculateSurfaceTemperature(radius, luminosity);
        angularVelocity = 2.7e-6f;

        luminosity = (double) UnitReference.LUMINOSITY_UNITS["Sun"];

        density = GetDensityFromMassAndVolume();
    }

    #endregion Constructors


    #region Entity Methods

    // class-wide method to return star luminosity by mass
    // * source: https://en.wikipedia.org/wiki/Mass%E2%80%93luminosity_relation
    public static double CalculateLuminosity(double mass)
    {
        double luminosity;

        double SUN_MASS = (double) UnitReference.MASS_UNITS["Sun"];
        double SUN_LUMINOSITY = (double) UnitReference.LUMINOSITY_UNITS["Sun"];

        if (mass < 0.43 * SUN_MASS)
        {
            luminosity = 0.23f * Math.Pow(SUN_MASS / mass, 2.3f) * SUN_LUMINOSITY;
        }
        else if (mass < 2 * SUN_MASS)
        {
            luminosity = Math.Pow(mass / SUN_MASS, 4) * SUN_LUMINOSITY;
        }
        else if (mass < 55 * SUN_MASS)
        {
            luminosity = 1.4f * Math.Pow(mass / SUN_MASS, 3.5f) * SUN_LUMINOSITY;
        }
        else
        {
            luminosity = 32_000 * mass / SUN_MASS;
        }

        return luminosity;
    }

    // class-wide method to return star surface temperature from luminosity amd radius
    // * source: https://en.wikipedia.org/wiki/Effective_temperature
    public static double CalculateSurfaceTemperature(double radius, double luminosity)
    {
        double temperature = Math.Pow(
            luminosity / (4 * Math.PI) / Math.Pow(radius, 2) / (double) UnitReference.STEFAN_BOLTZMANN, 
            0.25f);

        return temperature;
    }

    #endregion Entity Methods
}
