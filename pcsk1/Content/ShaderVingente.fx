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

float u_time;
float u_intensidade;

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 uv = input.TextureCoordinates;

    float peso = uv.x * uv.y;
    float influence = peso - u_intensidade * u_intensidade * 0.65;

    uv.x += sin(uv.y * 20.0 + u_time * 5.0) * u_intensidade * peso * influence;

    float4 texColor = tex2D(SpriteTextureSampler, uv);
    return texColor * input.Color;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}