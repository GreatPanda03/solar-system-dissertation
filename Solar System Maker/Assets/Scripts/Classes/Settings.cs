// Holds the settings options for a simulation

public class Settings
{
    private string simulationName;
    public string SimulationName {
        get => simulationName;
        set => simulationName = value;
    }

    private string gravityAlgorithm;
    public string GravityAlgorithm {
        get => gravityAlgorithm;
        set => gravityAlgorithm = value;
    }

    private float gravityConstant;
    public float GravityConstant {
        get => gravityConstant;
        set => gravityConstant = value;
    }

    private string collisionQuality;
    public string CollisionQuality {
        get => collisionQuality;
        set => collisionQuality = value;
    }

    public Settings()
    {
        gravityAlgorithm = "Newtonian Physics";
        gravityConstant = 0.001f;
        collisionQuality = "High";
    }
}
