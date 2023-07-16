using Vortice.Direct3D11;

namespace MyGame.Bindable;

internal class VertexBuffer<T> : IBindable where T : unmanaged
{
    private readonly ID3D11Buffer _buffer;
    private readonly int _slot;

    public VertexBuffer(ID3D11Device device, Span<T> vertices, int slot)
    {
        _buffer = device.CreateBuffer(vertices, BindFlags.VertexBuffer);
        _slot = slot;
    }

    public void Bind(ID3D11DeviceContext context)
    {
        unsafe
        {
            context.IASetVertexBuffer(_slot, _buffer, sizeof(T), 0);
        }
    }
}
