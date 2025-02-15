using System.Numerics;
using System.Runtime.CompilerServices;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace MyOtherOtherGame.Graphics;

internal static class Vertices
{
    public readonly record struct PNTVertex(Vector3 Position, Vector3 Normal, Vector2 TextureCoordinate);

    public static int PNTVertexStride = Unsafe.SizeOf<PNTVertex>();

    public static InputElementDescription[] PNTVertexLayout = new InputElementDescription[]
    {
        new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
        new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
        new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 24, 0, InputClassification.PerVertexData, 0)
    };
}
