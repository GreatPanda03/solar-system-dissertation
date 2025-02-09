// Handles the orbit demo for the main menu

using System;
using System.Collections.Generic;
using UnityEngine;

public class OurSystemDemoScript : MonoBehaviour
{  
    public GameObject StarTemplate;
    public GameObject PlanetTemplate;

    public List<GameObject> planets;
    public Vector3 systemCentre;

    private Dictionary<string, Dictionary<string, string>> systemData;
    private Dictionary<string, Color> planetColours;

    private void Awake() {
        systemCentre = new Vector3(0,0,0);

        // fixed data for the entire solar system
        // * distance is in Astronimical Units, mass is in Earth weights, period is in Earth cycles
        systemData = new Dictionary<string, Dictionary<string, string>>()
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

        planetColours = new Dictionary<string, Color>()
        {
            {"Mercury", new Color(125f / 255f, 125f / 255f, 125f / 255f)},
            {"Venus",   new Color(200f / 255f,           0,           0)},
            {"Earth",   new Color(          0, 200f / 255f,           0)},
            {"Mars",    new Color(255f / 255f,           0,           0)},
            {"Jupiter", new Color(200f / 255f, 150f / 255f,  50f / 255f)},
            {"Saturn",  new Color(200f / 255f, 200f / 255f,  50f / 255f)},
            {"Uranus",  new Color( 50f / 255f, 150f / 255f, 255f / 255f)},
            {"Neptune", new Color(100f / 255f, 100f / 255f, 200f / 255f)}
        };
    }

    // preparing the initial demo with a single star and some number of planets
    private void Start()
    {
        foreach (string bodyName in systemData.Keys)
        {
            GameObject body = null;
            GameObject systemContainer = GameObject.Find("SimulatorContents");

            if (systemData[bodyName]["Type"] == "Star")
            {
                body = Instantiate(StarTemplate);
                body.transform.position = systemCentre;
                body.transform.localScale = new Vector3(3,3,3);
                body.transform.name = "Sun";
            }
            else if (systemData[bodyName]["Type"] == "Planet")
            {
                float distance = Convert.ToSingle(systemData[bodyName]["Distance"]) 
                    * 10f;

                body = Instantiate(PlanetTemplate);
                body.transform.position = new Vector3(distance,0,0);
                body.transform.name = bodyName;
                body.GetComponent<MeshRenderer>().material.color = planetColours[bodyName];
                body.transform.localScale = new Vector3(1,1,1);
                    // * (float) Math.Log(Convert.ToSingle(systemData[bodyName]["Mass"]));

                planets.Add(body);
            }

            if (body != null)
            {
                body.transform.parent = systemContainer.transform;
            }
            
        }

        Debug.Log("[OUR SYSTEM] SUCESS: System Loaded");
    }

    // making the planets orbit around the star indefinitely
    private void FixedUpdate()
    {
        foreach (GameObject planet in planets)
        {
            string planetName = planet.transform.name;
            float period = Convert.ToSingle(systemData[planetName]["Period"]);

            planet.transform.RotateAround(systemCentre, Vector3.up, 1 / period);
        }
    }
}