﻿using System.Numerics;
using Vortice.Mathematics;

namespace MyGame;

public readonly record struct VertexPositionColor(Vector3 Position, Color4 Color);
public readonly record struct VertexPosition(Vector3 Position);
