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

    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}