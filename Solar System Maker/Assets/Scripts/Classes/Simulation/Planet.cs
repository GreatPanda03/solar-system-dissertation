// Creating instances of Planets
// * all other body types extend from this class

/* REFERENCES:
* [1] - Andrew Raphael Lukasik, "BFN", www.github.com [Online]. Available at: https://github.com/andrew-raphael-lukasik/BFN
*/

using System;
using UnityEngine;

[Serializable]
public class Planet : Body
{

    #region Attributes

    readonly string type = "Planet";
    public override string Type { get => type; }

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

    public Planet() : base()
    {
        name = "Planet";
        mass = (float) UnitReference.MASS_UNITS["Earth"];
        radius = (float) UnitReference.DISTANCE_UNITS["Earth Radius"];
        habitability = 0;
        waterContent = 0;
        
        density = GetDensityFromMassAndRadius();
    }

    #endregion Constructors


    #region Methods

    public override Body GetShallowCopy()
    {
        return (Planet) MemberwiseClone();
    }

    public override Body GetDeepCopy()
    {
        Planet copy = (Planet) MemberwiseClone();
        copy.entity = entity != null ? UnityEngine.Object.Instantiate(entity) : null;
        copy.label = label != null ? UnityEngine.Object.Instantiate(label) : null; 

        return copy;
    }

    #endregion
}
