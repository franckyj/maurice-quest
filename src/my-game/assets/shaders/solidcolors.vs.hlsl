struct VSInput
{
    float3 position: POSITION;
};

cbuffer transformBuffer : register(b0)
{
    matrix transform;
}

cbuffer transformBuffer : register(b1)
{
    matrix model;
}

cbuffer transformBuffer : register(b2)
{
    matrix view;
}

cbuffer transformBuffer : register(b3)
{
    matrix projection;
}

struct VSOutput
{
    float4 position: SV_Position;
    float4 test : POSITION;
};

VSOutput Main(VSInput input)
{
    VSOutput output = (VSOutput)0;
    //float4x4 mvp = mul(mul(projection, view), model);
    //output.position = mul(mvp, float4(input.position, 1.0));
    
    float4x4 mvp = mul(mul(projection, view), model);
    output.position = mul(mvp, float4(input.position, 1.0));
    
    output.test = mul(transform, float4(input.position, 1.0));

    //output.position = mul(transform, float4(input.position, 1.0));
    return output;
}
