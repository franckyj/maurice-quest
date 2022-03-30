using System.Numerics;
using Vortice.Mathematics;

namespace MyGame;

public readonly record struct VertexPositionColor(Vector3 Position, Color4 Color);
//public readonly struct VertexPositionColor
//{
//    public readonly Vector3 Position;
//    public readonly Color4 Color;

//    public VertexPositionColor(Vector3 position, Color4 color)
//    {
//        Position = position;
//        Color = color;
//    }
//}