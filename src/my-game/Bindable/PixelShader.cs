using Vortice.Direct3D11;

namespace MyGame.Bindable;

internal class PixelShader : IBindable
{
    private readonly ID3D11PixelShader _shader;

    public PixelShader(ID3D11Device device, string shaderPath, string entryPoint, string profile)
    {
        var shaderBlob = ShaderCompiler.CompileShader(shaderPath, entryPoint, profile);
        _shader = device.CreatePixelShader(shaderBlob);
    }

    public void Bind(ID3D11DeviceContext context)
    {
        context.PSSetShader(_shader);
    }
}
