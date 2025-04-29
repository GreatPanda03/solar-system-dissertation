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

    #endregion Attributes


    #region Constructors

    public Star() : base()
    {
        name = "Star";
        mass = (float) UnitReference.MASS_UNITS["Sun"];
        radius = (float) UnitReference.DISTANCE_UNITS["Sun Radius"];
        luminosity = 1f;

        density = GetDensityFromMassAndRadius();
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


    #endregion Entity Methods
}
