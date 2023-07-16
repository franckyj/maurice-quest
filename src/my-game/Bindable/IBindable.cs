using Vortice.Direct3D11;

namespace MyGame.Bindable;

internal interface IBindable
{
    void Bind(ID3D11DeviceContext context);
}
