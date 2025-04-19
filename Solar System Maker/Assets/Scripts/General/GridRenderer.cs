using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    public Color majorLineColor = Color.white;
    public Color minorLineColor = new Color(1, 1, 1, 0.2f);
    public float spacing = 1f;
    public int gridSize = 100;
    public int majorLineEvery = 10;
    public float cameraHeightFadeStart = 30f;
    public float cameraHeightFadeEnd = 60f;

    private Material _lineMaterial;

    void OnEnable()
    {
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        _lineMaterial = new Material(shader)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        _lineMaterial.SetInt("_ZWrite", 0);
    }

    void Update()
    {
        if (!_lineMaterial) return;

        Camera cam = Camera.main;
        if (!cam) return;

        _lineMaterial.SetPass(0);
        GL.Begin(GL.LINES);

        float camY = cam.transform.position.y;
        float alpha = Mathf.InverseLerp(cameraHeightFadeEnd, cameraHeightFadeStart, camY);
        alpha = Mathf.Clamp01(alpha);

        for (int i = -gridSize; i <= gridSize; i++)
        {
            bool isMajor = i % majorLineEvery == 0;
            Color color = isMajor ? majorLineColor : minorLineColor;
            color.a *= isMajor ? 1 : alpha;

            GL.Color(color);

            // Vertical lines
            GL.Vertex3(i * spacing, 0, -gridSize * spacing);
            GL.Vertex3(i * spacing, 0, gridSize * spacing);

            // Horizontal lines
            GL.Vertex3(-gridSize * spacing, 0, i * spacing);
            GL.Vertex3(gridSize * spacing, 0, i * spacing);
        }

        GL.End();
    }
}
