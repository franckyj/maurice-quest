// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using GLFW;
using SharpGen.Runtime;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.DXGI.Debug;
using Vortice.Mathematics;
using static Vortice.Direct3D11.D3D11;
using static Vortice.DXGI.DXGI;

namespace MyGame;

public static class MeshUtilities2
{
    private const int CubeFaceCount = 6;

    public static MeshData CreateCube(float size)
    {
        return CreateBox(new Vector3(size));
    }

    public static MeshData CreateBox(in Vector3 size)
    {
        List<VertexPositionNormalTexture> vertices = new();
        List<ushort> indices = new();

        Vector3[] faceNormals = new Vector3[CubeFaceCount]
        {
            Vector3.UnitZ,
            new Vector3(0.0f, 0.0f, -1.0f),
            Vector3.UnitX,
            new Vector3(-1.0f, 0.0f, 0.0f),
            Vector3.UnitY,
            new Vector3(0.0f, -1.0f, 0.0f),
        };

        Vector2[] textureCoordinates = new Vector2[4]
        {
            Vector2.UnitX,
            Vector2.One,
            Vector2.UnitY,
            Vector2.Zero,
        };

        Vector3 tsize = size / 2.0f;

        // Create each face in turn.
        int vbase = 0;
        for (int i = 0; i < CubeFaceCount; i++)
        {
            Vector3 normal = faceNormals[i];

            // Get two vectors perpendicular both to the face normal and to each other.
            Vector3 basis = (i >= 4) ? Vector3.UnitZ : Vector3.UnitY;

            Vector3 side1 = Vector3.Cross(normal, basis);
            Vector3 side2 = Vector3.Cross(normal, side1);

            // Six indices (two triangles) per face.
            indices.Add((ushort)(vbase + 0));
            indices.Add((ushort)(vbase + 1));
            indices.Add((ushort)(vbase + 2));

            indices.Add((ushort)(vbase + 0));
            indices.Add((ushort)(vbase + 2));
            indices.Add((ushort)(vbase + 3));

            // Four vertices per face.
            // (normal - side1 - side2) * tsize // normal // t0
            vertices.Add(new VertexPositionNormalTexture(
                Vector3.Multiply(Vector3.Subtract(Vector3.Subtract(normal, side1), side2), tsize),
                normal,
                textureCoordinates[0]
                ));

            // (normal - side1 + side2) * tsize // normal // t1
            vertices.Add(new VertexPositionNormalTexture(
                Vector3.Multiply(Vector3.Add(Vector3.Subtract(normal, side1), side2), tsize),
                normal,
                textureCoordinates[1]
                ));

            // (normal + side1 + side2) * tsize // normal // t2
            vertices.Add(new VertexPositionNormalTexture(
                Vector3.Multiply(Vector3.Add(normal, Vector3.Add(side1, side2)), tsize),
                normal,
                textureCoordinates[2]
                ));

            // (normal + side1 - side2) * tsize // normal // t3
            vertices.Add(new VertexPositionNormalTexture(
                Vector3.Multiply(Vector3.Subtract(Vector3.Add(normal, side1), side2), tsize),
                normal,
                textureCoordinates[3]
                ));

            vbase += 4;
        }

        return new MeshData(vertices.ToArray(), indices.ToArray());
    }
}

internal unsafe class DrawCube : D3D11ApplicationVortice
{
    private ID3D11Buffer _vertexBuffer;
    private ID3D11Buffer _indexBuffer;
    private ID3D11Buffer _constantBuffer;
    private ID3D11VertexShader _vertexShader;
    private ID3D11PixelShader _pixelShader;
    private ID3D11InputLayout _inputLayout;
    private Stopwatch _clock;

    protected override void Initialize()
    {
        MeshData mesh = MeshUtilities2.CreateCube(1.0f);
        _vertexBuffer = Device.CreateBuffer(mesh.Vertices, BindFlags.VertexBuffer);
        _indexBuffer = Device.CreateBuffer(mesh.Indices, BindFlags.IndexBuffer);

        _constantBuffer = Device.CreateBuffer(sizeof(Matrix4x4), BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write);

        var vertexShaderByteCode = ShaderCompiler.CompileShader("assets/shaders/Cube.hlsl", "VSMain", "vs_4_0");
        var pixelShaderByteCode = ShaderCompiler.CompileShader("assets/shaders/Cube.hlsl", "PSMain", "ps_4_0");

        _vertexShader = Device.CreateVertexShader(vertexShaderByteCode);
        _pixelShader = Device.CreatePixelShader(pixelShaderByteCode);
        _inputLayout = Device.CreateInputLayout(VertexPositionNormalTexture.InputElements, vertexShaderByteCode);

        _clock = Stopwatch.StartNew();
    }

