// Creating instances of Bodies
// * all other body types extend from this class

/* REFERENCES:
* [1] - StackOverflow, "Using Getters and Setters in Unity", www.stackoverflow.com [Online]. Available at: https://stackoverflow.com/questions/23649784/using-getters-and-setters-in-unity.
* [2] - W3Schools, "C# Properties (Get and Set)", www.w3schools.com [Online]. Available at: https://www.w3schools.com/cs/cs_properties.php#:~:text=It%20is%20a%20good%20practice,we%20assign%20to%20the%20property.
* [3] - Daniel Shiffman, "2.6 Mutual Attraction - The Nature of Code", The Coding Train, www.youtube.com [Online]. Available at: https://www.youtube.com/watch?v=GjbKsOkN1Oc
* [4] - Andrew Raphael Lukasik, "BFN", www.github.com [Online]. Available at: https://github.com/andrew-raphael-lukasik/BFN
*/


// LIBRARIES
using System;
using Newtonsoft.Json;
using UnityEngine;


// CLASS DEFINITION
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
            if (entity != null) entity.transform.name = value;
            // update physical name label in world space...
        }   
    }

    protected GameObject entity;
    [JsonIgnore] // recreated a runtime
    public GameObject Entity {
        get => entity;
        set {
            entity = value;
            // TOOD: make proper set
        }
    }

    protected float mass;
    public float Mass {
        get => mass;
        set => mass = value; 
    }

    protected float radius;
    public float Radius {
        get => radius;
        set {
            radius = value;
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
        set => density = value; 
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


    #region Class Methods

    public Body()
    {
        name = "Body";
        mass = 10f;
        position = new Vector3(0,0,0);
        velocity = new Vector3(0,0,0);
        acceleration = new Vector3(0,0,0);
        surfaceTemp = 0f;
        density = 1f;

        radius = GetRadiusFromMass();
    }

    public Body(float mass, Vector3 position, Vector3 velocity)
    {
        name = "Body";
        this.mass = mass;
        this.position = position;
        this.velocity = velocity;
        acceleration = new Vector3(0,0,0);
        surfaceTemp = 0f;
        density = 1f;

        radius = GetRadiusFromMass();
    }

    // returns a copy of the body
    public virtual Body GetCopy()
    {
        return new Body()
        {
            name = name,
            mass = mass,
            position = position,
            velocity = velocity,
            acceleration = acceleration,
            surfaceTemp = surfaceTemp,
            density = density,
            radius = radius
        };
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

    #endregion Class Methods


    #region Entity Methods

    // auxiliary: returns a radius value from the body's mass
    public float GetRadiusFromMass()
    {
        return (float) Math.Pow(3 * mass / (4 * Math.PI), 1f / 3f);
    }

    // returns the force applied between two objects
    // * F1 = F2 = G * (m1 * m2) / r^2
    // * NOTE: the force's magnitude is the same, but the direction is different
    //   > this method returns an attractive force by THIS on OTHER
    public Vector3 GetForceBetween(Body other, float G)
    {
        Vector3 vector = other.Position - this.position;
        Vector3 direction = vector.normalized;
        float distance = Math.Max(vector.magnitude, this.radius + other.radius);
        float magnitude = G * (this.mass * other.mass) / (distance * distance);

        Vector3 force = magnitude * direction;

        return force;
    }

    // applies the given force to the body
    // * F = m * a   :.   a = F / m
    public void ApplyForce(Vector3 force)
    {
        acceleration += force / mass;
    }

    // updates the body's physics
    public void Update()
    {
        float dt = Time.deltaTime;

        // TODO: use dt

        velocity += acceleration; // * dt;
        position += velocity; // * dt;
        acceleration = new Vector3(0,0,0);
    }

    // returns a new generic Body entity
    public void CreateEntity() {
        GameObject prefab = Resources.Load<GameObject>($"Prefabs/{this.Type}");
        entity = GameObject.Instantiate(prefab);
        entity.name = this.Name;
    }

    #endregion Entity Methods
}
