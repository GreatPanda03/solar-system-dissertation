// represents a single snapshot of the simulation at a point in time

using System.Collections.Generic;
using UnityEngine;

public class SpaceSnapshot
{

    #region Attributes

    private List<BodySnapshot> capture;

    #endregion Attributes


    #region Constructor

    public SpaceSnapshot()
    {
        capture = new List<BodySnapshot>();
    }

    #endregion Constructor


    #region Methods

    public void Capture()
    {
        SimulationController simController = GameObject.Find("Simulator")
            .GetComponent<SimulationController>();

        capture.Clear();

        foreach (Body body in simController.GetBodies())
        {
            BodySnapshot bodySnapshot = new(body);
            capture.Add(bodySnapshot);
        }
    }

    #endregion Methods
}