    protected override void Dispose(bool dispose)
    {
        if (dispose)
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _constantBuffer.Dispose();
            _vertexShader.Dispose();
            _pixelShader.Dispose();
            _inputLayout.Dispose();
        }

        base.Dispose(dispose);
    }

    public override void OnRender()
    {
        DeviceContext.ClearRenderTargetView(ColorTextureView, Colors.CornflowerBlue);
        DeviceContext.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);

        var time = _clock.ElapsedMilliseconds / 1000.0f;
        //Matrix4x4 world = Matrix4x4.CreateRotationX(time) * Matrix4x4.CreateRotationY(time * 2) * Matrix4x4.CreateRotationZ(time * .7f);
        Matrix4x4 world = Matrix4x4.CreateTranslation(new Vector3(time * 1.5f, time / 1.2f, 0.0f));

        Matrix4x4 view = Matrix4x4.CreateLookAt(new Vector3(0, 0, 45), new Vector3(0, 0, 0), Vector3.UnitY);
        //Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4, AspectRatio, 0.1f, 100);
        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4.0f, (float)_window.ClientSize.Width / (float)_window.ClientSize.Height, 0.1f, 100);
        Matrix4x4 viewProjection = Matrix4x4.Multiply(view, projection);
        Matrix4x4 worldViewProjection = Matrix4x4.Multiply(world, viewProjection);

        // Update constant buffer data
        MappedSubresource mappedResource = DeviceContext.Map(_constantBuffer, MapMode.WriteDiscard);
        Unsafe.Copy(mappedResource.DataPointer.ToPointer(), ref worldViewProjection);
        DeviceContext.Unmap(_constantBuffer, 0);

        DeviceContext.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        DeviceContext.VSSetShader(_vertexShader);
        DeviceContext.PSSetShader(_pixelShader);
        DeviceContext.IASetInputLayout(_inputLayout);
        DeviceContext.VSSetConstantBuffer(0, _constantBuffer);
        DeviceContext.IASetVertexBuffer(0, _vertexBuffer, VertexPositionNormalTexture.SizeInBytes);
        DeviceContext.IASetIndexBuffer(_indexBuffer, Format.R16_UInt, 0);
        DeviceContext.DrawIndexed(36, 0, 0);
    }
}

/// <summary>
/// Class that handles all logic to have D3D11 running application.
/// </summary>
internal class D3D11ApplicationVortice : IDisposable
{
    private static readonly FeatureLevel[] s_featureLevels = new[]
    {
        FeatureLevel.Level_11_1,
        FeatureLevel.Level_11_0,
        FeatureLevel.Level_10_1,
        FeatureLevel.Level_10_0
    };


    private List<IDXGIAdapter1>? _adapters1;

    private readonly Format _colorFormat;
    private readonly Format _depthStencilFormat;
    private readonly int _backBufferCount;
    private IDXGIFactory2 _dxgiFactory;
    private bool disposedValue;
    private readonly bool _isTearingSupported;
    private readonly FeatureLevel _featureLevel;

    protected readonly NativeWindow _window;

    internal void Run()
    {
        while (true)
        {
            if (!BeginDraw())
                return;

            Render();

            EndDraw();
        }
    }

