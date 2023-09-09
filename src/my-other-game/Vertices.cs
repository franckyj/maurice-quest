using System.Numerics;

namespace MyOtherGame;

internal static class Vertices
{
    public readonly record struct PNTVertex(Vector3 Position, Vector3 Normal, Vector2 TextureCoordinate);
}
