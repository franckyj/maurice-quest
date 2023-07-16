using Vortice.Direct3D11;
using Vortice.DXGI;

namespace MyGame.Bindable;

internal class IndexBuffer : IBindable
{
    private readonly ID3D11Buffer _buffer;
    private readonly int _indexCount;

    public IndexBuffer(ID3D11Device device, short[] indices)
    {
        _buffer = device.CreateBuffer(indices, BindFlags.IndexBuffer);
        _indexCount = indices.Length;
    }

    public void Bind(ID3D11DeviceContext context)
    {
        context.IASetIndexBuffer(_buffer, Format.R16_UInt, 0);
    }

    public int Count => _indexCount;
}
