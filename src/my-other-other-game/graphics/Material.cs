using System.Numerics;
using Vortice.Direct3D11;
using static MyOtherOtherGame.Graphics.ConstantBuffers;

namespace MyOtherOtherGame.Graphics;

internal class Material
{
    public Vector4 MeshColor { get; }
    public Vector4 DiffuseColor { get; }
    public Vector4 SpecularColor { get; }
    public float SpecularExponent { get; }
    public ID3D11ShaderResourceView TextureResourceView { get; }
    public ID3D11VertexShader VertexShader { get; }
    public ID3D11PixelShader PixelShader { get; }

    public Material(
        Vector4 meshColor,
        Vector4 diffuseColor,
        Vector4 specularColor,
        float specularExponent,
        in ID3D11ShaderResourceView textureResourceView,
        in ID3D11VertexShader vertexShader,
        in ID3D11PixelShader pixelShader)
    {
        MeshColor = meshColor;
        DiffuseColor = diffuseColor;
        SpecularColor = specularColor;
        SpecularExponent = specularExponent;
        TextureResourceView = textureResourceView;
        VertexShader = vertexShader;
        PixelShader = pixelShader;
    }

    public void SetupRender(
        in ID3D11DeviceContext context,
        ConstantBufferChangesEveryPrim constantBuffer)
    {
        constantBuffer.MeshColor = MeshColor;
        constantBuffer.SpecularColor = SpecularColor;
        constantBuffer.SpecularPower = SpecularExponent;
        constantBuffer.DiffuseColor = DiffuseColor;

        context.PSSetShaderResource(0, TextureResourceView);
        context.VSSetShader(VertexShader, null, 0);
        context.PSSetShader(PixelShader, null, 0);
        context.VSSetShader(VertexShader);
        context.PSSetShader(PixelShader);
    }
}
