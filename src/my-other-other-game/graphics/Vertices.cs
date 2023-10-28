using System.Numerics;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace MyOtherOtherGame.Graphics;

internal static class Vertices
{
    public readonly record struct PNTVertex(Vector3 Position, Vector3 Normal, Vector2 TextureCoordinate);


    public static InputElementDescription[] PNTVertexLayout = new InputElementDescription[]
    {
        new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
        new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, 0, 12, InputClassification.PerVertexData, 0),
        new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 0, 24, InputClassification.PerVertexData, 0)
    };
}
