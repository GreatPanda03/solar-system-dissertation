// Holds the settings options for a simulation

using System;
using UnityEngine;

[Serializable]
public class Settings
{

    #region Attributes

    [SerializeField]
    private string simulationName;
    public string SimulationName {
        get => simulationName;
        set => simulationName = value;
    }

    [SerializeField]
    private string gravityAlgorithm;
    public string GravityAlgorithm {
        get => gravityAlgorithm;
        set => gravityAlgorithm = value;
    }

    [SerializeField]
    private float gravityConstant;
    public float GravityConstant {
        get => gravityConstant;
        set => gravityConstant = value;
    }

    [SerializeField]
    private string collisionQuality;
    public string CollisionQuality {
        get => collisionQuality;
        set => collisionQuality = value;
    }

    #endregion Attributes


    #region Constructor

    public Settings()
    {
        simulationName = "New Simulation";
        gravityAlgorithm = "Newtonian Physics";
        gravityConstant = 0.01f;
        collisionQuality = "High";
    }

    #endregion Constructor


    #region Methods

    public Settings GetCopy()
    {
        return new()
        {
            SimulationName = simulationName,
            GravityAlgorithm = gravityAlgorithm,
            GravityConstant = gravityConstant,
            CollisionQuality = collisionQuality
        };
    }

    public override string ToString()
    {
        return JsonUtility.ToJson(this, true);
    }

    #endregion Methods
}