    private NativeWindow GetWindow()
    {
        try
        {
            var result = Glfw.Init();
            if (!result)
            {
                throw new GLFW.Exception("Error while initializing GLFW");
            }

            var videoMode = Glfw.GetVideoMode(Glfw.PrimaryMonitor);
            var width = (int)(videoMode.Width * 0.85f);
            var height = (int)(videoMode.Height * 0.85f);

            //Width = 640;
            //Height = 640;

            Glfw.WindowHint(Hint.ScaleToMonitor, false);
            Glfw.WindowHint(Hint.ClientApi, ClientApi.None);

            var window = new NativeWindow(width, height, "abc");

            int windowLeft = videoMode.Width / 2 - width / 2;
            int windowTop = videoMode.Height / 2 - height / 2;
            Glfw.SetWindowPosition(window, windowLeft, windowTop);

            return window;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Format ToSwapChainFormat(Format format)
    {
        // FLIP_DISCARD and FLIP_SEQEUNTIAL swapchain buffers only support these formats
        switch (format)
        {
            case Format.R16G16B16A16_Float:
                return Format.R16G16B16A16_Float;

            case Format.B8G8R8A8_UNorm:
            case Format.B8G8R8A8_UNorm_SRgb:
                return Format.B8G8R8A8_UNorm;

            case Format.R8G8B8A8_UNorm:
            case Format.R8G8B8A8_UNorm_SRgb:
                return Format.R8G8B8A8_UNorm;

            case Format.R10G10B10A2_UNorm:
                return Format.R10G10B10A2_UNorm;

            default:
                return Format.B8G8R8A8_UNorm;
        }
    }

    public D3D11ApplicationVortice(
        DeviceCreationFlags creationFlags = DeviceCreationFlags.BgraSupport,
        Format colorFormat = Format.B8G8R8A8_UNorm,
        Format depthStencilFormat = Format.D32_Float,
        int backBufferCount = 2)
    {
        _colorFormat = colorFormat;
        _depthStencilFormat = depthStencilFormat;
        _backBufferCount = backBufferCount;

        _dxgiFactory = CreateDXGIFactory1<IDXGIFactory2>();

        using (IDXGIFactory5? factory5 = _dxgiFactory.QueryInterfaceOrNull<IDXGIFactory5>())
        {
            if (factory5 != null)
            {
                _isTearingSupported = factory5.PresentAllowTearing;
            }
        }

        using (IDXGIAdapter1 adapter = GetHardwareAdapter())
        {
#if DEBUG
            if (SdkLayersAvailable())
            {
                creationFlags |= DeviceCreationFlags.Debug;
            }
#endif

            if (D3D11CreateDevice(
                adapter,
                DriverType.Unknown,
                creationFlags,
                s_featureLevels,
                out ID3D11Device tempDevice, out _featureLevel, out ID3D11DeviceContext tempContext).Failure)
            {
                // If the initialization fails, fall back to the WARP device.
                // For more information on WARP, see:
                // http://go.microsoft.com/fwlink/?LinkId=286690
                D3D11CreateDevice(
                    IntPtr.Zero,
                    DriverType.Warp,
                    creationFlags,
                    s_featureLevels,
                    out tempDevice, out _featureLevel, out tempContext).CheckError();
            }

            Device = tempDevice.QueryInterface<ID3D11Device1>();
            DeviceContext = tempContext.QueryInterface<ID3D11DeviceContext1>();
            tempContext.Dispose();
            tempDevice.Dispose();
        }

        var window = GetWindow();
        _window = window;
        IntPtr hwnd = window.Hwnd;

        int backBufferWidth = Math.Max(window.ClientBounds.Width, 1);
        int backBufferHeight = Math.Max(window.ClientBounds.Height, 1);
        Format backBufferFormat = ToSwapChainFormat(colorFormat);

        SwapChainDescription1 swapChainDescription = new()
        {
            Width = backBufferWidth,
            Height = backBufferHeight,
            Format = backBufferFormat,
            BufferCount = _backBufferCount,
            BufferUsage = Usage.RenderTargetOutput,
            SampleDescription = SampleDescription.Default,
            Scaling = Scaling.Stretch,
            SwapEffect = SwapEffect.FlipDiscard,
            AlphaMode = AlphaMode.Ignore,
            Flags = _isTearingSupported ? SwapChainFlags.AllowTearing : SwapChainFlags.None
        };

        SwapChainFullscreenDescription fullscreenDescription = new SwapChainFullscreenDescription
        {
            Windowed = true
        };

        SwapChain = _dxgiFactory.CreateSwapChainForHwnd(Device, hwnd, swapChainDescription, fullscreenDescription);
        _dxgiFactory.MakeWindowAssociation(hwnd, WindowAssociationFlags.IgnoreAltEnter);

        ColorTexture = SwapChain.GetBuffer<ID3D11Texture2D>(0);
        RenderTargetViewDescription renderTargetViewDesc = new(RenderTargetViewDimension.Texture2D, colorFormat);
        ColorTextureView = Device.CreateRenderTargetView(ColorTexture, renderTargetViewDesc);

        // Create depth stencil texture if required
        if (depthStencilFormat != Format.Unknown)
        {
            DepthStencilTexture = Device.CreateTexture2D(depthStencilFormat, backBufferWidth, backBufferHeight, 1, 1, null, BindFlags.DepthStencil);
            DepthStencilView = Device.CreateDepthStencilView(DepthStencilTexture!, new DepthStencilViewDescription(DepthStencilTexture, DepthStencilViewDimension.Texture2D));
        }

        Initialize();
    }

    public ID3D11Device1 Device { get; }
    public ID3D11DeviceContext1 DeviceContext { get; }
    public FeatureLevel FeatureLevel => _featureLevel;
    public IDXGISwapChain1 SwapChain { get; }

    public Format ColorFormat => _colorFormat;
    public ID3D11Texture2D ColorTexture { get; private set; }
    public ID3D11RenderTargetView ColorTextureView { get; private set; }

    public Format DepthStencilFormat => _depthStencilFormat;
    public ID3D11Texture2D? DepthStencilTexture { get; private set; }
    public ID3D11DepthStencilView? DepthStencilView { get; private set; }

    /// <summary>
    /// Gets the viewport.
    /// </summary>
    public Viewport Viewport => new(_window.ClientSize.Width, _window.ClientSize.Height);

    public bool DiscardViews { get; set; } = true;

    private IDXGIAdapter1 GetHardwareAdapter()
    {
        IDXGIFactory6? factory6 = _dxgiFactory.QueryInterfaceOrNull<IDXGIFactory6>();
        if (factory6 != null)
        {
            for (int adapterIndex = 0;
                factory6.EnumAdapterByGpuPreference(adapterIndex, GpuPreference.HighPerformance, out IDXGIAdapter1? adapter).Success;
                adapterIndex++)
            {
                if (adapter == null)
                {
                    continue;
                }

                AdapterDescription1 desc = adapter.Description1;

                if ((desc.Flags & AdapterFlags.Software) != AdapterFlags.None)
                {
                    // Don't select the Basic Render Driver adapter.
                    adapter.Dispose();
                    continue;
                }

                factory6.Dispose();
                return adapter;
            }

            factory6.Dispose();
        }

        foreach (IDXGIAdapter1 adapter in EnumAdapters1())
        {
            AdapterDescription1 desc = adapter.Description1;

            if ((desc.Flags & AdapterFlags.Software) != AdapterFlags.None)
            {
                // Don't select the Basic Render Driver adapter.
                continue;
            }

            return adapter;
        }

        throw new InvalidOperationException("Cannot detect D3D11 adapter");
    }

    public IEnumerable<IDXGIAdapter1> EnumAdapters1()
    {
        if (_adapters1 == null)
        {
            _adapters1 = new List<IDXGIAdapter1>();
            while (true)
            {
                Result result = _dxgiFactory.EnumAdapters1(_adapters1.Count, out IDXGIAdapter1? adapter);
                if (result.Failure || adapter == null)
                {
                    break;
                }

                _adapters1.Add(adapter);
            }
        }

        return _adapters1!;
    }

    private void HandleDeviceLost()
    {

    }

    private void UpdateColorSpace()
    {
        if (!_dxgiFactory.IsCurrent)
        {
            // Output information is cached on the DXGI Factory. If it is stale we need to create a new factory.
            _dxgiFactory.Dispose();
            _dxgiFactory = CreateDXGIFactory1<IDXGIFactory2>();
        }
    }

    private void ResizeSwapchain()
    {
        // Clear the previous window size specific context.
        DeviceContext.UnsetRenderTargets();
        ColorTextureView.Dispose();
        DepthStencilView?.Dispose();
        ColorTexture.Dispose();
        DepthStencilTexture?.Dispose();
        DeviceContext.Flush();

        int backBufferWidth = Math.Max(_window.ClientSize.Width, 1);
        int backBufferHeight = Math.Max(_window.ClientSize.Height, 1);
        Format backBufferFormat = ToSwapChainFormat(_colorFormat);

        // If the swap chain already exists, resize it.
        Result hr = SwapChain.ResizeBuffers(
            _backBufferCount,
            backBufferWidth,
            backBufferHeight,
            backBufferFormat,
            _isTearingSupported ? SwapChainFlags.AllowTearing : SwapChainFlags.None
            );

        if (hr == Vortice.DXGI.ResultCode.DeviceRemoved || hr == Vortice.DXGI.ResultCode.DeviceReset)
        {
#if DEBUG
            Result logResult = (hr == Vortice.DXGI.ResultCode.DeviceRemoved) ? Device.DeviceRemovedReason : hr;
            Debug.WriteLine($"Device Lost on ResizeBuffers: Reason code {logResult}");
#endif
            // If the device was removed for any reason, a new device and swap chain will need to be created.
            HandleDeviceLost();

            // Everything is set up now. Do not continue execution of this method. HandleDeviceLost will reenter this method
            // and correctly set up the new device.
            return;
        }
        else
        {
            hr.CheckError();
        }

        ColorTexture = SwapChain.GetBuffer<ID3D11Texture2D>(0);
        RenderTargetViewDescription renderTargetViewDesc = new(RenderTargetViewDimension.Texture2D, _colorFormat);
        ColorTextureView = Device.CreateRenderTargetView(ColorTexture, renderTargetViewDesc);

        // Create depth stencil texture if required
        if (_depthStencilFormat != Format.Unknown)
        {
            DepthStencilTexture = Device.CreateTexture2D(_depthStencilFormat, backBufferWidth, backBufferHeight, 1, 1, null, BindFlags.DepthStencil);
            DepthStencilView = Device.CreateDepthStencilView(DepthStencilTexture!, new DepthStencilViewDescription(DepthStencilTexture, DepthStencilViewDimension.Texture2D));
        }
    }

    protected internal void Render()
    {
        DeviceContext.OMSetRenderTargets(ColorTextureView, DepthStencilView);
        DeviceContext.RSSetViewport(Viewport);
        DeviceContext.RSSetScissorRect(0, 0, _window.ClientSize.Width, _window.ClientSize.Height);

        OnRender();
    }

    public virtual void OnRender() { }

    protected virtual void Initialize() { }

    protected bool BeginDraw()
    {
        // Check for window size changes and resize the swapchain if needed.
        SwapChainDescription1 swapChainDesc = SwapChain.Description1;

        if (_window.ClientSize.Width != swapChainDesc.Width ||
            _window.ClientSize.Height != swapChainDesc.Height)
        {
            ResizeSwapchain();
        }

        return true;
    }

    protected void EndDraw()
    {
        int syncInterval = 1;
        PresentFlags presentFlags = PresentFlags.None;
        //if (!EnableVerticalSync)
        if (false)
        {
            syncInterval = 0;
            if (_isTearingSupported)
            {
                presentFlags = PresentFlags.AllowTearing;
            }
        }

        Result result = SwapChain.Present(syncInterval, presentFlags);

        if (DiscardViews)
        {
            // Discard the contents of the render target.
            // This is a valid operation only when the existing contents will be entirely
            // overwritten. If dirty or scroll rects are used, this call should be removed.
            DeviceContext.DiscardView(ColorTextureView);

            if (DepthStencilView != null)
            {
                // Discard the contents of the depth stencil.
                DeviceContext.DiscardView(DepthStencilView);
            }
        }

        // If the device was reset we must completely reinitialize the renderer.
        if (result == Vortice.DXGI.ResultCode.DeviceRemoved || result == Vortice.DXGI.ResultCode.DeviceReset)
        {
#if DEBUG
            Result logResult = (result == Vortice.DXGI.ResultCode.DeviceRemoved) ? Device.DeviceRemovedReason : result;
            Debug.WriteLine($"Device Lost on Present: Reason code {logResult}");
#endif
            HandleDeviceLost();
        }
        else
        {
            result.CheckError();

            if (!_dxgiFactory.IsCurrent)
            {
                UpdateColorSpace();
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposedValue) return;

        if (disposing)
        {
            ColorTexture.Dispose();
            ColorTextureView.Dispose();
            DepthStencilTexture?.Dispose();
            DepthStencilView?.Dispose();

            SwapChain.Dispose();
            DeviceContext.Dispose();
            Device.Dispose();
            _dxgiFactory.Dispose();

#if DEBUG
            if (DXGIGetDebugInterface1(out IDXGIDebug1? dxgiDebug).Success)
            {
                dxgiDebug!.ReportLiveObjects(DebugAll, ReportLiveObjectFlags.Summary | ReportLiveObjectFlags.IgnoreInternal);
                dxgiDebug!.Dispose();
            }
#endif

        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~D3D11ApplicationVortice()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
