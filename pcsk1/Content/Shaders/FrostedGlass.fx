#if OPENGL
#define PS_SHADERMODEL ps_3_0
#else
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

float2 u_resolution;

float u_blurRadius;
float u_noiseIntensity;
float u_tintOpacity;

float3 u_tintColor = float3(0.95, 0.97, 1.0);

struct PixelInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 UV : TEXCOORD0;
};

// Função pseudoaleatoria portada do GLSL
float Random(float2 st)
{
    return frac(
        sin(dot(st, float2(12.9898, 78.233)))
        * 43758.5453123
    );
}

float4 MainPS(PixelInput input) : COLOR
{
    float2 uv = input.UV;

    // dimensao de um texel em coordenadas UV
    float2 texelSize = 1.0 / u_resolution;

    // Ruido para simular irregularidades no vidro
    float noise = (
        Random(uv * 1500.0) - 0.5
    ) * u_noiseIntensity;

    float4 colorSum = float4(0, 0, 0, 0);
    float totalWeight = 0.0;

    // kernel 5x5
    [unroll]
    for (int x = -2; x <= 2; x++)
    {
        [unroll]
        for (int y = -2; y <= 2; y++)
        {
            float2 sampleOffset =
                float2(x, y) * texelSize * u_blurRadius;

            float2 sampleUV =
                uv + sampleOffset + float2(noise, noise);

            sampleUV = saturate(sampleUV);

            // peso baseado na distancia ao centro
            float weight =
                1.0 - length(float2(x, y)) / 2.0;

            if (weight > 0.0)
            {
                float4 sampleColor = tex2D(
                    SpriteTextureSampler,
                    sampleUV
                );

                colorSum += sampleColor * weight;
                totalWeight += weight;
            }
        }
    }

    // normaliza a media ponderada
    float4 blurredColor =
        colorSum / max(totalWeight, 0.0001);

    float3 finalColor = lerp(
        blurredColor.rgb,
        u_tintColor,
        saturate(u_tintOpacity)
    );


    return float4(finalColor, 1.0) * input.Color;
}

technique FrostedGlass
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
