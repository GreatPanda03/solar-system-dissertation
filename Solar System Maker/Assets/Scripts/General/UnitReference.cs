
using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitReference
{
    private static SimulatorController simController
        = GameObject.Find("Simulator").GetComponent<SimulatorController>(); 

    public static Dictionary<string, double> DISTANCE_UNITS = new() {
        ["Centimetre"]      = 1e-5,
        ["Metre"]           = 1e-3,
        ["Kilometre"]       = 1,
        ["Mile"]            = 1.609_344,
        ["Lunar Distance"]  = 3.844_00e5,
        ["AU"]              = 1.495_978_707e8
    };

    public static Dictionary<string, double> MASS_UNITS = new() {
        ["Gram"]            = 1e-3,
        ["Pound"]           = 0.453_592_37,
        ["Kilogram"]        = 1,
        ["Tonne"]           = 1e3,
        ["Moon"]            = 7.346e22,
        ["Earth"]           = 5.972e24,
        ["Sun"]             = 1.989e30
    };
}
