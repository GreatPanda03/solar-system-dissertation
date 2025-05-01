// a star-specific renderable body

using System;
using UnityEngine;

public class RenderableStar : RenderableBody
{
    
    #region Attributes

    private Star body;
    public override Body Body {
        get => body;
        set => body = (Star) value;
    }

    private GameObject light;
    public GameObject Light {
        get => light;
        set => light = value;
    }

    private GameObject habitableZone;
    public GameObject HabitableZone {
        get => habitableZone;
        set => habitableZone = value;
    }

    private bool showHabitableZone = true;
    public bool ShowHabitableZone {
        get => showHabitableZone;
        set {
            showHabitableZone = value;
            SetHabitableZoneVisibility(value);
        }
    }

    #endregion Attributes


    #region Constructor

    public RenderableStar() : base() {}

    public RenderableStar(Star star)
    {
        Body = star;
    }

    #endregion Constructor


    #region Entity Methods

    // renders the star as nprmal
    public override void RenderBody(Vector3d offset, double scale)
    {
        base.RenderBody(offset, scale);
    }

    // deletes the star with its habitable zone
    public override void DestroyEntity()
    {
        base.DestroyEntity();

        if (habitableZone != null)
        {
            GameObject.Destroy(habitableZone);
        }
    }

    #endregion Entity Methods


    #region Habitable Zone

    // creates a habitable zone mesh for the RenderableStar
    public void CreateHabitableZone()
    {
        if (Entity == null) return;
        if (habitableZone != null) return;

        GameObject zonePrefab = Resources.Load<GameObject>("Prefabs/HabitableZone");
        habitableZone = GameObject.Instantiate(zonePrefab);
        habitableZone.name = "Habitable Zone";
        SetHabitableZoneVisibility(showHabitableZone);
        habitableZone.transform.SetParent(GameObject.Find("Decorators").transform);
    }

    // creates a new habitable zone for the star at the given scale
    // * source: https://www.youtube.com/watch?v=Rze4GEFrYYs
    // * source: https://worldbuilding.stackexchange.com/questions/79646/what-is-the-habitable-zone-around-my-star
    public void RenderHabitableZone(double scale)
    {
        if (Entity == null) return;

        if (habitableZone == null)
        {
            CreateHabitableZone();
        }

        // defining the habitable zone
        double auScale = UnitReference.DISTANCE_UNITS["AU"] * scale;
        double relativeLuminosity = body.Luminosity / UnitReference.LUMINOSITY_UNITS["Sun"];
        float innerRadius = (float) (Math.Sqrt(relativeLuminosity / 1.1f) * auScale);
        float outerRadius = (float) (Math.Sqrt(relativeLuminosity / 0.53f) * auScale);
        
        // building the habitable zone
        Mesh mesh = new();
        int segments = 64;
        Vector3[] vertices = new Vector3[segments * 2];
        int[] triangles = new int[(segments - 1) * 6];
        Vector2[] uv = new Vector2[vertices.Length];

        float step = 360f / (segments - 1);

        for (int s = 0; s < segments; s++)
        {
            float angle = Mathf.Deg2Rad * s * step;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            vertices[s * 2] = new Vector3(cos * innerRadius, 0f, sin * innerRadius);
            vertices[s * 2 + 1] = new Vector3(cos * outerRadius, 0f, sin * outerRadius);

            uv[s * 2] = new Vector2(0f, s / segments);
            uv[s * 2 + 1] = new Vector2(1f, s / segments);

            if (s < segments - 1)
            {
                int index = s * 6;
                int vi = s * 2;

                triangles[index] = vi;
                triangles[index + 1] = vi + 3;
                triangles[index + 2] = vi + 1;

                triangles[index + 3] = vi;
                triangles[index + 4] = vi + 2;
                triangles[index + 5] = vi + 3;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        // assigning and rendering the mesh
        MeshFilter meshFilter = habitableZone.GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        Material material = new(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = new Color(0, 1f, 0, 0.1f)
        };

        MeshRenderer meshRenderer = habitableZone.GetComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshRenderer.transform.position = Entity.transform.position;
    }

    // sets the visibility of the habitable zone
    public void SetHabitableZoneVisibility(bool visible)
    {
        if (habitableZone == null) return;
        habitableZone.GetComponent<MeshRenderer>().enabled = visible;
    }

    #endregion Habitable Zone
}