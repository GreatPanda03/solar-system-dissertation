// Creating instances of Bodies
// * all other body types extend from this class

/* REFERENCES:
* [1] - StackOverflow, "Using Getters and Setters in Unity", www.stackoverflow.com [Online]. Available at: https://stackoverflow.com/questions/23649784/using-getters-and-setters-in-unity.
* [2] - W3Schools, "C# Properties (Get and Set)", www.w3schools.com [Online]. Available at: https://www.w3schools.com/cs/cs_properties.php#:~:text=It%20is%20a%20good%20practice,we%20assign%20to%20the%20property.
* [3] - Daniel Shiffman, "2.6 Mutual Attraction - The Nature of Code", The Coding Train, www.youtube.com [Online]. Available at: https://www.youtube.com/watch?v=GjbKsOkN1Oc
* [4] - Andrew Raphael Lukasik, "BFN", www.github.com [Online]. Available at: https://github.com/andrew-raphael-lukasik/BFN
*/

using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class Body
{

    #region Attributes

    protected string name;
    public virtual string Name {
        get => name;
        set => name = value;
    }

    private Material texture;
    public Material Texture {
        get => texture;
        set => texture = value;
    }

    protected bool anchored = false;
    public virtual bool Anchored {
        get => anchored;
        set {
            anchored = value;

            if (anchored)
            {
                acceleration = Vector3.zero;
                velocity = Vector3.zero;
            }
        }
    }

    protected float mass;
    public virtual float Mass {
        get => mass;
        set {
            mass = value;
            density = GetDensityFromMassAndVolume();
        }
    }

    protected float radius;
    public virtual float Radius {
        get => radius;
        set { 
            radius = value;
            density = GetDensityFromMassAndVolume();
        }
    }

    protected Vector3 position;
    public virtual Vector3 Position {
        get => position; 
        set => position = value; 
    }

    protected Vector3 velocity;
    public virtual Vector3 Velocity {
        get => velocity; 
        set => velocity = value; 
    }

    protected Vector3 acceleration;
    [JsonIgnore] // 0 between frames
    public virtual Vector3 Acceleration {
        get => acceleration; 
        set => acceleration = value; 
    }

    protected float surfaceTemp;
    public virtual float SurfaceTemp {
        get => surfaceTemp; 
        set => surfaceTemp = value; 
    }

    protected float density;
    public virtual float Density {
        get => density; 
        set {
            density = value;
            radius = GetRadiusFromMassAndDensity();
        }
    }

    protected Vector3 axis; // axis of rotation, relative to plane Y = 0
    public virtual Vector3 Axis {
        get => axis;
        set => axis = value;
    }

    protected float rotation; // radians around axis
    public virtual float Rotation {
        get => rotation;
        set => rotation = value;
    }

    protected float angularVelocity; // measured in radians/s
    public virtual float AngularVelocity {
        get => angularVelocity;
        set {
            angularVelocity = value;
        }
    }

    #endregion Attributes


    #region Constructor

    public Body()
    {
        name = "Body";
        mass = 10f;
        position = new Vector3(0,0,0);
        velocity = new Vector3(0,0,0);
        acceleration = new Vector3(0,0,0);
        surfaceTemp = 0f;
        density = 1f;
        angularVelocity = 1 * Mathf.Deg2Rad;
        axis = Vector3.up;

        radius = GetRadiusFromMassAndDensity();
    }

    #endregion Constructor


    #region Object Methods

    public virtual Body GetCopy()
    {
        return (Body) MemberwiseClone();
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(
            this, 
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            }
        );
    }

    #endregion Object Methods


    #region Metric Methods

    public float GetVolume()
    {
        return 4f / 3f * Mathf.PI * Mathf.Pow(radius, 3);
    }

    // auxiliary: returns values in relation to "p = m / v"
    public float GetRadiusFromMassAndDensity()
    {
        return Mathf.Pow(mass / density * 0.75f / Mathf.PI, 1f / 3f);
    }

    public float GetDensityFromMassAndVolume()
    {
        return mass / GetVolume();
    }

    #endregion Metric Methods
}
