// class for Moon instances (inherits from Body)

using System;
using UnityEngine;

[Serializable]
public class Moon : SubBody
{

    #region Attributes

    private readonly string type = "Moon";
    public override string Type { get => type; }

    #endregion Attributes


    #region Constructors

    // like the moon by default
    public Moon() : base()
    {
        name = "Moon";
        mass = (float) UnitReference.MASS_UNITS["Moon"];
        radius = 1.74e6f;
        angularVelocity = 2.661_7e-6f;

        // source: https://www.researchgate.net/figure/a-Emissivity-spectra-of-lunar-surface-samples-and-spectral-filters-of-the-three-bands_fig1_346092026
        albedo = 0.11f;
        emissivity = 0.97f;

        density = GetDensityFromMassAndVolume();
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
