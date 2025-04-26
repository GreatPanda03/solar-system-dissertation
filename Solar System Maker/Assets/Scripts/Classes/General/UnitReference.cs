// a reference class to hold all units and their values
// * all values are relative to SI units (metres, kilogram, etc.)

using System.Collections.Generic;

public class UnitReference
{
    // public static float G = 6.674_301_5e-11f;
    public static float G = 1;

    public static Dictionary<string, double> DISTANCE_UNITS = new() {
        ["Centimetre"]      = 1e-2,
        ["Metre"]           = 1,
        ["Kilometre"]       = 1e+3,
        ["Mile"]            = 1.609_344e+3,
        ["Lunar Distance"]  = 3.843_99e8,
        ["AU"]              = 1.495_978_707e11
    };

    public static Dictionary<string, double> MASS_UNITS = new() {
        ["Gram"]            = 1e-3,
        ["Pound"]           = 0.453_592_37,
        ["Kilogram"]        = 1,
        ["Tonne"]           = 1e3,
        ["Moon"]            = 7.345_8e22,
        ["Earth"]           = 5.972_2e24,
        ["Sun"]             = 1.988_5e30
    };
}
