using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace MyGame.Bindable;

internal class VertexShader : IBindable
{
    private readonly ID3D11VertexShader _shader;
    private readonly Blob _shaderBlob;

    public VertexShader(ID3D11Device device, string shaderPath, string entryPoint, string profile)
    {
        _shaderBlob = ShaderCompiler.CompileShader(shaderPath, entryPoint, profile);
        _shader = device.CreateVertexShader(_shaderBlob);
    }

    public void Bind(ID3D11DeviceContext context)
    {
        context.VSSetShader(_shader);
    }
}
