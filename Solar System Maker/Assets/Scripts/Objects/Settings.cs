// Holds the settings options for a simulation

using System;
using UnityEngine;

[Serializable]
public class Settings
{
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

    public Settings()
    {
        simulationName = "New Simulation";
        gravityAlgorithm = "Newtonian Physics";
        gravityConstant = 0.01f;
        collisionQuality = "High";
    }

    public override string ToString()
    {
        return JsonUtility.ToJson(this, true);
    }
}
