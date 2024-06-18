using System.Numerics;
using System.Runtime.CompilerServices;
using MyOtherOtherGame.Graphics;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using static MyOtherOtherGame.Graphics.ConstantBuffers;
using static MyOtherOtherGame.Graphics.Vertices;

namespace MyOtherOtherGame.Voxels;

internal partial struct Chunk
{
    public const float BLOCK_RENDER_SIZE = 0.5f;

    private int _indexCount;
    private List<PNTVertex> _vertices;
    private List<ushort> _indices;

    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public Vector3 Scale { get; set; } = Vector3.One;

    public Matrix4x4 ModelMatrix { get; private set; }

    public void UpdateModelMatrix()
    {
        // S * R * T
        ModelMatrix = Matrix4x4.CreateScale(Scale) *
                      Matrix4x4.CreateRotationX(Rotation.X) *
                      Matrix4x4.CreateRotationY(Rotation.Y) *
                      Matrix4x4.CreateRotationZ(Rotation.Z) *
                      Matrix4x4.CreateTranslation(Position);
    }

    public ID3D11Buffer? VertexBuffer { get; private set; } = null;
    public ID3D11Buffer? IndexBuffer { get; private set; } = null;

    public void MeshCunk(Direct3D d3d)
    {
        if (!IsAllocated || !IsDirty()) return;

        _vertices.Clear();
        _indices.Clear();

        for (int i = 0; i < _blocks.Length; i++)
        {
            if (!_blocks[i].IsActive) continue;

            // >> dividing
            // & remainder

            var x = (i & World.WORLD_TO_LOCAL_MASK) + WorldPosition.X;
            var y = (i >> World.CHUNK_SHIFT & World.WORLD_TO_LOCAL_MASK) + WorldPosition.Y;
            var z = (i >> World.CHUNK_SHIFT >> World.CHUNK_SHIFT & World.WORLD_TO_LOCAL_MASK) + WorldPosition.Z;
            CreateOffsetedCubeMesh(_vertices, _indices, x, y, z);
            //CreateOffsetedCubeMesh2(_vertices, _indices, x, y, z);
        }

        var device = d3d.Device;

        _indexCount = _indices.Count;
        VertexBuffer = device.CreateBuffer(_vertices.ToArray(), BindFlags.VertexBuffer);
        IndexBuffer = device.CreateBuffer(_indices.ToArray(), BindFlags.IndexBuffer);

        UnsetDirty();
    }

    public readonly void Render(
        in ID3D11DeviceContext context,
        ID3D11Buffer primitiveConstantBuffer)
    {
        if (VertexBuffer == null || IndexBuffer == null) return;

        int stride = Unsafe.SizeOf<PNTVertex>();
        int offset = 0;

        var constantBuffer =
            new ConstantBufferChangesEveryPrim(
                ModelMatrix,
                Vector4.Zero,
                Vector4.Zero,
                Vector4.Zero,
                1.0f);

        // TODO use ref?
        //Material.SetupRender(context, constantBuffer);
        // Meterial stuff
        constantBuffer.MeshColor = new Vector4(0.8f, 0.8f, 0.8f, .5f);
        constantBuffer.SpecularColor = new Vector4(0.8f, 0.8f, 0.8f, .5f);
        constantBuffer.SpecularPower = 15.0f;
        constantBuffer.DiffuseColor = new Vector4(0.8f, 0.8f, 0.8f, .5f);

        context.UpdateSubresource(constantBuffer, primitiveConstantBuffer);

        context.IASetVertexBuffer(0, VertexBuffer, stride, offset);

        context.IASetIndexBuffer(IndexBuffer, Vortice.DXGI.Format.R16_UInt, 0);
        context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        context.DrawIndexed(_indexCount, 0, 0);
    }

