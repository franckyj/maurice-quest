using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using MyOtherOtherGame.Assets;
using MyOtherOtherGame.Graphics;
using Vortice.Direct3D11;

using static MyOtherOtherGame.Graphics.ConstantBuffers;
using static MyOtherOtherGame.Graphics.GameObjects;
using static MyOtherOtherGame.Graphics.Vertices;

namespace MyOtherOtherGame;

internal class MyGame : D3D11Application
{
    private readonly Camera _camera;

    // buffers
    private ID3D11Buffer _constantBufferNeverChanges;
    private ID3D11Buffer _constantBufferChangeOnResize;
    private ID3D11Buffer _constantBufferChangesEveryFrame;
    private ID3D11Buffer _constantBufferTest;
    private ID3D11Buffer _constantBufferChangesEveryPrim;

    private ID3D11SamplerState _samplerLinear;
    private ID3D11VertexShader _vertexShader;
    private ID3D11PixelShader _pixelShader;
    private ID3D11InputLayout _vertexLayout;

    // test objects
    private GameObject[] _gameObjects;

    public MyGame(
        ILogger<D3D11Application> logger,
        string title)
        : base(logger, title)
    {
        _camera = new Camera();
        _camera.SetProjParams(
            MathF.PI / 4.0f,
            (float)_window.ClientWindowWidth / (float)_window.ClientWindowHeight,
            0.1f,
            100);
        _camera.SetViewParams(new Vector3(25, 25, 25), Vector3.Zero, Vector3.UnitY);

        CreateGameDeviceResources();

        // test objects
        _gameObjects = new GameObject[1]
        {
            //new SphereObject(Vector3.Zero, 2.0f)
            //{
            //    Mesh = new Meshes.SphereMesh(_d3d.Device, 13),
            //    Material = new Material(Vector4.Zero, Vector4.Zero, Vector4.Zero, 0.0f, null, _vertexShader, _pixelShader)
            //},
            //new SphereObject(Vector3.Zero, 5.0f)
            //{
            //    Mesh = new Meshes.SphereMesh(_d3d.Device, 13),
            //    Material = new Material(Vector4.Zero, Vector4.Zero, Vector4.Zero, 0.0f, null, _vertexShader, _pixelShader)
            //},
            new CubeObject(Vector3.Zero, 3.0f)
            {
                Mesh = new Meshes.CubeMesh(_d3d.Device),
                Material = new Material(Vector4.Zero, Vector4.Zero, Vector4.Zero, 0.0f, null, _vertexShader, _pixelShader)
            }
        };

        BindKeyboard();
    }

    public void CreateGameDeviceResources()
    {
        var device = _d3d.Device;

        // create the constant buffers
        var size = (Unsafe.SizeOf<ConstantBufferNeverChanges>() + 15) / 16 * 16;
        //var size = Unsafe.SizeOf<ConstantBufferNeverChanges>();
        _constantBufferNeverChanges = device.CreateBuffer(
            size,
            BindFlags.ConstantBuffer,
            ResourceUsage.Default,
            CpuAccessFlags.None);

        size = (Unsafe.SizeOf<ConstantBufferChangeOnResize>() + 15) / 16 * 16;
        //size = Unsafe.SizeOf<ConstantBufferChangeOnResize>();
        _constantBufferChangeOnResize = device.CreateBuffer(
            //size,
            Unsafe.SizeOf<Matrix4x4>(),
            BindFlags.ConstantBuffer,
            ResourceUsage.Default,
            CpuAccessFlags.None);

        size = (Unsafe.SizeOf<ConstantBufferChangesEveryFrame>() + 15) / 16 * 16;
        //size = Unsafe.SizeOf<ConstantBufferChangesEveryFrame>();
        _constantBufferChangesEveryFrame = device.CreateBuffer(
            //size,
            Unsafe.SizeOf<Matrix4x4>(),
            BindFlags.ConstantBuffer,
            ResourceUsage.Default,
            CpuAccessFlags.None);

        _constantBufferTest = device.CreateBuffer(
            //size,
            Unsafe.SizeOf<Matrix4x4>(),
            BindFlags.ConstantBuffer,
            ResourceUsage.Default,
            CpuAccessFlags.None);

        size = (Unsafe.SizeOf<ConstantBufferChangesEveryPrim>() + 15) / 16 * 16;
        //size = Unsafe.SizeOf<ConstantBufferChangesEveryPrim>();
        _constantBufferChangesEveryPrim = device.CreateBuffer(
            //size,
            Unsafe.SizeOf<Matrix4x4>(),
            BindFlags.ConstantBuffer,
            ResourceUsage.Default,
            CpuAccessFlags.None);

        // sampler
        var samplerDescription = new SamplerDescription(
            Filter.MinMagMipLinear,
            TextureAddressMode.Wrap,
            TextureAddressMode.Wrap,
            TextureAddressMode.Wrap,
            0,
            1,
            ComparisonFunction.Never,
            0,
            float.MaxValue
        );
        _samplerLinear = device.CreateSamplerState(samplerDescription);

        // shaders
        var vertexShaderBlob = ShaderCompiler.CompileShader("assets/shaders/VertexShader.hlsl", "main", "vs_5_0");
        _vertexShader = _d3d.Device.CreateVertexShader(vertexShaderBlob);
        _vertexLayout = _d3d.Device.CreateInputLayout(PNTVertexLayout, vertexShaderBlob);

        var pixelShaderBlob = ShaderCompiler.CompileShader("assets/shaders/PixelShader.hlsl", "main", "ps_5_0");
        _pixelShader = _d3d.Device.CreatePixelShader(pixelShaderBlob);

        ConstantBufferChangeOnResize changesOnResizeBuffer = new ConstantBufferChangeOnResize(_camera.ProjectionMatrix);
        _d3d.DeviceContext.UpdateSubresource(changesOnResizeBuffer, _constantBufferChangeOnResize);

        var constantBufferNeverChanges = new ConstantBufferNeverChanges
        {
            LightPosition1 = new Vector4(3.5f, 2.5f, 5.5f, 1.0f),
            LightPosition2 = new Vector4(3.5f, 2.5f, -5.5f, 1.0f),
            LightPosition3 = new Vector4(-3.5f, 2.5f, -5.5f, 1.0f),
            LightPosition4 = new Vector4(3.5f, 2.5f, 5.5f, 1.0f),
            LightColor = new Vector4(0.25f, 0.25f, 0.25f, 1.0f)
        };
        _d3d.DeviceContext.UpdateSubresource(constantBufferNeverChanges, _constantBufferNeverChanges);
    }

