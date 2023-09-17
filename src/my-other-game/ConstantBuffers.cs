using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace MyOtherGame;

internal class ConstantBuffers
{
    public readonly struct PNTVertex
    {
        public PNTVertex(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
        }

        public readonly Vector3 Position { get; }
        public readonly Vector3 Normal { get; }
        public readonly Vector2 TextureCoordinate { get; }
    }

    public static InputElementDescription[] PNTVertexLayout = new InputElementDescription[]
    {
        new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
        new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, 0, 12, InputClassification.PerVertexData, 0),
        new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 0, 24, InputClassification.PerVertexData, 0)
    };

    public struct ConstantBufferNeverChanges
    {
        public Vector4 LightPosition1;
        public Vector4 LightPosition2;
        public Vector4 LightPosition3;
        public Vector4 LightPosition4;
        public Vector4 LightColor;

        ////public ConstantBufferNeverChanges(Vector4[] lightPosition, Vector4 lightColor)
        ////{
        ////    LightPosition = lightPosition;
        ////    LightColor = lightColor;
        ////}

        //public ConstantBufferNeverChanges(
        //    Vector4 lightPosition1,
        //    Vector4 lightPosition2,
        //    Vector4 lightPosition3,
        //    Vector4 lightPosition4,
        //    Vector4 lightColor)
        //{
        //    LightPosition1 = lightPosition1;
        //    LightPosition2 = lightPosition2;
        //    LightPosition3 = lightPosition3;
        //    LightPosition4 = lightPosition4;
        //    LightColor = lightColor;
        //}

        ////public readonly fixed Vector4 LightPosition[4] { get; }
        //public readonly Vector4 LightPosition1 { get; }
        //public readonly Vector4 LightPosition2 { get; }
        //public readonly Vector4 LightPosition3 { get; }
        //public readonly Vector4 LightPosition4 { get; }
        //public readonly Vector4 LightColor { get; }
    }

    public struct ConstantBufferChangeOnResize
    {
        public Matrix4x4 Projection;

        //public ConstantBufferChangeOnResize(Matrix4x4 projection)
        //{
        //    Projection = projection;
        //}

        //public readonly Matrix4x4 Projection { get; }
    }

    public struct ConstantBufferChangesEveryFrame
    {
        public Matrix4x4 View;

        //public ConstantBufferChangesEveryFrame(Matrix4x4 view)
        //{
        //    View = view;
        //}

        //public readonly Matrix4x4 View { get; }
    }

    public struct ConstantBufferChangesEveryPrim
    {
        public Matrix4x4 WorldMatrix;
        public Vector4 MeshColor;
        public Vector4 DiffuseColor;
        public Vector4 SpecularColor;
        public float SpecularPower;

        //public ConstantBufferChangesEveryPrim(
        //    Matrix4x4 worldMatrix,
        //    Vector4 meshColor,
        //    Vector4 diffuseColor,
        //    Vector4 specularColor,
        //    float specularPower)
        //{
        //    WorldMatrix = worldMatrix;
        //    MeshColor = meshColor;
        //    DiffuseColor = diffuseColor;
        //    SpecularColor = specularColor;
        //    SpecularPower = specularPower;
        //}

        //public ConstantBufferChangesEveryPrim(Matrix4x4 worldMatrix)
        //{
        //    WorldMatrix = worldMatrix;
        //}

        //public readonly Matrix4x4 WorldMatrix { get; }
        //public Vector4 MeshColor { get; set; }
        //public Vector4 DiffuseColor { get; set; }
        //public Vector4 SpecularColor { get; set; }
        //public float SpecularPower { get; set; }
    }
}
