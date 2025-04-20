
using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitReference
{
    private static SimulatorController simController
        = GameObject.Find("Simulator").GetComponent<SimulatorController>(); 

    public static Dictionary<string, double> DISTANCE_UNITS = new() {
        ["1 Centimeter"]        = 1e-5,
        ["1 Meter"]             = 1e-3,
        ["1 Kilometer"]         = 1,
        ["1 Mile"]              = 1.609_344,
        ["Lunar Orbit"]         = 3.844_00e5,
        ["AU"]                  = 1.495_978_707e8
    };

    public static Dictionary<string, double> MASS_UNITS = new() {
        ["1 Gram"]          = 1e-3,
        ["1 Pound"]         = 1.609_344,
        ["1 Kilogram"]      = 1,
        ["1 Tonne"]         = 1_000,
        ["1 Moon"]          = 7.346e22,
        ["1 Earth"]         = 5.972e24,
        ["1 Sun"]           = 1.989e30
    };
}
