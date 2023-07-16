struct VSInput
{
    float3 position: POSITION;
};

cbuffer transformBuffer : register(b0)
{
    matrix transform;
}

struct VSOutput
{
    float4 position: SV_Position;
};

VSOutput Main(VSInput input)
{
    VSOutput output = (VSOutput)0;
    output.position = mul(transform, float4(input.position, 1.0));
    return output;
}
