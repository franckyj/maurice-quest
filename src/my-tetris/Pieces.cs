using System.Numerics;
using Vortice.Direct3D11;
using Vortice.Mathematics;
using static MyTetris.ConstantBuffers;
using static MyTetris.Vertices;

namespace MyTetris;

public enum PieceType
{
    I,
    L, J,
    O,
    T,
    S, Z
}

internal class Piece
{
    public PieceType Type { get; init; }
    public PieceRotation[] RotationsList { get; init; }

    public bool HasRotations => RotationsList.Length > 1;
    public int CurrentRotation = 0;
    public int CurrentOffset = 0;

    public Piece(
        PieceType pieceType,
        PieceRotation[] clockwiseRotations)
    {
        if (clockwiseRotations is null || clockwiseRotations is { Length: 0 })
            throw new ArgumentException("Must declare at least one rotation");

        Type = pieceType;
        RotationsList = clockwiseRotations;

        // TODO put something else here
        // like Width / 2
        CurrentOffset = 5;
    }

    public void RotateClockwise()
    {
        CurrentRotation++;
        if (CurrentRotation >= RotationsList.Length)
            CurrentRotation = 0;
    }

    public int GetNextClockwiseRotation()
    {
        var next = CurrentRotation + 1;
        if (next >= RotationsList.Length)
            next = 0;

        return next;
    }

    public void RotateCounterClockwise()
    {
        CurrentRotation--;
        if (CurrentRotation < 0)
            CurrentRotation = RotationsList.Length - 1;
    }

    public int GetNextCounterClockwiseRotation()
    {
        var next = CurrentRotation - 1;
        if (next < 0)
            next = RotationsList.Length - 1;

        return next;
    }

    public BoardCell[] GetFilledCellsFromCurrentLine(int currentLine, int? wannaBeRotation = null)
    {
        var rotations = RotationsList[wannaBeRotation ?? CurrentRotation];
        var cells = new BoardCell[rotations.Deltas.Length];

        for (int i = 0; i < cells.Length; i++)
        {
            var rotation = rotations.Deltas[i];

            cells[i] = new(
                rotation.Item1 + CurrentOffset,
                rotation.Item2 + currentLine);
        }

        return cells;
    }
}

internal readonly record struct PieceRotation(ValueTuple<int, int>[] Deltas);

internal static class PieceSpawner
{
    public static Piece GetI()
    {
        // rotations are translations from the current line
        // and current offset
        // the '2' is the intersection of the line and the offset
        return new Piece(
            PieceType.I,
            [
                // 1 1 2 1
                new PieceRotation([new(-2, 0), new(-1, 0), new(0, 0), new(1, 0)]),

                // 1
                // 2
                // 1
                // 1
                new PieceRotation([new(0, 1), new(0, 0), new(0, -1), new(0, -2)]),

                //     2
                // 1 1 1 1
                new PieceRotation([new(-2, -1), new(-1, -1), new(0, -1), new(1, -1)]),

                // 1
                // 1 2
                // 1
                // 1
                new PieceRotation([new(-1, 1), new(-1, 0), new(-1, -1), new(-1, -2)]),
            ]);
    }

    public static Piece GetJ()
    {
        return new Piece(
            PieceType.J,
            [
                // 1 2
                // 1 1 1
                new PieceRotation([new(-1, 0), new(-1, -1), new(0, -1), new(1, -1)]),

                //  2 1
                //  1
                //  1
                new PieceRotation([new(0, 0), new(0, -1), new(0, -2), new(1, 0)]),

                //   2
                // 1 1 1
                //     1
                new PieceRotation([new(-1, -1), new(0, -1), new(1, -1), new(1, -2)]),

                //   2
                //   1
                // 1 1
                new PieceRotation([new(-1, -2), new(0, -2), new(0, -1), new(0, 0)]),
            ]);
    }

    public static Piece GetL()
    {
        return new Piece(
            PieceType.L,
            [
                //   2 1
                // 1 1 1
                new PieceRotation([new(-1, -1), new(0, -1), new(1, -1), new(1, 0)]),

                //  2
                //  1
                //  1 1
                new PieceRotation([new(0, 0), new(0, -1), new(0, -2), new(1, -2)]),

                //   2
                // 1 1 1
                // 1
                new PieceRotation([new(-1, -2), new(-1, -1), new(0, -1), new(1, -1)]),

                // 1 2
                //   1
                //   1
                new PieceRotation([new(-1, 0), new(0, 0), new(0, -1), new(0, -2)]),
            ]);
    }

    public static Piece GetO()
    {
        return new Piece(
            PieceType.O,
            [
                // 2 1
                // 1 1
                new PieceRotation([new(0, 0), new(1, 0), new(0, -1), new(1, -1)])
            ]);
    }

    public static Piece GetS()
    {
        return new Piece(
            PieceType.S,
            [
                //   2 1
                // 1 1
                new PieceRotation([new(-1, -1), new(0, -1), new(0, 0), new(1, 0)]),

                //  2
                //  1 1
                //    1
                new PieceRotation([new(0, 0), new(0, -1), new(1, -1), new(1, -2)]),

                //   2
                //   1 1
                // 1 1
                new PieceRotation([new(-1, -2), new(0, -2), new(0, -1), new(1, -1)]),

                // 1 2
                // 1 1
                //   1
                new PieceRotation([new(-1, 0), new(-1, -1), new(0, -1), new(0, -2)]),
            ]);
    }

