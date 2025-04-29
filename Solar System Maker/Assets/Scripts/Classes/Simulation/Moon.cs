// class for Moon instances (inherits from Body)

using System;
using UnityEngine;

[Serializable]
public class Moon : Body
{

    #region Attributes

    readonly string type = "Moon";
    public override string Type { get => type; }

    #endregion Attributes


    #region Constructors

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

    #endregion Constructors

    
    #region Methods

    public override Body GetShallowCopy()
    {
        return (Moon) MemberwiseClone();
    }

    public override Body GetDeepCopy()
    {
        Moon copy = (Moon) MemberwiseClone();
        copy.entity = entity != null ? UnityEngine.Object.Instantiate(entity) : null;
        copy.label = label != null ? UnityEngine.Object.Instantiate(label) : null; 

        return copy;
    }

    #endregion Methods
}