    private static void CreateOffsetedCubeMesh(
        List<PNTVertex> vertices, List<ushort> indices,
        int offsetX, int offsetY, int offsetZ

        // faces
        //bool top, bool bottom, bool left, bool right, bool front, bool back
        )
    {
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

        var lenght = vertices.Count;
        Vector3 tsize = Vector3.One / 2.0f;

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
            indices.Add((ushort)(vbase + 0 + lenght));
            indices.Add((ushort)(vbase + 1 + lenght));
            indices.Add((ushort)(vbase + 2 + lenght));

            indices.Add((ushort)(vbase + 0 + lenght));
            indices.Add((ushort)(vbase + 2 + lenght));
            indices.Add((ushort)(vbase + 3 + lenght));

            // Four vertices per face.
            // (normal - side1 - side2) * tsize // normal // t0
            vertices.Add(new PNTVertex(
                Vector3.Add(
                    Vector3.Multiply(Vector3.Subtract(Vector3.Subtract(normal, side1), side2), tsize),
                    new Vector3(offsetX, offsetY, offsetZ)
                ),
                normal,
                textureCoordinates[0]
                ));

            // (normal - side1 + side2) * tsize // normal // t1
            vertices.Add(new PNTVertex(
                Vector3.Add(
                    Vector3.Multiply(Vector3.Add(Vector3.Subtract(normal, side1), side2), tsize),
                    new Vector3(offsetX, offsetY, offsetZ)
                ),
                normal,
                textureCoordinates[1]
                ));

            // (normal + side1 + side2) * tsize // normal // t2
            vertices.Add(new PNTVertex(
                Vector3.Add(
                    Vector3.Multiply(Vector3.Add(normal, Vector3.Add(side1, side2)), tsize),
                    new Vector3(offsetX, offsetY, offsetZ)
                ),
                normal,
                textureCoordinates[2]
                ));

            // (normal + side1 - side2) * tsize // normal // t3
            vertices.Add(new PNTVertex(
                Vector3.Add(
                    Vector3.Multiply(Vector3.Subtract(Vector3.Add(normal, side1), side2), tsize),
                    new Vector3(offsetX, offsetY, offsetZ)
                ),
                normal,
                textureCoordinates[3]
                ));

            vbase += 4;
        }
    }

    private static void CreateOffsetedCubeMesh2(
        List<PNTVertex> vertices, List<ushort> indices,
        ushort offsetX, ushort offsetY, ushort offsetZ

        // faces
        //bool top, bool bottom, bool left, bool right, bool front, bool back
        )
    {
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

        var lenght = vertices.Count;

        vertices.Add(new PNTVertex(new Vector3(offsetX - BLOCK_RENDER_SIZE, offsetY - BLOCK_RENDER_SIZE, offsetZ + BLOCK_RENDER_SIZE), faceNormals[0], Vector2.UnitX));
        vertices.Add(new PNTVertex(new Vector3(offsetX + BLOCK_RENDER_SIZE, offsetY - BLOCK_RENDER_SIZE, offsetZ + BLOCK_RENDER_SIZE), faceNormals[1], Vector2.UnitX));
        vertices.Add(new PNTVertex(new Vector3(offsetX - BLOCK_RENDER_SIZE, offsetY + BLOCK_RENDER_SIZE, offsetZ + BLOCK_RENDER_SIZE), faceNormals[0], Vector2.UnitX));
        vertices.Add(new PNTVertex(new Vector3(offsetX + BLOCK_RENDER_SIZE, offsetY + BLOCK_RENDER_SIZE, offsetZ + BLOCK_RENDER_SIZE), faceNormals[0], Vector2.UnitX));
        vertices.Add(new PNTVertex(new Vector3(offsetX - BLOCK_RENDER_SIZE, offsetY - BLOCK_RENDER_SIZE, offsetZ - BLOCK_RENDER_SIZE), faceNormals[0], Vector2.UnitX));
        vertices.Add(new PNTVertex(new Vector3(offsetX + BLOCK_RENDER_SIZE, offsetY - BLOCK_RENDER_SIZE, offsetZ - BLOCK_RENDER_SIZE), faceNormals[0], Vector2.UnitX));
        vertices.Add(new PNTVertex(new Vector3(offsetX - BLOCK_RENDER_SIZE, offsetY + BLOCK_RENDER_SIZE, offsetZ - BLOCK_RENDER_SIZE), faceNormals[0], Vector2.UnitX));
        vertices.Add(new PNTVertex(new Vector3(offsetX + BLOCK_RENDER_SIZE, offsetY + BLOCK_RENDER_SIZE, offsetZ - BLOCK_RENDER_SIZE), faceNormals[0], Vector2.UnitX));

        Span<ushort> localIndices = stackalloc ushort[]
        {
            0, 2, 1, 2, 3, 1,
            1, 3, 5, 3, 7, 5,
            2, 6, 3, 3, 6, 7,
            4, 5, 7, 4, 7, 6,
            0, 4, 2, 2, 4, 6,
            0, 1, 4, 1, 5, 4
        };
        foreach (var i in localIndices)
            indices.Add((ushort)(i + lenght));
    }
}
