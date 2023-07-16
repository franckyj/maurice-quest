cbuffer colorBuffer : register(b0)
{
    float4 colors[6];
}

struct PSInput
{
    float4 position: SV_Position;

    uint triangle_id: SV_PrimitiveID;
};

struct PSOutput
{
    float4 color: SV_Target0;
};

PSOutput Main(PSInput input)
{
    PSOutput output = (PSOutput)0;

    output.color = colors[input.triangle_id / 2];
    return output;
}
