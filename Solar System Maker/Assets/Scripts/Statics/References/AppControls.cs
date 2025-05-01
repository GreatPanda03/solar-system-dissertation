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

        ["Move Slower"]         = KeyCode.LeftControl,
        ["Move Faster"]         = KeyCode.LeftShift,
        
        ["Decrease Eccentricity"]   = KeyCode.Z,
        ["Increase Eccentricity"]   = KeyCode.X,
        ["Flip Orbit"]          = KeyCode.C,

        ["Lower Plane"]         = KeyCode.T,
        ["Raise Plane"]         = KeyCode.Y,
        ["Descend On Plane"]    = KeyCode.G,
        ["Ascend On Plane"]     = KeyCode.H,
        ["Reset Plane"]         = KeyCode.B,
        ["Localise Plane"]      = KeyCode.N
    };
}