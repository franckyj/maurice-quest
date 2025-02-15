using System.Numerics;
using System.Runtime.InteropServices;

namespace MyTetris;

internal static class ConstantBuffers
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct ConstantBufferChangesEveryFrame
    {
        public Matrix4x4 ViewProjection;

        public ConstantBufferChangesEveryFrame(Matrix4x4 viewProjection)
        {
            ViewProjection = viewProjection;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct ConstantBufferChangesEveryPrim
    {
        public Matrix4x4 WorldMatrix;
        public Vector4 MeshColor;
        public Vector4 DiffuseColor;
        public Vector4 SpecularColor;
        public float SpecularPower;

        public ConstantBufferChangesEveryPrim(
            Matrix4x4 worldMatrix,
            Vector4 meshColor,
            Vector4 diffuseColor,
            Vector4 specularColor,
            float specularPower)
        {
            WorldMatrix = worldMatrix;
            MeshColor = meshColor;
            DiffuseColor = diffuseColor;
            SpecularColor = specularColor;
            SpecularPower = specularPower;
        }
    }
}
