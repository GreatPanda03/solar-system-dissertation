// holds lists of pre-made body templates

using System.Collections.Generic;
using UnityEngine;

public static class BodyIndex
{

    #region Attributes

    public static List<BodyTemplate> stars = new()
    {
        {
            new()
            {
                Name = "Sun", // description: source: https://science.nasa.gov/sun/facts/
                Description = "Our Sun is a 4.5 billion-year-old yellow dwarf star - a hot glowing ball of hydrogen and helium - at the center of our solar system. It's about 93 million miles (150 million kilometers) from Earth and it's our solar system's only star. Without the Sun's energy, life as we know it could not exist on our home planet.",
                Body = new Star()
                {
                    Name = "Sun",
                    Mass = (float) UnitReference.MASS_UNITS["Sun"],
                    Radius = (float) UnitReference.DISTANCE_UNITS["Solar Radius"],
                    AngularVelocity = 2.7e-6f,
                    Texture = Resources.Load<Material>("Materials/Specific/Sun")
                }
            }
        },
    };

    public static List<BodyTemplate> planets = new()
    {
        {
            new()
            {
                Name = "Earth", // description source: https://science.nasa.gov/earth/facts/
                Description = "Earth - our home planet - is the third planet from the Sun, and the fifth largest planet. It's the only place we know of inhabited by living things.",
                Body = new Planet()
                {
                    Name = "Earth",
                    Mass = (float) UnitReference.MASS_UNITS["Earth"],
                    Radius = (float) UnitReference.DISTANCE_UNITS["Earth Radius"],
                    AngularVelocity = 7.29e-5f,
                    Texture = Resources.Load<Material>("Materials/Specific/Earth")
                }
            }
        }
    };

    public static List<BodyTemplate> moons = new()
    {
        {
            new() 
            {
                Name = "Moon", // description source: https://science.nasa.gov/moon/facts/
                Description = "The Moon is Earth's only natural satellite, a spherical rocky body that orbits our planet",
                Body = new Moon()
                {
                    Name = "Moon",
                    Mass = (float) UnitReference.MASS_UNITS["Moon"],
                    Radius = 1.74e6f,
                    AngularVelocity = 2.661_7e-6f,
                    Texture = Resources.Load<Material>("Materials/Specific/Moon")
                }
            }
        }
    };

    #endregion Attributes
}