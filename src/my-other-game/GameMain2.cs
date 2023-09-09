using System.Numerics;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace MyOtherGame;

internal unsafe sealed class GameMain2 : Application
{
    private static readonly FeatureLevel[] _featureLevels;

    private Viewport _viewport;
    private ID3D11Device _device;
    private ID3D11DeviceContext _deviceContext;
    private IDXGIFactory7 _dxgiFactory;
    private IDXGISwapChain1 _swapchain;
    private ID3D11Texture2D _backBuffer;
    private ID3D11RenderTargetView _renderTarget;
    private ID3D11RenderTargetView[] _renderTargets;
    private ID3D11DepthStencilState _depthState;
    private ID3D11DepthStencilView _depthView;
    private ID3D11RasterizerState _rasterizerState;

    private Matrix4x4 _viewProjectionMatrix;
    private readonly Color4 _clearColor;

    //private IDrawable[] _shapes;

    static GameMain2()
    {
        _featureLevels = new[]
        {
            FeatureLevel.Level_11_0
        };
    }

    public GameMain2(string title) : base(title)
    {
        _clearColor = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
    }

    public void BeginFrame()
    {
        _deviceContext.ClearRenderTargetView(_renderTarget, _clearColor);
        _deviceContext.ClearDepthStencilView(_depthView, DepthStencilClearFlags.Depth, 1.0f, 0);

        // this needs to stay there!
        _deviceContext.OMSetRenderTargets(1, _renderTargets, _depthView);
        _deviceContext.RSSetViewport(_viewport);
    }

    public void EndFrame()
    {
        _swapchain.Present(1, PresentFlags.None);
    }

    //protected override void Update(float mouseX, float mouseY)
    //{
    //    base.Update(mouseX, mouseY);

    //    //var dt = 1.0f / 60.0f / 2.0f;
    //    var dt = 1.0f / 60.0f;
    //    //for (int i = 0; i < _shapes.Length; i++)
    //    //{
    //    //    _shapes[i].Update(
    //    //        dt,
    //    //        _viewProjectionMatrix,
    //    //        _deviceContext,
    //    //        mouseX,
    //    //        mouseY,
    //    //        Width,
    //    //        Height);
    //    //}
    //}

    //protected override void Render()
    //{
    //    BeginFrame();

    //    //for (int i = 0; i < _shapes.Length; i++)
    //    //{
    //    //    _shapes[i].Draw(_deviceContext);
    //    //}

    //    EndFrame();

    //    base.Render();
    //}

    protected override void Initialize()
    {
        base.Initialize();

        _dxgiFactory = DXGI.CreateDXGIFactory2<IDXGIFactory7>(false);

        using var adapter = GetHardwareAdapter();
        var deviceCreationFlags = DeviceCreationFlags.BgraSupport;
#if DEBUG
        deviceCreationFlags |= DeviceCreationFlags.Debug;
#endif

        var result = D3D11.D3D11CreateDevice(
            null,
            DriverType.Hardware,
            deviceCreationFlags,
            _featureLevels,
            out var tempDevice,
            out var _featureLevel,
            out var tempDeviceContext);

        if (result.Failure)
            throw new InvalidOperationException(result.Description);

        _device = tempDevice.QueryInterface<ID3D11Device5>();
        _deviceContext = tempDeviceContext.QueryInterface<ID3D11DeviceContext4>();

        tempDeviceContext.Dispose();
        tempDevice.Dispose();

        CreateSwapchain(Window!.Hwnd, Width, Height);
        CreateSwapchainResources();

        _rasterizerState = _device.CreateRasterizerState(new RasterizerDescription(CullMode.Back, FillMode.Solid));
        _deviceContext.RSSetState(_rasterizerState);

        Matrix4x4 view = Matrix4x4.CreateLookAt(new Vector3(0, 0, 30), new Vector3(0, 0, 0), Vector3.UnitY);
        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4, AspectRatio, 1.0f, 100.0f);
        _viewProjectionMatrix = Matrix4x4.Multiply(view, projection);

        _depthState = _device.CreateDepthStencilState(new DepthStencilDescription()
        {
            DepthEnable = true,
            DepthWriteMask = DepthWriteMask.All,
            DepthFunc = ComparisonFunction.Less
        });
        _deviceContext.OMSetDepthStencilState(_depthState);

        //const int shapeCount = 80;
        //_shapes = new IDrawable[shapeCount];

