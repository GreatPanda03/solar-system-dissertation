// holds all keybinds for the application

using System.Collections.Generic;
using UnityEngine;

public class AppControls
{
    public static Dictionary<string, KeyCode> keybinds = new() {
        ["Place Body"]          = KeyCode.Q,
        ["Deselect Preview"]    = KeyCode.E,
        ["Explorer"]            = KeyCode.Tab,
        ["Properties"]          = KeyCode.P,
        ["Follow"]              = KeyCode.F,
        ["Adjust Scale"]        = KeyCode.V,

        ["Move Slower"]         = KeyCode.LeftShift,
        ["Move Faster"]         = KeyCode.LeftControl,
        
        ["Decrease Eccentricity"]   = KeyCode.Z,
        ["Increase Eccentricity"]   = KeyCode.X,
        ["Flip Orbit"]          = KeyCode.C,

        ["Lower Plane"]         = KeyCode.U,
        ["Raise Plane"]         = KeyCode.I,
        ["Turn Plane Left"]     = KeyCode.J,
        ["Turn Plane Right"]      = KeyCode.K,
        ["Turn Plane Backwards"]  = KeyCode.N,
        ["Turn Plane Forwards"]   = KeyCode.M,
        ["Reset Plane"]         = KeyCode.O,
        ["Localise Plane"]      = KeyCode.L
    };
}