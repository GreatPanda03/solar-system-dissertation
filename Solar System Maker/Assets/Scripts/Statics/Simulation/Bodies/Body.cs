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

    private string texture; // the path of the material to use, if any
    public string Texture {
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
                acceleration = Vector3d.zero;
                velocity = Vector3d.zero;
            }
        }
    }

    protected double mass;
    public virtual double Mass {
        get => mass;
        set {
            mass = value;
            density = GetDensityFromMassAndVolume();
        }
    }

    protected double radius;
    public virtual double Radius {
        get => radius;
        set { 
            radius = value;
            density = GetDensityFromMassAndVolume();
        }
    }

    protected Vector3d position;
    public virtual Vector3d Position {
        get => position; 
        set => position = value; 
    }

    protected Vector3d velocity;
    public virtual Vector3d Velocity {
        get => velocity; 
        set => velocity = value; 
    }

    protected Vector3d acceleration;
    [JsonIgnore] // 0 between frames
    public virtual Vector3d Acceleration {
        get => acceleration; 
        set => acceleration = value; 
    }

    protected double surfaceTemp;
    public virtual double SurfaceTemp {
        get => surfaceTemp; 
        set => surfaceTemp = value; 
    }

    protected double density;
    public virtual double Density {
        get => density; 
        set {
            density = value;
            radius = GetRadiusFromMassAndDensity();
        }
    }

    protected Quaternion tilt; // axis of rotation, relative to plane Y = 0
    public virtual Quaternion Tilt {
        get => tilt;
        set => tilt = value;
    }

    protected double rotation; // radians around axis
    public virtual double Rotation {
        get => rotation;
        set => rotation = value;
    }

    protected double angularVelocity; // measured in radians/s
    public virtual double AngularVelocity {
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
        position = new Vector3d(0,0,0);
        velocity = new Vector3d(0,0,0);
        acceleration = new Vector3d(0,0,0);
        surfaceTemp = 0f;
        density = 1f;
        angularVelocity = 1 * Mathf.Deg2Rad;
        tilt = Quaternion.Euler(0,0,0);

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

    public double GetVolume()
    {
        return 4f / 3f * Math.PI * Math.Pow(radius, 3);
    }

    // auxiliary: returns values in relation to "p = m / v"
    public double GetRadiusFromMassAndDensity()
    {
        return Math.Pow(mass / density * 0.75f / Math.PI, 1f / 3f);
    }

    public double GetDensityFromMassAndVolume()
    {
        return mass / GetVolume();
    }

    #endregion Metric Methods
}
