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
using TMPro;
using UnityEngine;

[Serializable]
public class Body
{

    #region Attributes

    readonly string type = "Body";
    public virtual string Type { get => type; }

    protected string name;
    public string Name {
        get => name;
        set {
            name = value;
            if (entity != null) entity.transform.name = name;
            if (label != null)
            {
                label.GetComponent<TextMeshProUGUI>().text = entity.name;
            }
        }   
    }

    protected GameObject entity;
    [JsonIgnore] // recreated at runtime anyway
    public GameObject Entity {
        get => entity;
        set {
            entity = value;
            entity.name = name;
            if (label != null)
                label.GetComponent<TextMeshProUGUI>().text = entity.name;
        }
    }

    protected GameObject label;
    [JsonIgnore] // recreated at runtime anyway
    public GameObject Label {
        get => label;
        set {
            label = value;
            label.GetComponent<TextMeshProUGUI>().text = entity.name;
        }
    }

    protected bool showLabel = true;
    [JsonIgnore] // only for simulation
    public bool ShowLabel {
        get => showLabel;
        set {
            showLabel = value;
            SetLabelVisibility(value);
        }
    }

    protected bool showTrail = true;
    [JsonIgnore] // only for simulation
    public bool ShowTrail {
        get => showTrail;
        set {
            showTrail = value;
            SetTrailVisibility(value);
        }
    }

    protected bool anchored = false;
    [JsonIgnore] // only for simulation
    public bool Anchored {
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
    public float Mass {
        get => mass;
        set {
            mass = value;
            density = GetDensityFromMassAndRadius();
        }
    }

    protected float radius;
    public float Radius {
        get => radius;
        set { 
            radius = value;
            density = GetDensityFromMassAndRadius();
        }
    }

    protected Vector3 position;
    public Vector3 Position {
        get => position; 
        set => position = value; 
    }

    protected Vector3 velocity;
    public Vector3 Velocity {
        get => velocity; 
        set => velocity = value; 
    }

    protected Vector3 acceleration;
    [JsonIgnore] // will always be serialized as empty Vector3 anyway
    public Vector3 Acceleration {
        get => acceleration; 
        set => acceleration = value; 
    }

    protected float surfaceTemp;
    public float SurfaceTemp {
        get => surfaceTemp; 
        set => surfaceTemp = value; 
    }

    protected float density;
    public float Density {
        get => density; 
        set {
            density = value;
            radius = GetRadiusFromMassAndDensity();
        }
    }

    protected float angularVelocity; // measured in radians/s
    public float AngularVelocity {
        get => angularVelocity;
        set {
            angularVelocity = value;
        }
    }
    
    // private Texture texture {
    //     get { return texture; }
    //     set
    //     {
    //         texture = value;
    //         // apply texture to body... 
    //     }
    // }

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

        radius = GetRadiusFromMassAndDensity();
    }

    public Body(float mass, Vector3 position, Vector3 velocity)
    {
        new Body()
        {
            mass = mass,
            position = position,
            velocity = velocity
        };
    }

    #endregion Constructor


    #region General Methods

    public virtual Body GetShallowCopy()
    {
        return (Body) MemberwiseClone();
    }

    public virtual Body GetDeepCopy()
    {
        Body copy = (Body) MemberwiseClone();
        copy.entity = entity != null ? UnityEngine.Object.Instantiate(entity) : null;
        copy.label = label != null ? UnityEngine.Object.Instantiate(label) : null; 

        return copy;
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

    #endregion General Methods


    #region Entity Methods

    // auxiliary: returns values in relation to "p = m / v"
    public float GetRadiusFromMassAndDensity()
    {
        return Mathf.Pow(mass / density * 0.75f / Mathf.PI, 1f / 3f);
    }

    public float GetDensityFromMassAndRadius()
    {
        return mass * 0.75f / Mathf.PI / Mathf.Pow(radius, 3);
    }

    // applies the given force to the body
    // * F = m * a   :.   a = F / m
    public void ApplyForce(Vector3 force)
    {
        acceleration += force / mass;
    }

    // updates the body's physics
    public void Update(float dt)
    {
        if (entity != null)
        {
            entity.transform.rotation 
                *= Quaternion.Euler(0, -angularVelocity * Mathf.Rad2Deg * dt, 0);
        }

        velocity += acceleration * dt;
        position += velocity * dt;
        acceleration = new Vector3(0,0,0);
    }

    // returns a new generic Body entity
    public void CreateEntity() {
        GameObject bodyPrefab = Resources.Load<GameObject>($"Prefabs/{this.Type}");
        entity = GameObject.Instantiate(bodyPrefab);
        entity.name = this.Name;
        entity.transform.position = this.position;

        GameObject labelPrefab = Resources.Load<GameObject>("Prefabs/BodyLabel");
        label = GameObject.Instantiate(labelPrefab);
        label.name = "BodyLabel";
        label.GetComponent<TextMeshProUGUI>().text = name;
        label.transform.SetParent(GameObject.Find("Overlay").transform, false);
    }

    // sets the visibility of the body's label
    public void SetLabelVisibility(bool visible)
    {
        label.GetComponent<TextMeshProUGUI>().enabled = visible;
    }

    // sets the visibility of the body's trail
    public void SetTrailVisibility(bool visible)
    {
        entity.GetComponent<TrailRenderer>().enabled = visible;
        if (!visible) entity.GetComponent<TrailRenderer>().Clear();
    }

    // destroys the entity's physical components
    public void DestroyEntity()
    {
        if (entity != null)
        {
            GameObject.Destroy(entity);
            entity = null;
        }

        if (label != null)
        {
            GameObject.Destroy(label);
            label = null;
        }
    }

    #endregion Entity Methods
}
