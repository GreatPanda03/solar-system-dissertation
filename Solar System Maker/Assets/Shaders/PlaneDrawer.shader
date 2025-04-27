// used for the grid plane in space when creating solar systems
// * "subgrids" fade out and "supergrids" fade in when zooming out
//   and likewise conversely when zooming in

Shader "Unlit/GridPlaneShader"
{
    Properties
    {
        // grid properties (used below)
        _GridLineColour ("Grid Line Colour", Color) = (1,1,1,1)
        _GridLineThickness ("Grid Line Thickness", Float) = 1
        _GridScale ("Grid Scale", Float) = 0.02
        _FadeWidth ("Fade Width", Float) = 1
    }
    
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
        Cull Off

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            
            // NB: "vert" and "frag" are built-in methods automatically called by Unity
            // * "GridLine" is called manually to actually draw the lines
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _GridLineColour;
            float  _GridLineThickness;
            float  _GridScale;
            float  _FadeWidth;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 worldPosition : TEXCOORD0;
            };

            // defining vertex
            v2f vert(appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            // draws the set of grid lines
            float DrawGrid(float2 worldXZ, float scale, float thickness)
            {
                float2 coord = frac(worldXZ / scale);
                coord = min(coord, 1.0 - coord);
                float lineX = step(coord.x, thickness);
                float lineY = step(coord.y, thickness);
                return saturate(lineX + lineY);
            }

            // fragmentation method:
            // 1. a "subgrid" covers the "decimal" component of the zoom factor
            // 2. a "supergrid" covers the "integer" component of the zoom factor
            float4 frag(v2f i) : SV_Target
            {

                // fade out subgrid with height
                float height = length(_WorldSpaceCameraPos.y);
                float visibility = saturate(1.0 - height / 25.0);

                float2 worldXZ = i.worldPosition.xz / _GridScale;
                float scale, thickness, grid;

                // super grid
                scale = _GridScale * 10;
                thickness = _GridLineThickness * 0.5;
                grid = DrawGrid(worldXZ, scale, thickness);
                
                // subgrid
                scale = _GridScale * 1;
                thickness = _GridLineThickness;
                grid += DrawGrid(worldXZ, scale, thickness) * visibility;

                grid = saturate(grid);

                return float4(_GridLineColour.rgb, grid * _GridLineColour.a);
            }
            ENDCG
        }
    }
}