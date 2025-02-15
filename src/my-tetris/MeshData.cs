using static MyTetris.Vertices;

namespace MyTetris;

internal class MeshData
{
    public PNTVertex[] Vertices;
    public ushort[] Indices;

    public MeshData(PNTVertex[] vertices, ushort[] indices)
    {
        Vertices = vertices;
        Indices = indices;
    }
}
