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

        //_rasterizerState = _device.CreateRasterizerState(new RasterizerDescription(CullMode.Back, FillMode.Solid));
        //_deviceContext.RSSetState(_rasterizerState);

        //Matrix4x4 view = Matrix4x4.CreateLookAt(new Vector3(0, 0, 30), new Vector3(0, 0, 0), Vector3.UnitY);
        //Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4, AspectRatio, 1.0f, 100.0f);
        //_viewProjectionMatrix = Matrix4x4.Multiply(view, projection);

        //_depthState = _device.CreateDepthStencilState(new DepthStencilDescription()
        //{
        //    DepthEnable = true,
        //    DepthWriteMask = DepthWriteMask.All,
        //    DepthFunc = ComparisonFunction.Less
        //});
        //_deviceContext.OMSetDepthStencilState(_depthState);
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
        _deviceContext.OMSetRenderTargets(1, new[] { _renderTargetView }, _depthStencilView);
        _viewport = new Viewport(_renderTargetSize.Width, _renderTargetSize.Height);
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

    /*
     private:
        void CreateDeviceIndependentResources();
        void CreateDeviceResources();
        void CreateWindowSizeDependentResources();
        DXGI_MODE_ROTATION ComputeDisplayRotation();
        void CheckStereoEnabledStatus();

        // Direct3D objects.
        Microsoft::WRL::ComPtr<ID3D11Device3>           m_d3dDevice;
        Microsoft::WRL::ComPtr<ID3D11DeviceContext3>    m_d3dContext;
        Microsoft::WRL::ComPtr<IDXGISwapChain1>         m_swapChain;

        // Direct3D rendering objects. Required for 3D.
        Microsoft::WRL::ComPtr<ID3D11RenderTargetView>  m_d3dRenderTargetView;
        Microsoft::WRL::ComPtr<ID3D11RenderTargetView>  m_d3dRenderTargetViewRight;
        Microsoft::WRL::ComPtr<ID3D11DepthStencilView>  m_d3dDepthStencilView;
        D3D11_VIEWPORT                                  m_screenViewport;

        // Direct2D drawing components.
        Microsoft::WRL::ComPtr<ID2D1Factory3>           m_d2dFactory;
        Microsoft::WRL::ComPtr<ID2D1Device2>            m_d2dDevice;
        Microsoft::WRL::ComPtr<ID2D1DeviceContext2>     m_d2dContext;
        Microsoft::WRL::ComPtr<ID2D1Bitmap1>            m_d2dTargetBitmap;
        Microsoft::WRL::ComPtr<ID2D1Bitmap1>            m_d2dTargetBitmapRight;

        // DirectWrite drawing components.
        Microsoft::WRL::ComPtr<IDWriteFactory3>         m_dwriteFactory;
        Microsoft::WRL::ComPtr<IWICImagingFactory2>     m_wicFactory;

        // Cached reference to the Window.
        Platform::Agile<Windows::UI::Core::CoreWindow>  m_window;

        // Cached device properties.
        D3D_FEATURE_LEVEL                               m_d3dFeatureLevel;
        Windows::Foundation::Size                       m_d3dRenderTargetSize;
        Windows::Foundation::Size                       m_outputSize;
        Windows::Foundation::Size                       m_logicalSize;
        Windows::Graphics::Display::DisplayOrientations m_nativeOrientation;
        Windows::Graphics::Display::DisplayOrientations m_currentOrientation;
        float                                           m_dpi;
        bool                                            m_stereoEnabled;

        // Transforms used for display orientation.
        D2D1::Matrix3x2F                                m_orientationTransform2D;
        DirectX::XMFLOAT4X4                             m_orientationTransform3D;

        // The IDeviceNotify can be held directly as it owns the DeviceResources.
        IDeviceNotify*                                  m_deviceNotify;
     */
}