    public static Piece GetT()
    {
        return new Piece(
            PieceType.T,
            [
                //   2
                // 1 1 1
                new PieceRotation([new(-1, -1), new(0, -1), new(0, 0), new(1, -1)]),

                //   2
                //   1 1
                //   1
                new PieceRotation([new(0, 0), new(0, -1), new(0, -2), new(1, -1)]),

                //   2
                // 1 1 1
                //   1
                new PieceRotation([new(-1, -1), new(0, -1), new(0, -2), new(1, -1)]),

                //   2
                // 1 1
                //   1
                new PieceRotation([new(-1, -1), new(0, 0), new(0, -1), new(0, -2)]),
            ]);
    }

    public static Piece GetZ()
    {
        return new Piece(
            PieceType.Z,
            [
                // 1 2
                //   1 1
                new PieceRotation([new(-1, 0), new(0, 0), new(0, -1), new(1, -1)]),

                //   2 1
                //   1 1
                //   1
                new PieceRotation([new(1, 0), new(1, -1), new(0, -1), new(0, -2)]),

                //   2
                // 1 1
                //   1 1
                new PieceRotation([new(-1, -1), new(0, -1), new(0, -2), new(1, -2)]),

                //   2
                // 1 1
                // 1
                new PieceRotation([new(-1, -2), new(-1, -1), new(0, -1), new(0, 0)]),
            ]);
    }

    public static MeshData CreateBox(in Vector3 size)
    {
        List<PNTVertex> vertices = new();
        List<ushort> indices = new();

        Vector3[] faceNormals = new Vector3[6]
        {
            Vector3.UnitZ,
            new Vector3(0.0f, 0.0f, -1.0f),
            Vector3.UnitX,
            new Vector3(-1.0f, 0.0f, 0.0f),
            Vector3.UnitY,
            new Vector3(0.0f, -1.0f, 0.0f),
        };

        Vector2[] textureCoordinates = new Vector2[4]
        {
            Vector2.UnitX,
            Vector2.One,
            Vector2.UnitY,
            Vector2.Zero,
        };

        Vector3 tsize = size / 2.0f;

        // Create each face in turn.
        int vbase = 0;
        for (int i = 0; i < 6; i++)
        {
            Vector3 normal = faceNormals[i];

            // Get two vectors perpendicular both to the face normal and to each other.
            Vector3 basis = (i >= 4) ? Vector3.UnitZ : Vector3.UnitY;

            Vector3 side1 = Vector3.Cross(normal, basis);
            Vector3 side2 = Vector3.Cross(normal, side1);

            // Six indices (two triangles) per face.
            indices.Add((ushort)(vbase + 0));
            indices.Add((ushort)(vbase + 1));
            indices.Add((ushort)(vbase + 2));

            indices.Add((ushort)(vbase + 0));
            indices.Add((ushort)(vbase + 2));
            indices.Add((ushort)(vbase + 3));

            // Four vertices per face.
            // (normal - side1 - side2) * tsize // normal // t0
            vertices.Add(new PNTVertex(
                Vector3.Multiply(Vector3.Subtract(Vector3.Subtract(normal, side1), side2), tsize),
                Colors.Red,
                normal,
                textureCoordinates[0]
                ));

            // (normal - side1 + side2) * tsize // normal // t1
            vertices.Add(new PNTVertex(
                Vector3.Multiply(Vector3.Add(Vector3.Subtract(normal, side1), side2), tsize),
                Colors.Blue,
                normal,
                textureCoordinates[1]
                ));

            // (normal + side1 + side2) * tsize // normal // t2
            vertices.Add(new PNTVertex(
                Vector3.Multiply(Vector3.Add(normal, Vector3.Add(side1, side2)), tsize),
                Colors.Green,
                normal,
                textureCoordinates[2]
                ));

            // (normal + side1 - side2) * tsize // normal // t3
            vertices.Add(new PNTVertex(
                Vector3.Multiply(Vector3.Subtract(Vector3.Add(normal, side1), side2), tsize),
                normal,
                textureCoordinates[3]
                ));

            vbase += 4;
        }

        return new MeshData([.. vertices], [.. indices]);
    }
}

internal class GameObject
{
    public GameObject(
        Vector3 position,
        Matrix4x4 modelMatrix)
    {
        Position = position;
        ModelMatrix = modelMatrix;

        UpdateModelMatrix();
    }

    public Mesh? Mesh { get; set; }
    public Material? Material { get; set; }

    public Vector3 Position { get; set; }
    public Matrix4x4 ModelMatrix { get; set; }

    public void SetPosition(Vector3 position)
    {
        Position = position;
        UpdateModelMatrix();
    }

    public void UpdateModelMatrix()
    {
        // S * R * T
        ModelMatrix = Matrix4x4
            .CreateScale(1, 1, 1) *
            Matrix4x4.CreateTranslation(Position);
    }

    public virtual void Render(in ID3D11DeviceContext context, in ID3D11Buffer primitiveConstantBuffer)
    {
        if (Mesh == null || Material == null) return;

        var constantBuffer = new ConstantBufferChangesEveryPrim(
            ModelMatrix,
            Vector4.Zero,
            Vector4.Zero,
            Vector4.Zero,
            1.0f);

        // TODO use ref?
        Material.SetupRender(context, ref constantBuffer);
        context.UpdateSubresource(constantBuffer, primitiveConstantBuffer);
        Mesh.Render(context);
    }
}