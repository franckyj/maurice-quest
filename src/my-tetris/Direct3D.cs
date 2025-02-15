using System.Drawing;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace MyTetris;

internal class Direct3D : IDisposable
{
    private static readonly FeatureLevel[] _featureLevels;

    private readonly ID3D11Device _device;
    private readonly ID3D11DeviceContext _deviceContext;
    private readonly IDXGIFactory7 _dxgiFactory;
    private IDXGISwapChain1 _swapchain;
    private ID3D11Texture2D _backBuffer;
    private ID3D11RenderTargetView[] _renderTargetViews;
    private ID3D11RenderTargetView _renderTargetView;
    private ID3D11DepthStencilView _depthStencilView;
    private Size _renderTargetSize;
    private IntPtr _windowHandle;

    private Viewport _viewportScore;
    private Viewport _viewportBoard;
    private Viewport _viewportPreview;

    private bool _isDisposed;

    public ID3D11Device Device => _device;
    public ID3D11DeviceContext DeviceContext => _deviceContext;

    static Direct3D()
    {
        _featureLevels = new[]
        {
            FeatureLevel.Level_11_1,
            FeatureLevel.Level_11_0,
            FeatureLevel.Level_10_1,
            FeatureLevel.Level_10_0
        };
    }

    public Direct3D()
    {
        _dxgiFactory = DXGI.CreateDXGIFactory2<IDXGIFactory7>(false);

        using var adapter = GetHardwareAdapter();
        var deviceCreationFlags = DeviceCreationFlags.BgraSupport;
#if DEBUG
        if (D3D11.SdkLayersAvailable())
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

        var rasterizerState = _device.CreateRasterizerState(new RasterizerDescription(CullMode.Back, FillMode.Solid));
        _deviceContext.RSSetState(rasterizerState);
    }

    public void SetWindow(IntPtr windowHandle, int windowWidth, int windowHeight)
    {
        _windowHandle = windowHandle;
        _renderTargetSize = new Size(windowWidth, windowHeight);
        CreateWindowSizeDependentResources();
    }

    public void BeginFrame()
    {
        Color4 clearColor = new Color4(255, 0, 255);
        _deviceContext.ClearRenderTargetView(_renderTargetView, clearColor);
        _deviceContext.ClearDepthStencilView(_depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);

        // this needs to stay there!
        _deviceContext.OMSetRenderTargets(1, _renderTargetViews, _depthStencilView);
    }

    public void Present()
    {
        // the first argument instructs DXGI to block until VSync, putting the application
        // to sleep until the next VSync. This ensures we don't waste any cycles rendering
        // frames that will never be displayed to the screen
        _swapchain.Present(1, PresentFlags.None);
    }

    public void OnResize(Size newClientSize)
    {
        _renderTargetSize = newClientSize;
        CreateWindowSizeDependentResources();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _depthStencilView.Dispose();
                _renderTargetView.Dispose();
                _backBuffer.Dispose();
                _swapchain.Dispose();
                _dxgiFactory.Dispose();
                _deviceContext.Dispose();
                _device.Dispose();
            }

            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void CreateWindowSizeDependentResources()
    {
        DestroySwapchainResources();

        if (_swapchain != null)
            // just resize it
            _swapchain.ResizeBuffers(0, _renderTargetSize.Width, _renderTargetSize.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);
        else
            CreateSwapchain();

        CreateSwapchainResources();
    }

    private void CreateSwapchain()
    {
        var swapChainDescriptor = new SwapChainDescription1
        {
            Width = _renderTargetSize.Width,
            Height = _renderTargetSize.Height,
            Format = Format.R8G8B8A8_UNorm,
            BufferCount = 2,
            BufferUsage = Usage.RenderTargetOutput,
            SampleDescription = new SampleDescription
            {
                Count = 1,
                Quality = 0
            },
            Scaling = Scaling.None,
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
            _windowHandle,
            swapChainDescriptor,
            swapChainFullscreenDescriptor);
    }

    private void CreateSwapchainResources()
    {
        _backBuffer = _swapchain.GetBuffer<ID3D11Texture2D>(0);
        RenderTargetViewDescription renderTargetViewDesc = new(RenderTargetViewDimension.Texture2D, Format.R8G8B8A8_UNorm);
        _renderTargetView = _device.CreateRenderTargetView(_backBuffer, renderTargetViewDesc);
        _renderTargetViews = new[] { _renderTargetView };

        var depthTexture = _device.CreateTexture2D(Format.D32_Float, _renderTargetSize.Width, _renderTargetSize.Height, 1, 1, null, BindFlags.DepthStencil, ResourceOptionFlags.None, ResourceUsage.Default, CpuAccessFlags.None);
        _depthStencilView = _device.CreateDepthStencilView(depthTexture, new DepthStencilViewDescription()
        {
            Format = Format.D32_Float,
            ViewDimension = DepthStencilViewDimension.Texture2D
        });
        _viewportBoard = new Viewport(_renderTargetSize.Width, _renderTargetSize.Height);
        _deviceContext.RSSetViewport(_viewportBoard);
    }

    private void DestroySwapchainResources()
    {
        _deviceContext.UnsetRenderTargets();
        _renderTargetView?.Dispose();
        _depthStencilView?.Dispose();
        _backBuffer?.Dispose();
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
             _dxgiFactory.EnumAdapters1(adapterIndex, out var adapter).Success;
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
}
