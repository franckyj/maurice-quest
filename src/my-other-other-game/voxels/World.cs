namespace MyOtherOtherGame.Voxels;

internal sealed class World
{
    public const ulong WORLD_SIZE_IN_CHUNKS = 1;
    public const ulong WORLD_SIZE_IN_CHUNKS_SQUARED = WORLD_SIZE_IN_CHUNKS * WORLD_SIZE_IN_CHUNKS;
    public const ulong WORLD_SIZE_IN_CHUNKS_CUBED = WORLD_SIZE_IN_CHUNKS_SQUARED * WORLD_SIZE_IN_CHUNKS;
    public const int WORLD_TO_LOCAL_MASK = 0b11111;
    public const int CHUNK_SHIFT = 5;

    public Chunk[] Chunks { get; init; }
    //private readonly ChunkMesh[] _chunkMeshes;

    public int WorldSizeX { get; init; }
    public int WorldSizeY { get; init; }
    public int WorldSizeZ { get; init; }

    public int ChunkSizeX { get; init; }
    public int ChunkSizeY { get; init; }
    public int ChunkSizeZ { get; init; }

    public World(WorldPosition size)
    {
        WorldSizeX = size.X;
        WorldSizeY = size.Y;
        WorldSizeZ = size.Z;

        ChunkSizeX = WorldSizeX >> CHUNK_SHIFT;
        ChunkSizeY = WorldSizeY >> CHUNK_SHIFT;
        ChunkSizeZ = WorldSizeZ >> CHUNK_SHIFT;

        Chunks = new Chunk[ChunkSizeX * ChunkSizeY * ChunkSizeZ];
    }

    public bool OutOfBounds(WorldPosition position) =>
        position.X >= WorldSizeX ||
        position.Y >= WorldSizeY ||
        position.Z >= WorldSizeZ;

    public void AddVoxel(WorldPosition position) // , byte index
    {
        if (OutOfBounds(position))
            return;

        // Get and initialise (if null) the chunk
        var chunk = InitializeChunk(position);

        // Add a voxel to the chunk
        chunk.SetBlockAt(position); // index
    }

    public Chunk InitializeChunk(WorldPosition position)
    {
        // i, j, k are voxel positions within a chunk
        // f, g, h are chunk positions
        // x, y, z are global voxel positions
        //var chunkPosition = Chunk.WorldToLocal(position);
        var chunkPosition = new LocalPosition(
            position.X >> CHUNK_SHIFT,
            position.Y >> CHUNK_SHIFT,
            position.Z >> CHUNK_SHIFT);
        var index = chunkPosition.X + (chunkPosition.Y * ChunkSizeY) + (chunkPosition.Z * ChunkSizeZ * ChunkSizeZ);
        ref var chunk = ref Chunks[index];

        // If already initialised, return it
        if (chunk.IsAllocated) return chunk;

        // Initialise it and return it
        chunk.Reset(this, chunkPosition);

        // effectively dividing by WORLD_SIZE_IN_CHUNKS
        //var f = position.X >> CHUNK_SHIFT;
        //var g = position.Y >> CHUNK_SHIFT;
        //var h = position.Z >> CHUNK_SHIFT;
        // Create a meshing wrapper for this chunk
        //_chunkMeshes[index] = new ChunkMesh(this, c);

        //Assert(c->voxels != null);

        return chunk;
    }
}
