// Creating instances of Planets
// * all other body types extend from this class

/* REFERENCES:
* [1] - Andrew Raphael Lukasik, "BFN", www.github.com [Online]. Available at: https://github.com/andrew-raphael-lukasik/BFN
*/

using System;
using UnityEngine;

[Serializable]
public class Planet : SubBody
{

    #region Attributes

    protected float habitability;
    public float Habitability {
        get => habitability;
        set => habitability = value;
    }

    /*
    private Atmosphere Atmosphere {
        get { return atmosphere;  }
        set { atmosphere = value; }
    }
    */

    protected float waterContent;
    public float WaterContent {
        get => waterContent;
        set => waterContent = value;
    }

    #endregion Attributes


    #region Constructors

    // like the earth by default
    public Planet() : base()
    {
        name = "Planet";
        mass = (float) UnitReference.MASS_UNITS["Earth"];
        radius = (float) UnitReference.DISTANCE_UNITS["Earth Radius"];
        angularVelocity = 7.29e-5f;
        
        // source: https://climatepuzzles.org/technical/surface-emissivity/
        albedo = 0.31f;
        emissivity = 0.65f;
        
        habitability = 0;
        waterContent = 0;
        
        density = GetDensityFromMassAndVolume();
    }

    #endregion Constructors
}
