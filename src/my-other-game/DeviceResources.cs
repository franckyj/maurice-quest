using System.Drawing;
using System.Numerics;
using GLFW;
using SharpGen.Runtime;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace MyOtherGame;

internal class DeviceResources
{
    private static readonly FeatureLevel[] _featureLevels;

    private IntPtr _windowHandle;
    private Viewport _viewport;
    private ID3D11Device _device;
    private ID3D11DeviceContext _deviceContext;
    private IDXGIFactory7 _dxgiFactory;
    private IDXGISwapChain1 _swapchain;
    private ID3D11Texture2D _backBuffer;
    private ID3D11RenderTargetView _renderTargetView;
    private Size _renderTargetSize;
    private ID3D11DepthStencilView _depthStencilView;

    // TODO change all of those properties
    public ID3D11DeviceContext DeviceContext => _deviceContext;
    public ID3D11Device Device => _device;
    public ID3D11RenderTargetView RenderTargetView => _renderTargetView;
    public Size RenderTargetSize => _renderTargetSize;
    public ID3D11DepthStencilView DepthStencilView => _depthStencilView;
    public Viewport Viewport => _viewport;

    static DeviceResources()
    {
        _featureLevels = new[]
        {
            FeatureLevel.Level_11_0
        };
    }

    public DeviceResources()
    {
        // D2D11 stuff
        // CreateDeviceIndependentResources();
        CreateDeviceResources();
    }

    /// <summary>
    /// Configures the Direct3D device, and stores handles to it and the device context.
    /// </summary>
    public void CreateDeviceResources()
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

        //CreateSwapchain(Window!.Hwnd, Width, Height);
        //CreateSwapchainResources();

        var rasterizerState = _device.CreateRasterizerState(new RasterizerDescription(CullMode.Back, FillMode.Solid));
        _deviceContext.RSSetState(rasterizerState);
    }

    public void SetWindow(IntPtr windowHandle, int windowHeight, int windowWidth)
    {
        _windowHandle = windowHandle;
        _renderTargetSize = new Size(windowWidth, windowHeight);
        CreateWindowSizeDependentResources();
    }

    public void Present()
    {
        // the first argument instructs DXGI to block until VSync, putting the application
        // to sleep until the next VSync. This ensures we don't waste any cycles rendering
        // frames that will never be displayed to the screen
        _swapchain.Present(1, PresentFlags.None);
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
            _windowHandle,
            swapChainDescriptor,
            swapChainFullscreenDescriptor);
    }

    private void CreateSwapchainResources()
    {
        _backBuffer = _swapchain.GetBuffer<ID3D11Texture2D>(0);
        _renderTargetView = _device.CreateRenderTargetView(_backBuffer);

        var depthTexture = _device.CreateTexture2D(Format.D32_Float, _renderTargetSize.Width, _renderTargetSize.Height, 1, 1, null, BindFlags.DepthStencil, ResourceOptionFlags.None, ResourceUsage.Default, CpuAccessFlags.None);
        _depthStencilView = _device.CreateDepthStencilView(depthTexture, new DepthStencilViewDescription()
        {
            Format = Format.D32_Float,
            ViewDimension = DepthStencilViewDimension.Texture2D
        });
        //_deviceContext.OMSetRenderTargets(1, new[] { _renderTargetView }, _depthStencilView);
        _viewport = new Viewport(_renderTargetSize.Width, _renderTargetSize.Height);

        //_deviceContext.RSSetViewport(_viewport);
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
}
