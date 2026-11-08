#if OPENGL
#define PS_SHADERMODEL ps_3_0
#else
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

float Time;
float Opacity;
float BlurRadius;
float RandomBlurBounds;
float NoiseIntensity;
float2 Resolution;
float WaveSpeed = 1.5;
float WaveFrequency = 10.0;
float WaveAmplitude = 0.02;
float3 TintColor = float3(1.0, 0.71, 0.66);

struct VertexShaderOutput
{
    float4 Position : SV_Position;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

float Random(float2 st)
{
    return frac(sin(dot(st, float2(12.9898, 78.233))) * 43758.5453123);
}

float CalculateWaveX(float uvX)
{
    return sin(uvX * WaveFrequency + Time * WaveSpeed) * WaveAmplitude;
}

float CalculateWaveY(float uvY)
{
    return cos(uvY * WaveFrequency + Time * WaveSpeed) * WaveAmplitude;
}

float4 MainPs(VertexShaderOutput input) : COLOR0
{
    float2 uv = input.TexCoord;
    float2 texelSize = 1.0 / Resolution;
    

    float noise = (Random(uv * RandomBlurBounds + Time * 0.1) - 0.5) * NoiseIntensity;

  
    uv.x += CalculateWaveX(uv.y) + noise;
    uv.y += CalculateWaveY(uv.x) + noise;

    float4 colorSum = float4(0, 0, 0, 0);
    float totalWeight = 0.0;

    [unroll]
    for (int x = -2; x <= 2; x++)
    {
        [unroll]
        for (int y = -2; y <= 2; y++)
        {
            float2 sampleOffset = float2(x, y) * texelSize * BlurRadius;
            float2 sampleUv = saturate(uv + sampleOffset);
            
            float weight = 1.0 - length(float2(x, y)) / 3.0;
            
            if (weight > 0.0)
            {
                float4 sampleColor = tex2D(SpriteTextureSampler, sampleUv);
                colorSum += sampleColor * weight;
                totalWeight += weight;
            }
        }
    }

    float4 blurredColor = colorSum / max(totalWeight, 0.0001);
    

    float3 finalColor = lerp(blurredColor.rgb, TintColor, saturate(Opacity));

    return float4(finalColor, blurredColor.a) * input.Color;
}

technique BlurredEnlightenment
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPs();
    }
}