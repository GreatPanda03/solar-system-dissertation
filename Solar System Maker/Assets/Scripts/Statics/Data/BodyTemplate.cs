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
        id = -1;
        this.body = body;
        
        // name the body after the template foremost, 
        //   otherwise the template after the body
        if (name != null)
        {
            body.Name = name;
        }
        else
        {
            name = body.Name;
        }

        Debug.Log(name);
        Debug.Log(body.Name);
    }

    #endregion Methods
}