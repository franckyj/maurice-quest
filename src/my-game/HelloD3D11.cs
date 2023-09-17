using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace MyGame
{
    #region vortice sample

    public readonly struct VertexPositionNormalTexture
    {
        public static unsafe readonly int SizeInBytes = sizeof(VertexPositionNormalTexture);

        public static InputElementDescription[] InputElements = new[]
        {
            new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
            new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
        };

        public VertexPositionNormalTexture(
            in Vector3 position,
            in Vector3 normal,
            in Vector2 textureCoordinate)
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
        }

        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Vector2 TextureCoordinate;
    }

    public class MeshData
    {
        public VertexPositionNormalTexture[] Vertices;
        public ushort[] Indices;

        public MeshData(VertexPositionNormalTexture[] vertices, ushort[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
    }

    public static class MeshUtilities
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
                new Vector3(0.0f, -1.0f, 0.0f)
            };

            Vector3[] faceColors = new Vector3[CubeFaceCount]
            {
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 1.0f)
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
                    //normal,
                    new Vector3(0.0f, 1.0f, 0.0f),
                    textureCoordinates[0]
                    ));

                // (normal - side1 + side2) * tsize // normal // t1
                vertices.Add(new VertexPositionNormalTexture(
                    Vector3.Multiply(Vector3.Add(Vector3.Subtract(normal, side1), side2), tsize),
                    //normal,
                    faceColors[i],
                    textureCoordinates[1]
                    ));

                // (normal + side1 + side2) * tsize // normal // t2
                vertices.Add(new VertexPositionNormalTexture(
                    Vector3.Multiply(Vector3.Add(normal, Vector3.Add(side1, side2)), tsize),
                    //normal,
                    faceColors[i],
                    textureCoordinates[2]
                    ));

                // (normal + side1 - side2) * tsize // normal // t3
                vertices.Add(new VertexPositionNormalTexture(
                    Vector3.Multiply(Vector3.Subtract(Vector3.Add(normal, side1), side2), tsize),
                    //normal,
                    faceColors[i],
                    textureCoordinates[3]
                    ));

                vbase += 4;
            }

            return new MeshData(vertices.ToArray(), indices.ToArray());
        }
    }

    #endregion

    internal unsafe sealed class HelloD3D11 : Application
    {
        private static readonly FeatureLevel[] _featureLevels;

        private IDXGIFactory7 _dxgiFactory;
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
        private ID3D11Buffer _indexBuffer;
        private float _angleInRadians = 0;
        private float _mouseX = 0;
        private float _mouseY= 0;
        private ID3D11Buffer _transformBuffer;
        private ID3D11Buffer _colorBuffer;
        private ID3D11DepthStencilState _depthState;
        private ID3D11DepthStencilView _depthView;
        private ID3D11RasterizerState _rasterizerState;

        // debug layer https://github.com/amerkoleci/Vortice.Windows/issues/170#issuecomment-903089686

        private Viewport _viewport;

        static HelloD3D11()
        {
            _featureLevels = new[]
            {
                FeatureLevel.Level_11_0
            };
        }

        public HelloD3D11(string title) : base(title)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            _dxgiFactory = DXGI.CreateDXGIFactory2<IDXGIFactory7>(false);

            using var adapter = GetHardwareAdapter();
            var deviceCreationFlags = DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug;
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
        }

        protected override void Load()
        {
            base.Load();

            //(_vertexShader, _vertexShaderBlob) = CreateVertexShader("assets/shaders/main.both.hlsl", "VSMain");
            //_pixelShader = CreatePixelShader("assets/shaders/main.both.hlsl", "PSMain");

            (_vertexShader, _vertexShaderBlob) = CreateVertexShader("assets/shaders/solidcolors.vs.hlsl", "Main");
            _pixelShader = CreatePixelShader("assets/shaders/solidcolors.ps.hlsl", "Main");

            var inputLayoutDescriptor = new[]
            {
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0)
            };
            _inputLayout = _device.CreateInputLayout(inputLayoutDescriptor, _vertexShaderBlob);
            //_inputLayout = _device.CreateInputLayout(VertexPositionNormalTexture.InputElements, _vertexShaderBlob);

            //MeshData mesh = MeshUtilities.CreateCube(2.0f);
            //_vertexBuffer = _device.CreateBuffer(mesh.Vertices, BindFlags.VertexBuffer);
            //_indexBuffer = _device.CreateBuffer(mesh.Indices, BindFlags.IndexBuffer);

            //Span<VertexPositionColor> vertices = stackalloc VertexPositionColor[]
            //{
            //    new VertexPositionColor(new Vector3(-1.0f, -1.0f, 1.0f), new Color4(0f, 0f, 0f, 1f)),
            //    new VertexPositionColor(new Vector3(1.0f, -1.0f, 1.0f), new Color4(1f, 0f, 0f, 1f)),
            //    new VertexPositionColor(new Vector3(-1.0f, 1.0f, 1.0f), new Color4(0f, 1f, 0f, 1f)),
            //    new VertexPositionColor(new Vector3(1.0f, 1.0f, 1.0f), new Color4(1f, 1f, 1f, 1f)),
            //    new VertexPositionColor(new Vector3(-1.0f, -1.0f, -1.0f), new Color4(1f, 1f, 1f, 1f)),
            //    new VertexPositionColor(new Vector3(1.0f, -1.0f, -1.0f), new Color4(1f, 1f, 1f, 1f)),
            //    new VertexPositionColor(new Vector3(-1.0f, 1.0f, -1.0f), new Color4(1f, 1f, 1f, 1f)),
            //    new VertexPositionColor(new Vector3(1.0f, 1.0f, -1.0f), new Color4(1f, 1f, 1f, 1f))
            //};
            Span<VertexPosition> vertices = stackalloc VertexPosition[]
            {
                new VertexPosition(new Vector3(-1.0f, -1.0f, 1.0f)),
                new VertexPosition(new Vector3(1.0f, -1.0f, 1.0f)),
                new VertexPosition(new Vector3(-1.0f, 1.0f, 1.0f)),
                new VertexPosition(new Vector3(1.0f, 1.0f, 1.0f)),
                new VertexPosition(new Vector3(-1.0f, -1.0f, -1.0f)),
                new VertexPosition(new Vector3(1.0f, -1.0f, -1.0f)),
                new VertexPosition(new Vector3(-1.0f, 1.0f, -1.0f)),
                new VertexPosition(new Vector3(1.0f, 1.0f, -1.0f))
            };
            _vertexBuffer = _device.CreateBuffer(vertices, BindFlags.VertexBuffer);

            Span<ushort> indices = stackalloc ushort[]
            {
                0, 2, 1, 2, 3, 1,
                1, 3, 5, 3, 7, 5,
                2, 6, 3, 3, 6, 7,
                4, 5, 7, 4, 7, 6,
                0, 4, 2, 2, 4, 6,
                0, 1, 4, 1, 5, 4
            };
            _indexBuffer = _device.CreateBuffer(indices, BindFlags.IndexBuffer);

            _transformBuffer = _device.CreateBuffer(sizeof(Matrix4x4), BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write);

            Span<Color4> colors = stackalloc Color4[]
            {
                new Color4(1.0f, 0.0f, 0.0f, 1.0f),
                new Color4(0.0f, 1.0f, 0.0f, 1.0f),
                new Color4(0.0f, 0.0f, 1.0f, 1.0f),
                new Color4(1.0f, 1.0f, 0.0f, 1.0f),
                new Color4(1.0f, 0.0f, 1.0f, 1.0f),
                new Color4(0.0f, 1.0f, 1.0f, 1.0f)
            };
            _colorBuffer = _device.CreateBuffer(colors, BindFlags.ConstantBuffer);

            var rasterizerDesc = new RasterizerDescription(CullMode.Back, FillMode.Solid);
            _rasterizerState = _device.CreateRasterizerState(rasterizerDesc);

            _depthState = _device.CreateDepthStencilState(new DepthStencilDescription()
            {
                DepthEnable = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthFunc = ComparisonFunction.Less
            });
            _deviceContext.OMSetDepthStencilState(_depthState);
        }

        protected override void OnResize()
        {
            base.OnResize();

            _deviceContext.Flush();
            DestroySwapchainResources();

            _swapchain.ResizeBuffers(0, Width, Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);
            CreateSwapchainResources();
        }

        protected override void Update(float mouseX, float mouseY)
        {
            _angleInRadians += 0.01f;

            _mouseX = mouseX;
            _mouseY = mouseY;

            base.Update(mouseX, mouseY);
        }

        protected override void Render()
        {
            Color4 clearColor = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            _deviceContext.ClearRenderTargetView(_renderTarget, clearColor);
            _deviceContext.ClearDepthStencilView(_depthView, DepthStencilClearFlags.Depth, 1.0f, 0);

            var mouseX = (float)_mouseX / (Width / 2.0f) - 1.0f;
            var mouseY = -(float)_mouseY / (Height / 2.0f) + 1.0f;

            //DrawCube(0, 0, 0, _angleInRadians);
            DrawCube(mouseX, 0.0f, mouseY * 1.5f, -_angleInRadians);

            _swapchain.Present(1, PresentFlags.None);
        }

        private void DrawCube(float translateX, float translateY, float translateZ, float angle)
        {
            Matrix4x4 world =
                Matrix4x4.CreateRotationX(angle) *
                Matrix4x4.CreateRotationY(angle) *
                Matrix4x4.CreateRotationZ(angle * .7f);
                //* Matrix4x4.CreateTranslation(new Vector3(translateX, translateY, translateZ));
            Matrix4x4 view = Matrix4x4.CreateLookAt(new Vector3(0, 0, 5), new Vector3(0, 0, 0), Vector3.UnitY);
            Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4, AspectRatio, 1.0f, 100.0f);
            Matrix4x4 viewProjection = Matrix4x4.Multiply(view, projection);
            Matrix4x4 worldViewProjection = Matrix4x4.Multiply(world, viewProjection);

            // update constant buffer data
            MappedSubresource mappedResource = _deviceContext.Map(_transformBuffer, MapMode.WriteDiscard);
            Unsafe.Copy(mappedResource.DataPointer.ToPointer(), ref worldViewProjection);
            _deviceContext.Unmap(_transformBuffer, 0);

            // input assembler
            _deviceContext.IASetInputLayout(_inputLayout);
            _deviceContext.IASetIndexBuffer(_indexBuffer, Format.R16_UInt, 0);
            //_deviceContext.IASetVertexBuffer(0, _vertexBuffer, sizeof(VertexPositionColor), 0);
            _deviceContext.IASetVertexBuffer(0, _vertexBuffer, sizeof(VertexPosition), 0);
            //_deviceContext.IASetVertexBuffer(0, _vertexBuffer, VertexPositionNormalTexture.SizeInBytes, 0);
            _deviceContext.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

            // vertex shader
            _deviceContext.VSSetShader(_vertexShader);
            _deviceContext.VSSetConstantBuffer(0, _transformBuffer);

            // rasterizer
            _deviceContext.RSSetState(_rasterizerState);
            _deviceContext.RSSetViewport(_viewport);

            // pixel shader
            _deviceContext.PSSetShader(_pixelShader);
            _deviceContext.PSSetConstantBuffer(0, _colorBuffer);

            // output merger
            _deviceContext.OMSetRenderTargets(_renderTarget, _depthView);

            _deviceContext.DrawIndexed(36, 0, 0);
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

            _viewport = new Viewport(Width, Height);
        }

        private void DestroySwapchainResources()
        {
            //_deviceContext.UnsetRenderTargets();
            _renderTarget?.Dispose();
            _depthView.Dispose();
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

        private (ID3D11VertexShader vertexShader, Blob vertexShaderBlob) CreateVertexShader(string fileName, string main)
        {
            var shader = CompileShader(fileName, main, "vs_5_0");
            return (_device.CreateVertexShader(shader), shader);
        }

        private ID3D11PixelShader CreatePixelShader(string fileName, string main)
        {
            var shader = CompileShader(fileName, main, "ps_5_0");
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

            _deviceContext?.Dispose();
            _device?.Dispose();

            base.Dispose(disposing);
        }
    }
}
