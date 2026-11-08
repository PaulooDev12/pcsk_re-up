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
    AddressU = Clamp;
    AddressV = Clamp;
};

float4 DropShadowColor = float4(0, 0, 0, 0.5);
float2 ShadowOffset = float2(0.0, 0.05);
float Time = 0.0;
float WindSpeed = 0.0;

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;

};

float4 SampleTexture(sampler2D tex, float2 uv)
{
    if ((uv.x < 0.0 || uv.x > 1.0) || (uv.y < 0.0 || uv.y > 1.0))
    {
        return float4(0, 0, 0, 0);
    }
    else
    {
        return tex2D(tex, uv);
    }
}

float4 MixColor(float4 colA, float4 colB)
{
    return float4((colA.rgb + colB.a * (colB.rgb - colA.rgb)), colA.a + colB.a);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float max_offset = abs(ShadowOffset.x);
    if (abs(ShadowOffset.y) > abs(ShadowOffset.x))
    {
        max_offset = abs(ShadowOffset.y);
    }

    float2 uv = input.TextureCoordinates * (1.0 + 2.0 * max_offset) - max_offset;
    float4 originalColor = SampleTexture(SpriteTextureSampler, uv);
    
    float2 ligthDir = normalize(ShadowOffset);
    float totalDistance = length(ShadowOffset);
    
    float accumulatedShadowAlpha = 0.0;
    const int STEPS = 32;
    
    [unroll]
    for (int i = 1; i <= STEPS; i++)
    {
        float factor = (float) i / (float) STEPS;
        
        float2 shadowUv = uv - (ligthDir * factor * totalDistance);
        
        shadowUv.y = lerp(uv.y, uv.y - (totalDistance * 0.3), factor * 0.5);
        float wave = sin(shadowUv.y * 12.0 + Time * 4.0) * WindSpeed;
        shadowUv.x += wave;
        float4 shadowSample = SampleTexture(SpriteTextureSampler, shadowUv);
        
        if(shadowSample.a > 0.0)
        {
            float attenuation = 1.0 - (factor / (float) STEPS);
            accumulatedShadowAlpha = max(accumulatedShadowAlpha, shadowSample.a * DropShadowColor.a * attenuation);
        }
    }
        
    float4 shadowColor = float4(DropShadowColor.rgb, accumulatedShadowAlpha);
    
    float4 finalColor;
    if (shadowColor.a > 0.0)
    {
        finalColor = MixColor(shadowColor, originalColor);
    }
    else
    {
        finalColor = originalColor;
    }

    return finalColor * input.Color;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}