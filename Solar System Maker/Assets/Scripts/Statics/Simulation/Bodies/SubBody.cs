// a subcategory of a body containing planets, moons, and other bodies
// * these are different from stars in that they don't have their properties, like Luminosity
// * instead, they have their own properties, like temperature-related variables

using System;
using Newtonsoft.Json;

[Serializable]
public class SubBody : Body
{
    
    #region Attributes

    // * source for temperature stuff: https://www.youtube.com/watch?v=pRs9v_Ze55w
    protected double albedo;
    public double Albedo {
        get => albedo;
        set => albedo = value;
    }

    #endregion Attributes


    #region Constructor

    // creates a new instance of a "sub-body" 
    public SubBody() : base() 
    {
        albedo = 0;
    }

    #endregion Constructor
}