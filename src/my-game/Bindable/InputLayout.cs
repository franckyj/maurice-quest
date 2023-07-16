using System.Reflection.Metadata;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace MyGame.Bindable;

internal class InputLayout : IBindable
{
    private readonly ID3D11InputLayout _inputLayout;

    public InputLayout(ID3D11Device device, Blob vertexShader, params InputElementDescription[] layout)
    {
        var inputLayoutDescriptor = new[]
            {
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                // new InputElementDescription("COLOR", 0, Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
            };
        //_inputLayout = device.CreateInputLayout(inputLayoutDescriptor, _vertexShaderBlob);
    }

    public void Bind(ID3D11DeviceContext context)
    {
        throw new NotImplementedException();
    }
}
