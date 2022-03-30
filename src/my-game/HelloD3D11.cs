using System.Numerics;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace MyGame
{
    internal unsafe sealed class HelloD3D11 : Application
    {
        private static readonly FeatureLevel[] _featureLevels;

        private IDXGIFactory2 _dxgiFactory;
        private IDXGISwapChain1 _swapchain;
        private ID3D11Device _device;
        private ID3D11DeviceContext _deviceContext;
        private ID3D11Texture2D _backBuffer;
        private ID3D11RenderTargetView _renderTarget;

        private Blob _vertexShaderBlob;
        private ID3D11VertexShader _vertexShader;
        private ID3D11PixelShader _pixelShader;
        private ID3D11InputLayout _inputLayout;
        private ID3D11Buffer _vertexBuffer;
        //private ID3D11Texture2D _depthStencilBuffer;
        //private ID3D11DepthStencilView _depthStencilView;
        //private ID3D11RasterizerState _wireFrameRasterizerState;
        //private ID3D11RasterizerState _solidRasterizerState;
        //private ID3D11Buffer[] _constantBuffers;

        private Viewport _viewport;
        //private Int2 _scissorRectDimensions;
        //private Matrix4x4 _projectionMatrix;
        //private Matrix4x4 _viewMatrix;
        //private Matrix4x4 _worldMatrix;

        static HelloD3D11()
        {
            _featureLevels = new[]
            {
                FeatureLevel.Level_11_0
            };
        }

        public HelloD3D11(string title) : base(title)
        { }

        protected override void Initialize()
        {
            base.Initialize();

            _dxgiFactory = DXGI.CreateDXGIFactory1<IDXGIFactory2>();

            using var adapter = GetHardwareAdapter();
            var deviceCreationFlags = DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug;
            var result = D3D11.D3D11CreateDevice(
                adapter,
                DriverType.Unknown,
                deviceCreationFlags,
                _featureLevels,
                out var tempDevice,
                out var _featureLevel,
                out var tempDeviceContext);

            _device = tempDevice.QueryInterface<ID3D11Device2>();
            _deviceContext = tempDeviceContext.QueryInterface<ID3D11DeviceContext2>();

            tempDeviceContext.Dispose();
            tempDevice.Dispose();

            CreateSwapchain(Window.Hwnd, Width, Height);
            CreateSwapchainResources();

            (_vertexShader, _vertexShaderBlob) = CreateVertexShader("assets/shaders/main.vs.hlsl");
            _pixelShader = CreatePixelShader("assets/shaders/main.ps.hlsl");

            var inputLayoutDescriptor = new[]
            {
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, 12, 0),
            };
            _inputLayout = _device.CreateInputLayout(inputLayoutDescriptor, _vertexShaderBlob);

            var vertices = new Span<VertexPositionColor>(new[]
            {
                new VertexPositionColor(new Vector3(0.0f,  0.5f, 0.0f), new Color(0.25f, 0.39f, 0.19f, 1f)),
                new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.0f), new Color(0.44f, 0.75f, 0.35f, 1f)),
                new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.0f), new Color(0.38f, 0.55f, 0.20f, 1f)),
            });

            _vertexBuffer = _device.CreateBuffer(BindFlags.VertexBuffer, vertices, vertices.Length * sizeof(VertexPositionColor));
        }

        protected override void OnResize()
        {
            base.OnResize();

            _deviceContext.Flush();
            DestroySwapchainResources();

            _swapchain.ResizeBuffers(0, Width, Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);
            CreateSwapchainResources();
        }

        protected override void Render()
        {
            Color4 clearColor = new Color4(0.1f, 0.1f, 0.1f, 1.0f);

            _deviceContext.ClearRenderTargetView(_renderTarget, clearColor);

            // input assembler
            _deviceContext.IASetInputLayout(_inputLayout);
            _deviceContext.IASetVertexBuffer(0, _vertexBuffer, sizeof(VertexPositionColor), 0);
            _deviceContext.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

            // vertex shader
            _deviceContext.VSSetShader(_vertexShader);

            // rasterizer
            _deviceContext.RSSetViewport(_viewport);

            // pixel shader
            _deviceContext.PSSetShader(_pixelShader);

            // output merger
            //_deviceContext.OMSetRenderTargets(_renderTarget, _depthStencilView);
            _deviceContext.OMSetRenderTargets(_renderTarget, null);

            _deviceContext.Draw(3, 0);

            _swapchain.Present(1, PresentFlags.None);
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
            _dxgiFactory.MakeWindowAssociation(Window.Handle, WindowAssociationFlags.IgnoreAltEnter);
            _backBuffer = _swapchain.GetBuffer<ID3D11Texture2D>(0);
            _renderTarget = _device.CreateRenderTargetView(_backBuffer);

            //var depthStencilTextureDescriptor = new Texture2DDescription
            //{
            //    ArraySize = 1,
            //    BindFlags = BindFlags.DepthStencil,
            //    Width = Width,
            //    Height = Height,
            //    Format = Format.D24_UNorm_S8_UInt,
            //    MipLevels = 1,
            //    Usage = ResourceUsage.Default,
            //    CpuAccessFlags = CpuAccessFlags.None,
            //    SampleDescription = new SampleDescription(1, 0)
            //};
            //_depthStencilBuffer = _device.CreateTexture2D(depthStencilTextureDescriptor);
            //_depthStencilView = _device.CreateDepthStencilView(
            //    _depthStencilBuffer!,
            //    new DepthStencilViewDescription(
            //        _depthStencilBuffer,
            //        DepthStencilViewDimension.Texture2D));

            //_viewport = new Viewport(Width, Height);
            //_scissorRectDimensions = new Int2(Width, Height);

            //_projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
            //    MathHelper.ToRadians(60.0f),
            //    Width / (float)Height,
            //    0.1f,
            //    512.0f);
            //_deviceContext.UpdateSubresource(_projectionMatrix, _constantBuffers[0]);
        }

        private void DestroySwapchainResources()
        {
            //_depthStencilView?.Dispose();
            //_depthStencilBuffer?.Dispose();
            _renderTarget?.Dispose();
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

        private (ID3D11VertexShader vertexShader, Blob vertexShaderBlob) CreateVertexShader(string fileName)
        {
            var shader = CompileShader(fileName, "Main", "vs_5_0");
            return (_device.CreateVertexShader(shader), shader);
        }

        private ID3D11PixelShader CreatePixelShader(string fileName)
        {
            var shader = CompileShader(fileName, "Main", "ps_5_0");
            return _device.CreatePixelShader(shader);
        }

        private Blob CompileShader(string fileName, string entryPoint, string profile)
        {
            ShaderFlags compileFlags = ShaderFlags.EnableStrictness;

            Blob tempShaderBlob;
            Blob errorBlob;

            var result = Compiler.CompileFromFile(fileName, null, null, entryPoint, profile, compileFlags, out tempShaderBlob, out errorBlob);

            if (errorBlob != null)
            {
                throw new Exception(errorBlob.AsString());
            }

            return tempShaderBlob;
        }

        protected override void Dispose(bool disposing)
        {
            DestroySwapchainResources();

            _vertexShader?.Dispose();
            _pixelShader?.Dispose();
            _inputLayout?.Dispose();
            _vertexBuffer?.Dispose();

            //foreach (var constantBuffer in _constantBuffers)
            //{
            //    constantBuffer?.Dispose();
            //}

            //_textureLinearSamplerState?.Dispose();
            //_textureSrv?.Dispose();
            //_texture?.Dispose();
            //_wireFrameRasterizerState?.Dispose();
            //_solidRasterizerState?.Dispose();

            _deviceContext?.Dispose();
            _device?.Dispose();
            //_factory?.Dispose();

            //_imagingFactory?.Dispose();

            base.Dispose(disposing);
        }
    }
}
