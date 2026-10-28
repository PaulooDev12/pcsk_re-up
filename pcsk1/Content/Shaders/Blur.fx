#if OPENGL
#define VS_SHADERMODEL ps_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif


Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

float2 gTexOffset;
float gWeights[5]; 

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = tex2D(SpriteTextureSampler, input.TexCoord) * gWeights[0];

    [unroll]
    for(int i = 1; i < 5; ++i)
    {
        float2 offset = gTexOffset * float(i);
        color += tex2D(SpriteTextureSampler, input.TexCoord + offset) * gWeights[i];
        color += tex2D(SpriteTextureSampler, input.TexCoord + offset) * gWeights[i];
    }
    return color * input.Color;
}
technique blurTechnique
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}