    public override void Render(double farseer)
    {
        _d3d.BeginFrame();

        var constantBufferChangesEveryFrame = new ConstantBufferChangesEveryFrame(_camera.ViewMatrix);
        _d3d.DeviceContext.UpdateSubresource(constantBufferChangesEveryFrame, _constantBufferChangesEveryFrame);

        _d3d.DeviceContext.IASetInputLayout(_vertexLayout);
        _d3d.DeviceContext.VSSetConstantBuffers(0, 5,
            new ID3D11Buffer[] {
                _constantBufferNeverChanges,
                _constantBufferChangeOnResize,
                _constantBufferChangesEveryFrame,
                _constantBufferChangesEveryPrim,
                _constantBufferTest
            });

        _d3d.DeviceContext.PSSetConstantBuffers(2, 2,
            new ID3D11Buffer[] {
                _constantBufferChangesEveryFrame,
                _constantBufferChangesEveryPrim
            });
        _d3d.DeviceContext.PSSetSampler(0, _samplerLinear);

        Matrix4x4 viewProj = Matrix4x4.Multiply(_camera.ViewMatrix, _camera.ProjectionMatrix);

        // simulate things to draw
        foreach (var renderObject in _gameObjects)
        {
            Matrix4x4 wvp = Matrix4x4.Multiply(renderObject.ModelMatrix, viewProj);
            _d3d.DeviceContext.UpdateSubresource(wvp, _constantBufferTest);

            renderObject.Render(_d3d.DeviceContext, _constantBufferChangesEveryPrim);
        }

        _d3d.Present();
    }

    public override void Update(TimeSpan deltaTime)
    {
        // do nothing
    }

    private void BindKeyboard()
    {
        _keyboard.SetOnLeftPressed(() =>
        {
            _gameObjects[0].Position = _gameObjects[0].Position with { X = _gameObjects[0].Position.X - 1 };
            _gameObjects[0].UpdateModelMatrix();
        });
        _keyboard.SetOnRightPressed(() =>
        {
            _gameObjects[0].Position = _gameObjects[0].Position with { X = _gameObjects[0].Position.X + 1 };
            _gameObjects[0].UpdateModelMatrix();
        });
        _keyboard.SetOnUpPressed(() =>
        {
            _gameObjects[0].Position = _gameObjects[0].Position with { Y = _gameObjects[0].Position.Y + 1 };
            _gameObjects[0].UpdateModelMatrix();
        });
        _keyboard.SetOnDownPressed(() =>
        {
            _gameObjects[0].Position = _gameObjects[0].Position with { Y = _gameObjects[0].Position.Y - 1 };
            _gameObjects[0].UpdateModelMatrix();
        });
        _keyboard.SetOnZoomInPressed(() =>
        {
            var newEye = new Vector3(_camera.Eye.X - 1, _camera.Eye.Y - 1, _camera.Eye.Z - 1);
            _camera.SetEyePosition(newEye);
        });
        _keyboard.SetOnZoomOutPressed(() =>
        {
            var newEye = new Vector3(_camera.Eye.X + 1, _camera.Eye.Y + 1, _camera.Eye.Z + 1);
            _camera.SetEyePosition(newEye);
        });
    }
}
