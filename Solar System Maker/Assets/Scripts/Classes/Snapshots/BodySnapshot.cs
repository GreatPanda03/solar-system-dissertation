// represents a snapshot of a body at a point in time

using UnityEngine;

public class BodySnapshot
{

    #region Attributes

    private int id;
    public int Id {
        get => id;
        set => id = value;
    }

    private Vector3 position;
    public Vector3 Position {
        get => position;
        set => position = value;
    }

    private Vector3 velocity;
    public Vector3 Velocity {
        get => velocity;
        set => velocity = value;
    }

    public BodySnapshot(Body body)
    {
        // id = body.id
        position = body.Position;
        velocity = body.Velocity;
    }

    #endregion Attributes
}