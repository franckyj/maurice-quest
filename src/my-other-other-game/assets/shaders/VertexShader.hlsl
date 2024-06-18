//********************************************************* 
// 
// Copyright (c) Microsoft. All rights reserved. 
// This code is licensed under the MIT License (MIT). 
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF 
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY 
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR 
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT. 
// 
//*********************************************************

#include "ConstantBuffers.hlsli"

PixelShaderInput main(VertextShaderInput input)
{
    PixelShaderInput output = (PixelShaderInput) 0;

    float4x4 mvp = mul(mul(projection, view), world);
    output.position = mul(mvp, float4(input.position, 1.0f));
    //output.textureUV = input.textureUV;

    // compute view space normal
    float3x3 vw3 = mul((float3x3)view, (float3x3)world);
    output.normal = normalize(mul(vw3, input.normal.xyz));

    // Vertex pos in view space (normalize in pixel shader)
    float4x4 vw = mul(view, world);
    output.vertexToEye = -mul((float4x3) vw, input.position).xyz;

    // Compute view space vertex to light vectors (normalized)
    output.vertexToLight0 = normalize(mul(view, lightPosition[0]).xyz + output.vertexToEye);
    output.vertexToLight1 = normalize(mul(view, lightPosition[1]).xyz + output.vertexToEye);
    output.vertexToLight2 = normalize(mul(view, lightPosition[2]).xyz + output.vertexToEye);
    output.vertexToLight3 = normalize(mul(view, lightPosition[3]).xyz + output.vertexToEye);

    return output;
}