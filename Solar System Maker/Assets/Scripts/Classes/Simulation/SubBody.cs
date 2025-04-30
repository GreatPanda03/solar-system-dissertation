// a subcategory of a body containing planets, moons, and other bodies
// * these are different from stars in that they don't have their properties, like Luminosity
// * instead, they have their own properties, like temperature-related variables

public class SubBody : Body
{
    
    #region Attributes

    // * source for temperature stuff: https://www.youtube.com/watch?v=pRs9v_Ze55w
    protected float albedo;
    public float Albedo {
        get => albedo;
        set => albedo = value;
    }

    protected float emissivity;
    public float Emissivity {
        get => emissivity;
        set => emissivity = value;
    }

    protected float powerAbsorbed; // used similarly to velocity to determine position
    public float PowerAbsorbed {
        get => powerAbsorbed;
        set => powerAbsorbed = value;
    }

    #endregion Attributes


    #region Constructor

    // creates a new instance of a "sub-body" 
    public SubBody() : base() 
    {
        albedo = 0;
        emissivity = 0.5f;
    }

    #endregion Constructor


    #region Object Methods

    public override Body GetShallowCopy()
    {
        return (SubBody) MemberwiseClone();
    }

    public override Body GetDeepCopy()
    {
        SubBody copy = (SubBody) MemberwiseClone();
        copy.entity = entity != null ? UnityEngine.Object.Instantiate(entity) : null;
        copy.label = label != null ? UnityEngine.Object.Instantiate(label) : null; 

        return copy;
    }

    #endregion Object Methods
}