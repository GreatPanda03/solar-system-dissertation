// a star-specific renderable body

using UnityEngine;

public class RenderableStar : RenderableBody
{
    
    #region Attributes

    private GameObject light;
    public GameObject Light {
        get => light;
        set => light = value;
    }

    private bool showHabitableZone = false;
    public bool ShowHabitableZone {
        get => showHabitableZone;
        set => showHabitableZone = false;
    }

    private GameObject habitableZone;
    public GameObject HabitableZone {
        get => habitableZone;
        set => habitableZone = value;
    }

    #endregion Attributes


    #region Constructor

    public RenderableStar() : base() {}

    #endregion Constructor


    #region Methods

    // creates a new habitable zone for the star
    public void CreateHabitableZone()
    {

    }

    // updates the star's habitable zone depending on its body's properties
    public void UpdateHabitableZone()
    {
        if (habitableZone == null) return;

        // TODO: update habitable zone accordingly
    }

    #endregion Methods
}