// holds data for body templates


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

    public BodyTemplate() { }

    #endregion Methods
}