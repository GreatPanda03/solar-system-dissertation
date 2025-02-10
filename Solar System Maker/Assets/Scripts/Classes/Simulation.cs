// Representations of simulations

using System;
using System.Collections.Generic;

[Serializable]
public class Simulation
{
    public int id;
    public Settings settings;
    public List<Body> bodies;

    public Simulation() {}
}