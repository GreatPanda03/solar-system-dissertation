// Representations of simulations

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Simulation
{
    public int id;
    public Settings settings;
    public List<Body> bodies;

    public Simulation() {
        id = -1;
        settings = new Settings();
        bodies = new List<Body>();
    }

    // returns a copy of the simulation
    public Simulation GetCopy()
    {
        List<Body> bodiesCopy = new();
        foreach (Body body in bodies)
            bodiesCopy.Add(body.GetCopy());

        return new()
        {
            id = id,
            settings = settings.GetCopy(),
            bodies = bodiesCopy
        };
    }

    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}