struct VSInput
{
    float3 position: POSITION;
    float3 color: COLOR0;
};

cbuffer transformBuffer : register(b0)
{
    matrix transform;
}

struct VSOutput
{
    float4 position: SV_Position;
    float3 color: COLOR0;
};

VSOutput Main(VSInput input)
{
    VSOutput output = (VSOutput)0;
    output.position = mul(transform, float4(input.position, 1.0));
    output.color = input.color;
    return output;
}
