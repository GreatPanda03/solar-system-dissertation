float DrawGrid(float2 worldXZ, float scale, float thickness)
{
    float2 coord = frac(worldXZ / scale);
    coord = min(coord, 1.0 - coord); // Mirror to get lines on both sides
    float lineX = step(coord.x, thickness);
    float lineY = step(coord.y, thickness);
    return saturate(lineX + lineY);
}

float InfiniteGrid(float2 worldXZ, float cameraY, float baseScale, float lineThickness, float fadeWidth)
{
    // Zoom level based on log10 of camera Y height
    float logZoom = log10(cameraY);
    float level = floor(logZoom);
    float blend = frac(logZoom); // blend between levels

    // Scale per level
    float scaleA = baseScale * pow(10.0, level);
    float scaleB = scaleA * 10.0;

    // Grid lines at both levels
    float gridA = DrawGrid(worldXZ, scaleA, lineThickness);
    float gridB = DrawGrid(worldXZ, scaleB, lineThickness * 1.5);

    // Blend based on camera height
    float gridValue = lerp(gridA, gridB, smoothstep(0.0, fadeWidth, blend));
    return gridValue;
}