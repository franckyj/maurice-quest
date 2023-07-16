using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace MyGame.Drawable;

internal static class Effects
{
    #region solid colors

    public unsafe class SolidColors
    {
        private static SolidColors _instance;

        public static SolidColors GetInstance(ID3D11Device device)
        {
            _instance ??= new SolidColors(device);
            return _instance;
        }

        public ID3D11Buffer ColorBuffer { get; private set; }

        public ID3D11VertexShader VertexShader { get; private set; }
        public ID3D11PixelShader PixelShader { get; private set; }

        public ID3D11InputLayout Layout { get; private set; }

        public SolidColors(ID3D11Device device)
        {
            Span<Color4> colors = stackalloc Color4[]
            {
                new Color4(1.0f, 0.0f, 0.0f, 1.0f),
                new Color4(0.0f, 1.0f, 0.0f, 1.0f),
                new Color4(0.0f, 0.0f, 1.0f, 1.0f),
                new Color4(1.0f, 1.0f, 0.0f, 1.0f),
                new Color4(1.0f, 0.0f, 1.0f, 1.0f),
                new Color4(0.0f, 1.0f, 1.0f, 1.0f)
            };
            ColorBuffer = device.CreateBuffer(colors, BindFlags.ConstantBuffer);

            var vertexShaderBlob = ShaderCompiler.CompileShader("assets/shaders/solidcolors.vs.hlsl", "Main", "vs_5_0");
            VertexShader = device.CreateVertexShader(vertexShaderBlob);

            var pixelShaderBlob = ShaderCompiler.CompileShader("assets/shaders/solidcolors.ps.hlsl", "Main", "ps_5_0");
            PixelShader = device.CreatePixelShader(pixelShaderBlob);

            var inputLayoutDescriptor = new InputElementDescription[]
            {
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
            };
            Layout = device.CreateInputLayout(inputLayoutDescriptor, vertexShaderBlob);
        }
    }

    #endregion

    #region blend colors

    public unsafe class BlendColors
    {
        private static BlendColors _instance;

        public static BlendColors GetInstance(ID3D11Device device)
        {
            _instance ??= new BlendColors(device);
            return _instance;
        }

        public ID3D11VertexShader VertexShader { get; private set; }
        public ID3D11PixelShader PixelShader { get; private set; }

        public ID3D11InputLayout Layout { get; private set; }

        public BlendColors(ID3D11Device device)
        {
            var vertexShaderBlob = ShaderCompiler.CompileShader("assets/shaders/blendcolors.vs.hlsl", "Main", "vs_5_0");
            VertexShader = device.CreateVertexShader(vertexShaderBlob);

            var pixelShaderBlob = ShaderCompiler.CompileShader("assets/shaders/blendcolors.ps.hlsl", "Main", "ps_5_0");
            PixelShader = device.CreatePixelShader(pixelShaderBlob);

            var inputLayoutDescriptor = new InputElementDescription[]
            {
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("COLOR", 0, Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
            };
            Layout = device.CreateInputLayout(inputLayoutDescriptor, vertexShaderBlob);
        }
    }

    #endregion
}
