using MyOtherOtherGame.Graphics;
using Vortice.Direct3D11;

namespace MyOtherOtherGame.Voxels;

internal class WorldRenderer
{
    private readonly World _world;
    private readonly Direct3D _d3d;

    public WorldRenderer(Direct3D d3d)
    {
        _world = new World(new WorldPosition(64, 64, 64));
        _d3d = d3d;

        // Create hills
        for (var x = 0; x < 64; x++)
            for (var z = 0; z < 64; z++)
            {
                var height = (1 + (int)((MathF.Sin(x / 8.0f) + 1) * 4) + (int)((MathF.Sin(z / 4.0f) + 1) * 4));

                for (var y = 0; y < height; y++)
                    _world.AddVoxel(new WorldPosition(x, y, z)); // , 1
            }
    }

    public void Render(ID3D11Buffer constantBufferChangesEveryPrim)
    {
        var length = _world.Chunks.Length;
        for (int i = 0; i < length; i++)
        {
            ref var chunk = ref _world.Chunks[i];

            if (!chunk.IsAllocated) continue;

            chunk.MeshCunk(_d3d);
            chunk.Render(_d3d.DeviceContext, constantBufferChangesEveryPrim);
        }
    }
}
