// stores an index of planets to be used


using System.Collections.Generic;

public class PlanetIndex
{

    #region Attributes



    #endregion Attributes


    #region Planets

    // loads all planet data
    public void GetAllPlanets()
    {
        Dictionary<string, Dictionary<string, string>> systemData = new()
        {
            {
                "Sun", new Dictionary<string, string>()
                {
                    {"Type", "Star"},
                    {"Mass", "333,000"}
                }
            },
            {
                "Mercury", new Dictionary<string, string>()
                {
                    {"Type", "Planet"},
                    {"Mass", "0.0553"},
                    {"Distance", "0.4"},
                    {"Period", "0.2411"}
                }
            },
            {
                "Venus", new Dictionary<string, string>()
                {
                    {"Type", "Planet"},
                    {"Mass", "0.815"},
                    {"Distance", "0.72"},
                    {"Period", "0.6156"}
                }
            },
            {
                "Earth", new Dictionary<string, string>()
                {
                    {"Type", "Planet"},
                    {"Mass", "1"},
                    {"Distance", "1"},
                    {"Period", "1"}
                }
            },
            {
                "Mars", new Dictionary<string, string>()
                {
                    {"Type", "Planet"},
                    {"Mass", "0.107"},
                    {"Distance", "1.52"},
                    {"Period", "1.88"}
                }
            },
            {
                "Jupiter", new Dictionary<string, string>()
                {
                    {"Type", "Planet"},
                    {"Mass", "317.8"},
                    {"Distance", "5.20"},
                    {"Period", "11.86"}
                }
            },
            {
                "Saturn", new Dictionary<string, string>()
                {
                    {"Type", "Planet"},
                    {"Mass", "95.2"},
                    {"Distance", "9.5"},
                    {"Period", "29.4"}
                }
            },
            {
                "Uranus", new Dictionary<string, string>()
                {
                    {"Type", "Planet"},
                    {"Mass", "14.6"},
                    {"Distance", "19.2"},
                    {"Period", "84.07"}
                }
            },
            {
                "Neptune", new Dictionary<string, string>()
                {
                    {"Type", "Planet"},
                    {"Mass", "17.2"},
                    {"Distance", "30.06"},
                    {"Period", "164.9"}
                }
            }
        };
    }

    #endregion Planets
}
            