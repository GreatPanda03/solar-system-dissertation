// class for Moon instances (inherits from Body)

using System;

[Serializable]
public class Moon : SubBody
{

    #region Attributes

    // ... no additional attributes

    #endregion Attributes


    #region Constructors

    // like the moon by default
    public Moon() : base()
    {
        name = "Moon";
        mass = (double) UnitReference.MASS_UNITS["Moon"];
        radius = 1.74e6f;
        angularVelocity = 2.661_7e-6f;

        // source: https://www.researchgate.net/figure/a-Emissivity-spectra-of-lunar-surface-samples-and-spectral-filters-of-the-three-bands_fig1_346092026
        albedo = 0.11f;

        density = GetDensityFromMassAndVolume();
    }

    #endregion Constructors
}