        //var cubeCount = shapeCount / 2;
        //for (int i = 0; i < cubeCount; ++i)
        //{
        //    _shapes[i] = new Drawable.Box(new Vector3(0, 0, 0), _device, i);
        //}

        //var pyramidCount = shapeCount - cubeCount;
        //for (int i = cubeCount; i < cubeCount + pyramidCount; ++i)
        //{
        //    _shapes[i] = new Drawable.Pyramid(new Vector3(0, 0, 0), _device, i);
        //}
    }

    protected override void OnResize()
    {
        base.OnResize();

        _deviceContext.Flush();
        DestroySwapchainResources();

        _swapchain.ResizeBuffers(0, Width, Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);
        CreateSwapchainResources();
    }

    protected override void Dispose(bool disposing)
    {
        DestroySwapchainResources();

        _deviceContext?.Dispose();
        _device?.Dispose();

        base.Dispose(disposing);
    }

    private IDXGIAdapter GetHardwareAdapter()
    {
        var factory7 = _dxgiFactory.QueryInterface<IDXGIFactory7>();
        if (factory7 != null)
        {
            for (var adapterIndex = 0;
                 factory7.EnumAdapterByGpuPreference(
                     adapterIndex,
                     GpuPreference.HighPerformance,
#if NET6
                         out IDXGIAdapter1? adapter).Success;
#else
                     out IDXGIAdapter1 adapter).Success;
#endif
                 adapterIndex++)
            {
                if (adapter == null)
                {
                    continue;
                }

                var adapterDescription = adapter.Description1;
                if ((adapterDescription.Flags & AdapterFlags.Software) != AdapterFlags.None)
                {
                    adapter.Dispose();
                    continue;
                }

                factory7.Dispose();
                return adapter;
            }

            factory7.Dispose();
        }

        for (var adapterIndex = 0;
#if NET6
                 _dxgiFactory.EnumAdapters1(adapterIndex, out IDXGIAdapter1? adapter).Success;
#else
             _dxgiFactory.EnumAdapters1(adapterIndex, out var adapter).Success;
#endif
             adapterIndex++)
        {
            var adapterDescription = adapter.Description1;
            if ((adapterDescription.Flags & AdapterFlags.Software) != AdapterFlags.None)
            {
                adapter.Dispose();
                continue;
            }

            return adapter;
        }

        throw new InvalidOperationException("Unable to find a D3D11 adapter");
    }
    private void CreateSwapchain(IntPtr windowHandle, int width, int height)
    {
        var swapChainDescriptor = new SwapChainDescription1
        {
            Width = width,
            Height = height,
            Format = Format.R8G8B8A8_UNorm,
            BufferCount = 2,
            BufferUsage = Usage.RenderTargetOutput,
            SampleDescription = new SampleDescription
            {
                Count = 1,
                Quality = 0
            },
            Scaling = Scaling.Stretch,
            SwapEffect = SwapEffect.FlipDiscard,
            AlphaMode = AlphaMode.Ignore,
            Flags = SwapChainFlags.None
        };

        var swapChainFullscreenDescriptor = new SwapChainFullscreenDescription
        {
            Windowed = true
        };

        _swapchain = _dxgiFactory.CreateSwapChainForHwnd(
            _device,
            windowHandle,
            swapChainDescriptor,
            swapChainFullscreenDescriptor);
    }

    private void CreateSwapchainResources()
    {
        _backBuffer = _swapchain.GetBuffer<ID3D11Texture2D>(0);
        _renderTarget = _device.CreateRenderTargetView(_backBuffer);

        var depthTexture = _device.CreateTexture2D(Format.D32_Float, Width, Height, 1, 1, null, BindFlags.DepthStencil, ResourceOptionFlags.None, ResourceUsage.Default, CpuAccessFlags.None);
        _depthView = _device.CreateDepthStencilView(depthTexture, new DepthStencilViewDescription()
        {
            Format = Format.D32_Float,
            ViewDimension = DepthStencilViewDimension.Texture2D
        });
        _renderTargets = new[] { _renderTarget };
        _deviceContext.OMSetRenderTargets(1, _renderTargets, _depthView);

        _viewport = new Viewport(Width, Height);
    }

    private void DestroySwapchainResources()
    {
        //_deviceContext.UnsetRenderTargets();
        _renderTarget?.Dispose();
        _depthView.Dispose();
        _backBuffer?.Dispose();
    }
}
