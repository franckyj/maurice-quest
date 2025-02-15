#include "ConstantBuffers.hlsli"

PixelShaderInput main(VertextShaderInput input)
{
    PixelShaderInput output = (PixelShaderInput) 0;

    float4x4 mvp = mul(viewProjection, world);
    output.position = mul(mvp, float4(input.position, 1.0f));
    output.textureUV = input.textureUV;
    output.color = input.color;

    return output;
}