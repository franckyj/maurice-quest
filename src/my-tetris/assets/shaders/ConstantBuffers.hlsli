cbuffer ConstantBufferChangesEveryFrame : register(b0)
{
    matrix viewProjection;
};

cbuffer ConstantBufferChangesEveryPrim : register (b1)
{
    matrix world;
    float4 meshColor;
    float4 diffuseColor;
    float4 specularColor;
    float specularExponent;
};

struct VertextShaderInput
{
    float3 position : POSITION;
    float4 color : COLOR;
    float4 normal : NORMAL;
    float2 textureUV : TEXCOORD0;
};

struct PixelShaderInput
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
    float2 textureUV : TEXCOORD0;
};
