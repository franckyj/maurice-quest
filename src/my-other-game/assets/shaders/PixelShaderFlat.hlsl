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

float4 main(PixelShaderFlatInput input) : SV_Target
{
    return float4(1.0f, 1.0f, 0.0f, 0.0f);
    
    //return diffuseTexture.Sample(linearSampler, input.textureUV) * input.diffuseColor;
}
