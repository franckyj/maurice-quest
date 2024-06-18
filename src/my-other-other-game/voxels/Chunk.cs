using System.Numerics;

namespace MyOtherOtherGame.Voxels;

internal partial struct Chunk
{
    public const int CHUNK_SIZE = 32;
    public const int CHUNK_SIZE_SQUARED = CHUNK_SIZE * CHUNK_SIZE;

    private bool _dirty;
    private Block[] _blocks;
    private World _world;

    // global map position of this chunk
    public WorldPosition WorldPosition { get; private set; }

    // indexes of this chunk in the world
    public LocalPosition LocalPosition { get; private set; }

    public bool IsAllocated { get; private set; }

    public readonly Block[] Blocks => _blocks;

    public Chunk()
    {
    }

    public void SetDirty() => _dirty = true;
    public void UnsetDirty() => _dirty = false;
    public readonly bool IsDirty() => _dirty;

    public readonly Block GetBlockAt(LocalPosition position)
    {
        var index = position.X + (position.Y * CHUNK_SIZE) + (position.Z * CHUNK_SIZE_SQUARED);
        return _blocks[index];
    }

    public readonly void SetBlockAt(LocalPosition position)
    {
        var index = position.X + (position.Y * CHUNK_SIZE) + (position.Z * CHUNK_SIZE_SQUARED);
        _blocks[index] = new Block();
    }

    public readonly void SetBlockAt(WorldPosition position)
    {
        var localPosition = WorldToLocal(position);
        var index = localPosition.X + (localPosition.Y * CHUNK_SIZE) + (localPosition.Z * CHUNK_SIZE_SQUARED);
        _blocks[index] = new Block() { IsActive = true };
    }

    public void Reset(World world, LocalPosition position)
    {
        _world = world;
        WorldPosition = new WorldPosition(
            position.X * CHUNK_SIZE,
            position.Y * CHUNK_SIZE,
            position.Z * CHUNK_SIZE);
        //LocalPosition = WorldToLocal(localPosition);
        LocalPosition = position;

        _blocks = new Block[CHUNK_SIZE * CHUNK_SIZE_SQUARED];

        //for (int i = 0; i < _blocks.Length; i++)
        //{
        //    _blocks[i] = new Block();
        //}

        // some default values
        Scale = new Vector3(1.0f, 1.0f, 1.0f);

        // make sure we're correcly positioned
        Position = new Vector3(WorldPosition.X, WorldPosition.Y, WorldPosition.Z);

        UpdateModelMatrix();

        _vertices = new(CHUNK_SIZE);
        _indices = new(CHUNK_SIZE);

        IsAllocated = true;

        SetDirty();
    }

    public static LocalPosition WorldToLocal(WorldPosition position) =>
        new(
            position.X & World.WORLD_TO_LOCAL_MASK,
            position.Y & World.WORLD_TO_LOCAL_MASK,
            position.Z & World.WORLD_TO_LOCAL_MASK);

    //public override void Render(in ID3D11DeviceContext context, in ID3D11Buffer primitiveConstantBuffer)
    //{
    //    if (Mesh == null || Material == null) return;

    //    var constantBuffer = new ConstantBufferChangesEveryPrim(
    //        ModelMatrix,
    //        Vector4.Zero,
    //        Vector4.Zero,
    //        Vector4.Zero,
    //        1.0f);

    //    // TODO use ref?
    //    Material.SetupRender(context, constantBuffer);
    //    context.UpdateSubresource(constantBuffer, primitiveConstantBuffer);
    //    Mesh.Render(context);
    //}

    //public class ChunkMesh : MeshObject
    //{
    //    private ID3D11Buffer _abc;

    //    public ChunkMesh(in ID3D11Device device, in Block[] blocks)
    //    {
    //        List<ushort> indices = new();
    //        List<PNTVertex> vertices = new();

    //        for (int i = 0; i < blocks.Length; i++)
    //        {
    //            var x = i % CHUNK_SIZE;
    //            var y = (i / CHUNK_SIZE) % CHUNK_SIZE;
    //            var z = i / (CHUNK_SIZE * CHUNK_SIZE);

    //            if (!blocks[i].IsActive) continue;

    //            //Console.WriteLine($"X: {x}, Y: {y}, Z: {z}");

    //            CreateOffsetedCubeMesh(
    //                vertices, indices,
    //                (ushort)x, (ushort)y, (ushort)z,

    //                // should we render all the faces
    //                true, true, true, true, true, true
    //            );
    //        }

    //        VertexCount = vertices.Count;
    //        VertexBuffer = device.CreateBuffer(vertices.ToArray(), BindFlags.VertexBuffer);

    //        IndexCount = indices.Count;
    //        IndexBuffer = device.CreateBuffer(indices.ToArray(), BindFlags.IndexBuffer);
    //    }

    //    public override void Render(in ID3D11DeviceContext context)
    //    {
    //        if (VertexBuffer == null || IndexBuffer == null) return;

    //        const int offset = 0;

    //        context.IASetVertexBuffer(0, VertexBuffer, PNTVertexStride, offset);

    //        context.IASetIndexBuffer(IndexBuffer, Vortice.DXGI.Format.R16_UInt, 0);
    //        context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

    //        context.DrawIndexed(IndexCount, 0, 0);
    //    }
}
