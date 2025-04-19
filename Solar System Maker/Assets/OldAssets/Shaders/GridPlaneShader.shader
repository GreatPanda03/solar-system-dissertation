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
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
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

                /*
                coord = abs(frac(coord / scale) - 0.5) * 2.0;
                float gridLine = min(coord.x, coord.y); 
                gridLine = smoothstep(thickness, thickness + 0.01, gridLine);
                return 1.0 - gridLine;
                */

                /*
                float2 grid = frac(coord * scale);
                float2 dist = abs(grid - 0.5);
                float2 fade = smoothstep(thickness, 0.0, dist);
                return min(fade.x, fade.y);
                */
            }

            // fragments the grid lines so they're recursive
            // * also fades inner grid lines out the further you go (and conversely)
            float4 frag(v2f i) : SV_Target
            {
                float2 worldXZ = i.worldPos.xz;
                float logZoom = log10(abs(_WorldSpaceCameraPos.y));

                float grid = 0.0;
                const int layers = 10;

                for (int l = -2; l < layers; ++l)
                {
                    float scale = _GridScale * pow(10.0, l);
                    float fade = smoothstep(0.0f, _FadeWidth, 1.0f - abs(logZoom - float(l)));
                    float thickness = _GridLineThickness;
                    grid += DrawGrid(worldXZ, scale, thickness) * fade;
                }

                grid = saturate(grid);
                return float4(_GridLineColour.rgb, grid * _GridLineColour.a);

                /*
                float zoom = _WorldSpaceCameraPos.y;
                
                float subGrid = GridLine(i.worldPos.xz, _GridScale, _GridLineThickness);
                float subFade = saturate(1.0 - (zoom / (_CameraDistance * 0.7)));

                float superScale = _GridScale * 10.0;
                float superThickness = _GridLineThickness * 1.5;
                float superGrid = GridLine(i.worldPos.xz, superScale, superThickness);
                float superFade = saturate((zoom - (_CameraDistance * 0.4)) / (_CameraDistance * 0.6));
                
                float combined = saturate((subGrid * subFade) + (superGrid * superFade));

                float4 gridColor = _GridLineColour * combined;
                return gridColor;
                */

                /*
                float distance = length(_WorldSpaceCameraPos - i.worldPos);
                float visibility = saturate(1.0 - distance * 1 / 100.0); // NB: percentage

                float minor = GridLine(i.worldPos.xz, _GridScale * 10.0, _GridLineThickness * 0.5);
                float major = GridLine(i.worldPos.xz, _GridScale, _GridLineThickness);
                
                float gridValue = lerp(minor, major, visibility);
                float alpha = _GridLineColour.a * gridValue * visibility;

                return float4(_GridLineColour.rgb, alpha);
                */
            }
            ENDCG
        }
    }
}