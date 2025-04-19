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

            // fragments the grid lines so they're recursive
            // * also fades inner grid lines out the further you go (and conversely)
            float4 frag(v2f i) : SV_Target
            {
                float2 worldXZ = i.worldPosition.xz / _GridScale;
                float logZoom = log10(abs(_WorldSpaceCameraPos.y));

                float grid = 0.0;
                const int layers = 10;

                // each layer represents 1 order of magnitude
                for (int layer = -2; layer < layers; layer++)
                {
                    float scale = _GridScale * pow(10.0, layer);
                    float fade = smoothstep(0.0f, _FadeWidth, 1.0f - abs(logZoom - float(layer)));
                    float thickness = _GridLineThickness / pow(10.0, layer - 1);

                    grid += DrawGrid(worldXZ, scale, thickness) * fade;
                }

                grid = saturate(grid);
                return float4(_GridLineColour.rgb, grid * _GridLineColour.a);
            }
            ENDCG
        }
    }
}