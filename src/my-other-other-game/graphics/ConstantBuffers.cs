using System.Numerics;

namespace MyOtherOtherGame.Graphics;

internal class ConstantBuffers
{
    public struct ConstantBufferNeverChanges
    {
        public Vector4 LightPosition1;
        public Vector4 LightPosition2;
        public Vector4 LightPosition3;
        public Vector4 LightPosition4;
        public Vector4 LightColor;

        public ConstantBufferNeverChanges(
            Vector4 lightPosition1,
            Vector4 lightPosition2,
            Vector4 lightPosition3,
            Vector4 lightPosition4,
            Vector4 lightColor)
        {
            LightPosition1 = lightPosition1;
            LightPosition2 = lightPosition2;
            LightPosition3 = lightPosition3;
            LightPosition4 = lightPosition4;
            LightColor = lightColor;
        }
    }

    public struct ConstantBufferChangeOnResize
    {
        public Matrix4x4 Projection;

        public ConstantBufferChangeOnResize(Matrix4x4 projection)
        {
            Projection = projection;
        }
    }

    public struct ConstantBufferChangesEveryFrame
    {
        public Matrix4x4 View;

        public ConstantBufferChangesEveryFrame(Matrix4x4 view)
        {
            View = view;
        }
    }

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
