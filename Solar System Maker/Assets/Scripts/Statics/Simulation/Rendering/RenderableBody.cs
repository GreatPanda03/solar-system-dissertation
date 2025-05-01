// manages renderable instances of bodies
// * this class acts as a wrapper around a body to then give it a physical form
// * the body handles the physics, the renderable handles the looks - everyone looks at the renderable!

using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class RenderableBody
{

    #region Attributes

    private Body body;
    public Body Body {
        get => body;
        set => body = value;
    }

    private GameObject entity;
    public GameObject Entity {
        get => entity;
        set {
            entity = value;
            entity.name = body.Name;
            if (label != null)
                label.GetComponent<TextMeshProUGUI>().text = entity.name;
        }
    }

    private GameObject label;
    public GameObject Label {
        get => label;
        set {
            label = value;
            label.GetComponent<TextMeshProUGUI>().text = entity.name;
        }
    }

    private bool showLabel = true;
    public bool ShowLabel {
        get => showLabel;
        set {
            showLabel = value;
            SetLabelVisibility(value);
        }
    }

    private LineRenderer line;
    public LineRenderer Line {
        get => line;
        set => line = value;
    }

    private TrailRenderer trail;
    public TrailRenderer Trail {
        get => trail;
        set => trail = value;
    }

    private bool showTrail = true;
    public bool ShowTrail {
        get => showTrail;
        set {
            showTrail = value;
            SetTrailVisibility(value);
        }
    }

    #endregion Attributes


    #region Constructor

    public RenderableBody() {}

    #endregion Constructor


    #region Object Methods

    public virtual RenderableBody GetCopy()
    {
        RenderableBody copy = (RenderableBody) MemberwiseClone();
        copy.body = body.GetCopy();

        return copy;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(
            this, 
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            }
        );
    }

    #endregion Object Methods


    #region Entity Methods

    // creates a new physical entity for the renderable
    public virtual void CreateEntity() {
        GameObject bodyPrefab = Resources.Load<GameObject>($"Prefabs/Body");
        entity = GameObject.Instantiate(bodyPrefab);
        entity.name = body.Name;
        entity.transform.position = (Vector3) body.Position;

        if (body.Texture != null)
        {
            Material material = Resources.Load<Material>(body.Texture);

            if (material != null)
            {
                entity.GetComponent<MeshRenderer>().material = material;
            }    
            else
            {
                ApplyDefaultTexture();
            }
        }
        else
        {
            ApplyDefaultTexture();
        }
    }

    // applies the default texture the body (determined by type)
    public void ApplyDefaultTexture()
    {
        entity.GetComponent<MeshRenderer>().material // standard material
                = Resources.Load<Material>($"Materials/Generic/{body.GetType()}");
    }

    // creates all decorators for the entity
    public void CreateAllDecorators()
    {
        CreateLabel();
        CreateLine();
        CreateTrail();
    }

    // renders the body in physical space at the given scale
    public void RenderBody(double scale)
    {
        if (entity == null || body == null) return;

        Transform transform = entity.transform;
        transform.position = (Vector3) (scale * body.Position);
        transform.localScale = (Vector3) (body.Radius * 2 * scale * Vector3d.one);

        transform.rotation = body.Tilt 
            * Quaternion.Euler(0, (float) body.Rotation * Mathf.Rad2Deg, 0);
    }
    
    // destroys the entity's physical components
    public void DestroyEntity()
    {
        if (entity != null)
        {
            GameObject.Destroy(entity);
            entity = null;
        }

        if (label != null)
        {
            GameObject.Destroy(label);
            label = null;
        }
    }

    #endregion Entity Methods


    #region Label Methods

    // creates a new physical label for the renderable
    public void CreateLabel()
    {
        GameObject labelPrefab = Resources.Load<GameObject>("Prefabs/Label");
        label = GameObject.Instantiate(labelPrefab);
        label.name = "Label";
        label.GetComponent<TextMeshProUGUI>().text = body.Name;
        label.transform.SetParent(GameObject.Find("Overlay").transform, false);
    }

    // renders the given body's label at the given scale
    // * positions the label at "top right" of body, relative to the camera
    public void RenderLabel()
    {
        if (entity == null || label == null) return;

        float radius = entity.transform.localScale.magnitude / 2;

        Transform origin = Camera.main.transform;
        Vector3 position = entity.transform.position;
        Vector3 edge = (origin.right + origin.up) * radius; // parallel to camera forward
        Vector3 screenPoint = Camera.main.WorldToScreenPoint((Vector3) (position + edge));

        label.transform.position = screenPoint;
        label.GetComponent<TextMeshProUGUI>().enabled = screenPoint.z >= 0;

        // ^ technically has a screen position even when BEHIND the camera
        // * this check stops the label from showing up when behind the camera
    }

    // updates the label with the given text
    public void UpdateLabel(string text)
    {
        label.GetComponent<TextMeshProUGUI>().text = text;
    }

    // sets the visibility of the body's label
    public void SetLabelVisibility(bool visible)
    {
        if (label == null) return;
        label.GetComponent<TextMeshProUGUI>().enabled = visible;
    }

    #endregion Label Methods

    
    #region Line Methods

    // creates a new line for the renderable body to use
    public void CreateLine()
    {
        if (entity == null) return;

        line = entity.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.widthMultiplier = 0.1f;

        Gradient gradient = new();

        gradient.SetKeys( // line that fades out
            new GradientColorKey[] {new(Color.white, 0), new(Color.white, 1)},
            new GradientAlphaKey[] {new(1, 0), new(0, 1)});

        line.colorGradient = gradient;
        line.SetPositions(new Vector3[] {});
    }

    #endregion Line Methods


    #region Trail Methods

    // creates a new trail for the renderable body to use
    public void CreateTrail()
    {
        if (entity == null) return;

        trail = entity.AddComponent<TrailRenderer>();
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.widthMultiplier = 0.1f;

        Gradient gradient = new();
        
        gradient.SetKeys( // trail that fades out
            new GradientColorKey[] {new(Color.white, 0), new(Color.white, 1)},
            new GradientAlphaKey[] {new(1, 0), new(0, 1)});

        trail.colorGradient = gradient;
    }

    // sets the visibility of the body's trail
    public void SetTrailVisibility(bool visible)
    {
        if (trail == null) return;
        
        trail.enabled = visible;
        if (!visible) trail.Clear();
    }

    #endregion Trail Methods
}