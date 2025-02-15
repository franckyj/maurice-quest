using System.Numerics;
using System.Runtime.CompilerServices;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace MyTetris;

internal static class Vertices
{
    public readonly record struct PNTVertex
    {
        public readonly Vector3 Position;
        public readonly Color4 Color;
        public readonly Vector3 Normal;
        public readonly Vector2 TextureCoordinate;

        public PNTVertex(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
            : this(position, new Color4(1.0f, 1.0f, 1.0f, 1.0f), normal, textureCoordinate)
        { }

        public PNTVertex(Vector3 position, Color4 color, Vector3 normal, Vector2 textureCoordinate)
        {
            Position = position;
            Color = color;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
        }
    }


    public static int PNTVertexStride = Unsafe.SizeOf<PNTVertex>();

    public static InputElementDescription[] PNTVertexLayout = new InputElementDescription[]
    {
        new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
        new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, 12, 0, InputClassification.PerVertexData, 0),
        new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, 24, 0, InputClassification.PerVertexData, 0),
        new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 36, 0, InputClassification.PerVertexData, 0)
    };
}
