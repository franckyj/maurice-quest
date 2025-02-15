using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Vortice.Direct3D11;
using static MyTetris.ConstantBuffers;

namespace MyTetris;

public enum GameState
{
    Idle,
    Playing,
    GameOver
}

internal class MyGame : D3D11Application
{
    private const int ScoreStep = 10;

    private readonly Board _board;
    private readonly Lock _boardLock;

    private GameState _state;
    private int _score;
    private TimeSpan _stepInternal;

    private PeriodicTimer _timer;
    StringBuilder _stringBuilder = new StringBuilder();

    // buffers
    private ID3D11Buffer _constantBufferChangesEveryFrame;
    private ID3D11Buffer _constantBufferChangesEveryPrim;

    private ID3D11ShaderResourceView _texture;

    private ID3D11SamplerState _samplerLinear;
    private ID3D11VertexShader _vertexShader;
    private ID3D11PixelShader _pixelShader;
    private ID3D11InputLayout _vertexLayout;

    // projections
    private Matrix4x4 _viewProjection;

    public MyGame(
        ILogger<D3D11Application> logger,
        string title)
        : base(logger, title)
    {
        CreateGameDeviceResources();

        BindInput();

        _boardLock = new Lock();
        _board = new Board();
        //_stepInternal = TimeSpan.FromSeconds(1);
        _stepInternal = TimeSpan.FromMilliseconds(500);

        StartGame();
        DrawBoard();
    }

    private void CreateGameDeviceResources()
    {
        // view & projection
        var view = Matrix4x4.CreateLookAtLeftHanded(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY);
        var projection = Matrix4x4.CreateOrthographicLeftHanded(
            _window.ClientWindowWidth,
            _window.ClientWindowHeight,
            0.1f, 100);
        _viewProjection = Matrix4x4.Multiply(view, projection);

        var device = _d3d.Device;

        var size = (Unsafe.SizeOf<ConstantBufferChangesEveryFrame>() + 15) / 16 * 16;
        _constantBufferChangesEveryFrame = device.CreateBuffer(
            size,
            BindFlags.ConstantBuffer,
            ResourceUsage.Default,
            CpuAccessFlags.None);

        size = (Unsafe.SizeOf<ConstantBufferChangesEveryPrim>() + 15) / 16 * 16;
        _constantBufferChangesEveryPrim = device.CreateBuffer(
            size,
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
        _vertexLayout = _d3d.Device.CreateInputLayout(Vertices.PNTVertexLayout, vertexShaderBlob);

        var pixelShaderBlob = ShaderCompiler.CompileShader("assets/shaders/PixelShader.hlsl", "main", "ps_5_0");
        _pixelShader = _d3d.Device.CreatePixelShader(pixelShaderBlob);
    }

    public override void Render(double farseer)
    {
        _d3d.BeginFrame();

        _d3d.DeviceContext.IASetInputLayout(_vertexLayout);
        _d3d.DeviceContext.VSSetConstantBuffers(0, 2,
            new ID3D11Buffer[] {
                _constantBufferChangesEveryFrame,
                _constantBufferChangesEveryPrim,
            });

        _d3d.DeviceContext.PSSetConstantBuffers(2, 2,
            new ID3D11Buffer[] {
                _constantBufferChangesEveryFrame,
                _constantBufferChangesEveryPrim
            });
        _d3d.DeviceContext.PSSetSampler(0, _samplerLinear);

        _d3d.DeviceContext.UpdateSubresource(_viewProjection, _constantBufferChangesEveryFrame);
        //MappedSubresource mappedResource = _d3d.DeviceContext.Map(_constantBufferChangesEveryFrame, MapMode.WriteDiscard);
        //unsafe
        //{
        //    Unsafe.Copy(mappedResource.DataPointer.ToPointer(), ref _viewProjection);
        //}
        //_d3d.DeviceContext.Unmap(_constantBufferChangesEveryFrame, 0);

        _d3d.DeviceContext.VSSetShader(_vertexShader, null, 0);
        _d3d.DeviceContext.PSSetShader(_pixelShader, null, 0);

        lock (_boardLock)
        {
            DrawScore();
            DrawBoard();
            DrawPreview();
        }
        _d3d.Present();
    }

    public override void Update(TimeSpan deltaTime)
    {
    }

    private void StartGame()
    {
        _state = GameState.Playing;
        _board.StartNewGame();

        _timer = new PeriodicTimer(_stepInternal);
        var timerTask = Task.Run(async () =>
        {
            while (await _timer.WaitForNextTickAsync())
            {
                lock (_boardLock)
                    if (!_board.PushPieceDown()) break;
            }
        });
    }

    private void DrawScore()
    {

    }

    private void DrawBoard()
    {
        _stringBuilder.Append('=', _board.ColumnsCount);
        _stringBuilder.AppendLine();
        for (int y = _board.RowsCount - 1; y >= 0; y--)
        {
            for (int x = 0; x < _board.ColumnsCount; x++)
            {
                var state = _board.GetCellState((short)x, (short)y);
                if (state.Filled)
                {
                    _stringBuilder.Append('X');
                }
                else
                {
                    _stringBuilder.Append(' ');
                }
            }
            _stringBuilder.AppendLine();
        }
        _stringBuilder.Append('=', _board.ColumnsCount);
        Console.Clear();
        Console.WriteLine(_stringBuilder.ToString());

        _stringBuilder.Clear();
    }

    private void DrawPreview()
    {

    }

    private void BindInput()
    {
        _input.SetOnLeftPressed(() =>
        {
            lock (_boardLock)
                _board.MovePieceLeft();
        });
        _input.SetOnRightPressed(() =>
        {
            lock (_boardLock)
                _board.MovePieceRight();
        });
        _input.SetOnUpPressed(() =>
        {
            lock (_boardLock)
                _board.RotatePieceClockwise();
        });
        _input.SetOnDownPressed(() =>
        {
            lock (_boardLock)
                _board.RotatePieceCounterClockwise();
        });
        _input.SetOnZoomInPressed(() =>
        {
        });
        _input.SetOnZoomOutPressed(() =>
        {
        });
    }
}
