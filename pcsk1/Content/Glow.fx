#if OPENGL
#define PS_SHADERMODEL ps_3_0
#else
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

texture SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

float u_time;
float u_intensidade;
float3 u_glowColor = float3(0.2, 0.7, 1.0);
float u_initialRay;
float u_finalRay;

struct PixelInput
{
    float4 Position: SV_POSITION;
    float2 UV : TEXCOORD0;
    float4 Color : COLOR0;
};

float4 MainPS(PixelInput input) : COLOR
{
    float2 uv = input.UV;
    float2 center = float2(0.5, 0.5);
    float distance = length(uv - center);
    float progress = frac(u_time * 0.4);
    float ray = lerp(u_initialRay, u_finalRay, progress);
    float smoothness = 0.12;
    float ring = 1.0 - smoothstep(
    0.0,
    smoothness,
    abs(distance - ray)
    );

    float alpha = ring * (1.0 - progress);
    alpha *= saturate(u_intensidade);
    
    float3 color = u_glowColor * alpha;
    
    return float4(color, alpha) * input.Color;
}

technique Glow
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}