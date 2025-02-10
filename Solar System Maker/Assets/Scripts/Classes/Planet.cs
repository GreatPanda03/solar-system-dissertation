// Creating instances of Planets
// * all other body types extend from this class

/* REFERENCES:
* [1] - Andrew Raphael Lukasik, "BFN", www.github.com [Online]. Available at: https://github.com/andrew-raphael-lukasik/BFN
*/

using System;
using UnityEngine;

[Serializable]
public class Planet : Body
{
    [SerializeField]
    protected float habitability;
    public float Habitability {
        get { return habitability;  }
        set { habitability = value; }
    }

    /*
    private Atmosphere Atmosphere {
        get { return atmosphere;  }
        set { atmosphere = value; }
    }
    */

    [SerializeField]
    protected float waterContent;
    public float WaterContent {
        get { return waterContent;  }
        set { waterContent = value; }
    }

    public Planet()
    {
        name = "Planet";
        mass = 10f;
        habitability = 0;
        waterContent = 0;
        radius = GetRadiusFromMass();
    }

    public Planet(float mass, Vector3 position, Vector3 velocity)
        : base(mass, position, velocity)
    {
        name = "Planet";
        habitability = 0;
        waterContent = 0;
    }

    public Planet(float mass, Vector3 position, Vector3 velocity, float habitability, float waterContent)
        : base(mass, position, velocity)
    {
        name = "Planet";
        this.habitability = habitability;
        this.waterContent = waterContent;
    }
}
