// holds data for body templates

using UnityEngine;

public class BodyTemplate
{
    #region Attributes

    public int id;

    private string name;
    public string Name {
        get => name;
        set => name = value;
    }

    private string description;
    public string Description {
        get => description;
        set => description = value;
    }

    private Body body;
    public Body Body {
        get => body;
        set => body = value;
    }

    #endregion Attributes


    #region Methods

    public BodyTemplate() {
        id = -1;
    }

    public BodyTemplate(Body body)
    {
        Debug.Log(name);
        Debug.Log(body.Name);
        id = -1;
        name ??= body.Name; // prioritises template name over body name
        this.body = body;
    }

    #endregion Methods
}