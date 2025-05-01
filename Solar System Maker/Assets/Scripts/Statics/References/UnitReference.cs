// a reference class to hold all units and their values
// * all values are relative to SI units (metres, kilogram, etc.)

using System.Collections.Generic;

public class UnitReference
{
    public static double G = 6.674_301_5e-11; // m^3 * kg^-1 * s^-2

    public static double STEFAN_BOLTZMANN = 5.670_374_419e-8; // W * m^2 * K^-4

    public static Dictionary<string, double> DISTANCE_UNITS = new() // metres
    {
        ["Centimetre"]      = 0.01,
        ["Metre"]           = 1,
        ["Kilometre"]       = 1e+3,
        ["Mile"]            = 1.609_344e+3,
        ["Earth Radius"]    = 6.367_5e+6,       // average between equator and pole
        ["Jupiter Radius"]  = 6.991_1e+7,
        ["Solar Radius"]    = 6.957e+8, 
        ["Lunar Distance"]  = 3.843_99e+8,      // average distance
        ["AU"]              = 1.495_978_707e+11
    };

    public static Dictionary<string, double> MASS_UNITS = new() // kilograms
    {
        ["Gram"]            = 1e-3,
        ["Pound"]           = 0.453_592_37,
        ["Kilogram"]        = 1,
        ["Tonne"]           = 1e+3,
        ["Moon"]            = 7.345_8e+22,
        ["Earth"]           = 5.972_2e+24,
        ["Jupiter"]         = 1.898e+27,
        ["Sun"]             = 1.988_5e+30
    };

    public static Dictionary<string, double> TIME_UNITS = new() // seconds
    {
        ["1 Millisecond"]     = 1e-3,
        ["10 Milliseconds"]   = 0.01,
        ["100 Milliseconds"]  = 0.1,
        ["1 Second"]          = 1,
        ["2 Seconds"]         = 2,
        ["5 Seconds"]         = 5,
        ["15 Seconds"]        = 15,
        ["30 Seconds"]        = 30,
        ["1 Minute"]          = 60,
        ["2 Minutes"]         = 120,
        ["5 Minutes"]         = 300,
        ["15 Minutes"]        = 900,
        ["30 Minutes"]        = 1.8e+3,
        ["1 Hour"]            = 3.6e+3,
        ["2 Hours"]           = 7.2e+3,
        ["6 Hours"]           = 2.16e+4,
        ["12 Hours"]          = 4.32e+4,
        ["1 Day"]             = 8.64e+4,
        ["1 Week"]            = 6.048e+5,
        ["2 Weeks"]           = 1.21e+6,
        ["1 Month"]           = 2.628e+6,   // 30.5 days (average)
        ["6 Months"]          = 1.5768e+7,  // ^ ditto
        ["1 Year"]            = 3.154e+7,
        ["1 Decade"]          = 3.154e+8,
        ["1 Century"]         = 3.154e+9,
        ["1 Millennium"]      = 3.154e+10
    };

    public static Dictionary<string, double> LUMINOSITY_UNITS = new() // watts (J/s)
    {
        ["Sun"]   = 3.83e+26
    };

    public static Dictionary<string, double> TEMPERATURE_UNITS = new() // kelvin
    {
        ["Kelvin"]      = 0,
        ["Celsius"]     = 273.15
    };
}
