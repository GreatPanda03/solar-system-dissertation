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
        _VirtualPosition ("Virtual Position", Vector) = (0,0,0,0)
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
            float4 _VirtualPosition;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 worldPosition : TEXCOORD0;
                float4 screenPosition : TEXCOORD1;
            };

            // defining vertex
            v2f vert(appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.screenPosition = ComputeScreenPos(o.position);
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

            // with two grids, we can make an infinite effect:
            // * Grid 1: covers 10^1, 10^3, 10^5, ... ( odd powers, 2n + 1)
            // * Grid 2: covers 10^2, 10^4, 10^6, ... (even powers, 2n    )
            // * you wouldn't see 10^3 if you were on the scale of 
            //     10^1 for example (and vice versa), so swapping them works!

            // WIP - using only two static grids for now with no infinite effect
            
            float4 frag(v2f i) : SV_Target
            {

                // components (drawing scale, line thickness, grid texture accumulation)
                float scale, thickness, grid;

                // defining thickness in pixels (same for both grids)
                float2 screenUV = i.screenPosition.xy / i.screenPosition.w; 
                float dx = length(ddx(screenUV));
                float dy = length(ddy(screenUV));
                float pixelSize = length(dx) + length(dy);

                // determining the layer and scale (by power of 10)
                float height = length(_WorldSpaceCameraPos.y);
                float2 worldXZ = i.worldPosition.xz / _GridScale;
                
                // float2 worldXZ = (i.worldPosition.xz + _VirtualPosition.xz) / _GridScale;

                float power = log10(height);
                int base_n = floor(power);

                // // odd-power grid
                // float odd_visibility 
                
                // if ((base_n + 1) % 2 == 1) { // it's on the "becoming visible" side
                //     upper_n = ceil(power);
                //     odd_transparency = map()
                //     odd_visibility = saturate(1.0 - height / 10);
                
                // } else { // it's on the "becoming invisible" side

                // }
                
                // odd_thickness = _GridLineThickness * pixelSize;
                // odd_scale = _GridScale * 10;
                // odd_grid = DrawGrid(worldXZ, scale, thickness * 0.1);

                // // even-power grid
                // float even_visibility = saturate(1.0 - height / 10);
                // even_thickness = _GridLineThickness * pixelSize;
                // even_scale = _GridScale * 10;
                // even_grid = DrawGrid(worldXZ, scale, thickness * 0.1);
                
                // * original code
                float visibility = saturate(1.0 - height / 10);
                thickness = _GridLineThickness * 0.005;
                
                scale = _GridScale * 10;
                grid = DrawGrid(worldXZ, scale, thickness * 0.1);
                
                scale = _GridScale;
                grid += DrawGrid(worldXZ, scale, thickness) * visibility;
                
                grid = saturate(grid);

                return float4(_GridLineColour.rgb, grid * _GridLineColour.a);
            }
            ENDCG
        }
    }
}