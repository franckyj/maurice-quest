using System.Runtime.CompilerServices;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using static MyTetris.Vertices;

namespace MyTetris;

internal class Mesh
{
    public ID3D11Buffer? VertexBuffer { get; init; } = null;
    public ID3D11Buffer? IndexBuffer { get; init; } = null;
    public int VertexCount { get; init; } = 0;
    public int IndexCount { get; init; } = 0;

    public virtual void Render(in ID3D11DeviceContext context)
    {
        if (VertexBuffer == null || IndexBuffer == null) return;

        int stride = Unsafe.SizeOf<PNTVertex>();
        int offset = 0;

        context.IASetVertexBuffer(0, VertexBuffer, stride, offset);

        context.IASetIndexBuffer(IndexBuffer, Vortice.DXGI.Format.R16_UInt, 0);
        context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        context.DrawIndexed(IndexCount, 0, 0);
    }
}
