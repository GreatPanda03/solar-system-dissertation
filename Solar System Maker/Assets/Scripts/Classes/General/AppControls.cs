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
        ["Move Faster"]         = KeyCode.LeftControl,
        ["Move Slower"]         = KeyCode.LeftShift
    };
}