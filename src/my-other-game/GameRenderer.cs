using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using Vortice.WIC;
using static MyOtherGame.ConstantBuffers;
using static MyOtherGame.GameObjects;
using static MyOtherGame.Meshes;

namespace MyOtherGame;

internal class GameRenderer
{
    private readonly DeviceResources _deviceResources;

    // ??
    private Simple3DGame _game;

    private ID3D11Buffer _constantBufferNeverChanges;
    private ID3D11Buffer _constantBufferChangeOnResize;
    private ID3D11Buffer _constantBufferChangesEveryFrame;
    private ID3D11Buffer _constantBufferChangesEveryPrim;
    private ID3D11SamplerState _samplerLinear;
    private ID3D11VertexShader _vertexShader;
    private ID3D11VertexShader _vertexShaderFlat;
    private ID3D11PixelShader _pixelShader;
    private ID3D11PixelShader _pixelShaderFlat;
    private ID3D11InputLayout _vertexLayout;

    private ID3D11ShaderResourceView _sphereTexture;
    private ID3D11ShaderResourceView _cylinderTexture;
    private ID3D11ShaderResourceView _ceilingTexture;
    private ID3D11ShaderResourceView _floorTexture;
    private ID3D11ShaderResourceView _wallsTexture;

    public GameRenderer(DeviceResources deviceResources)
    {
        _deviceResources = deviceResources;

        CreateDeviceDependentResources();
        CreateWindowSizeDependentResources();
    }

    public void CreateDeviceDependentResources()
    {

    }

    public void CreateWindowSizeDependentResources()
    {
        var context = _deviceResources.DeviceContext;
        var renderTargetSize = _deviceResources.RenderTargetSize;

        if (_game != null)
        {
            _game.Camera.UpdateAspectRatio(renderTargetSize.Width / (float)renderTargetSize.Height);

            // update 'change on resize' constant buffer
            ConstantBufferChangeOnResize changesOnResizeBuffer = new ConstantBufferChangeOnResize(_game.Camera.ProjectionMatrix);
            context.UpdateSubresource(changesOnResizeBuffer, _constantBufferChangeOnResize);
        }
    }

    public void CreateGameDeviceResources(Simple3DGame game)
    {
        _game = game;

        var device = _deviceResources.Device;

        // create the constant buffers
        // never change buffer
        unsafe
        {
            _constantBufferNeverChanges = device.CreateBuffer(
                (sizeof(ConstantBufferNeverChanges) + 15) / 16 * 16,
                BindFlags.ConstantBuffer,
                ResourceUsage.Default,
                CpuAccessFlags.None);

            _constantBufferChangeOnResize = device.CreateBuffer(
                (sizeof(ConstantBufferChangeOnResize) + 15) / 16 * 16,
                BindFlags.ConstantBuffer,
                ResourceUsage.Default,
                CpuAccessFlags.None);

            _constantBufferChangesEveryFrame = device.CreateBuffer(
                (sizeof(ConstantBufferChangesEveryFrame) + 15) / 16 * 16,
                BindFlags.ConstantBuffer,
                ResourceUsage.Default,
                CpuAccessFlags.None);

            _constantBufferChangesEveryPrim = device.CreateBuffer(
                (sizeof(ConstantBufferChangesEveryPrim) + 15) / 16 * 16,
                BindFlags.ConstantBuffer,
                ResourceUsage.Default,
                CpuAccessFlags.None);
        }

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
        _vertexShader = device.CreateVertexShader(vertexShaderBlob);
        _vertexLayout = device.CreateInputLayout(PNTVertexLayout, vertexShaderBlob);

        var pixelShaderBlob = ShaderCompiler.CompileShader("assets/shaders/PixelShader.hlsl", "main", "ps_5_0");
        _pixelShader = device.CreatePixelShader(pixelShaderBlob);

        vertexShaderBlob = ShaderCompiler.CompileShader("assets/shaders/VertexShaderFlat.hlsl", "main", "vs_5_0");
        _vertexShaderFlat = device.CreateVertexShader(vertexShaderBlob);

        pixelShaderBlob = ShaderCompiler.CompileShader("assets/shaders/PixelShaderFlat.hlsl", "main", "ps_5_0");
        _pixelShaderFlat = device.CreatePixelShader(pixelShaderBlob);

        // textures
        // make sure the previous versions if any of the textures are released.
        _sphereTexture = null;
        _cylinderTexture = null;
        _ceilingTexture = null;
        _floorTexture = null;
        _wallsTexture = null;

        var texture = LoadTexture("assets/imgs/seafloor.dds", device);
        _sphereTexture = device.CreateShaderResourceView(texture);

        texture = LoadTexture("assets/imgs/metal_texture.dds", device);
        _cylinderTexture = device.CreateShaderResourceView(texture);

        texture = LoadTexture("assets/imgs/cellceiling.dds", device);
        _ceilingTexture = device.CreateShaderResourceView(texture);

        texture = LoadTexture("assets/imgs/cellfloor.dds", device);
        _floorTexture = device.CreateShaderResourceView(texture);

        texture = LoadTexture("assets/imgs/cellwall.dds", device);
        _wallsTexture = device.CreateShaderResourceView(texture);
    }

    public void FinalizeCreateGameDeviceResources()
    {
        // all asynchronously loaded resources have completed loading.
        // now associate all the resources with the appropriate game objects.
        // this method is expected to run in the same thread as the GameRenderer
        // was created. all work will happen behind the "Loading ..." screen after the
        // main loop has been entered.

        // initialize the constant buffer with the light positions
        // these are handled here to ensure that the d3dContext is only
        // used in one thread.

        var device = _deviceResources.Device;

        var constantBufferNeverChanges = new ConstantBufferNeverChanges(
            new Vector4(3.5f, 2.5f, 5.5f, 1.0f),
            new Vector4(3.5f, 2.5f, -5.5f, 1.0f),
            new Vector4(-3.5f, 2.5f, -5.5f, 1.0f),
            new Vector4(3.5f, 2.5f, 5.5f, 1.0f),
            new Vector4(0.25f, 0.25f, 0.25f, 1.0f)
        );
        _deviceResources.DeviceContext.UpdateSubresource(constantBufferNeverChanges, _constantBufferNeverChanges);

        // meshes
        MeshObject cylinderMesh = new CylinderMesh(device, 26);
        MeshObject targetMesh = new FaceMesh(device);
        MeshObject sphereMesh = new SphereMesh(device, 26);

        var cylinderMaterial = new Material(
            new Vector4(0.8f, 0.8f, 0.8f, .5f),
            new Vector4(0.8f, 0.8f, 0.8f, .5f),
            new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
            15.0f,
            _cylinderTexture,
            _vertexShader,
            _pixelShader
        );
        var sphereMaterial = new Material(
            new Vector4(0.8f, 0.4f, 0.0f, 1.0f),
            new Vector4(0.8f, 0.4f, 0.0f, 1.0f),
            new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
            50.0f,
            _sphereTexture,
            _vertexShader,
            _pixelShader
        );

        foreach (var renderObject in _game.RenderObjects)
        {
            if (renderObject.TargetId == TargetId.WorldFloor)
            {
                renderObject.Material = new Material(
                    new Vector4(0.5f, 0.5f, 0.5f, 1.0f),
                    new Vector4(0.8f, 0.8f, 0.8f, 1.0f),
                    new Vector4(0.3f, 0.3f, 0.3f, 1.0f),
                    150.0f,
                    _floorTexture,
                    _vertexShaderFlat,
                    _pixelShaderFlat);
                renderObject.Mesh = new WorldFloorMesh(device);
            }
            else if (renderObject is CylinderObject)
            {
                renderObject.Mesh = cylinderMesh;
                renderObject.Material = cylinderMaterial;
            }
            else if (renderObject is SphereObject)
            {
                renderObject.Mesh = sphereMesh;
                renderObject.Material = sphereMaterial;
            }
        }

        var size = _deviceResources.RenderTargetSize;
        _game.Camera.SetProjParams(
            MathF.PI / 2.0f,
            size.Width / (float)size.Height,
            0.01f,
            100.0f);
    }

    public void Render()
    {
        Color4 cleanColor = new Color4(255, 127, 127);

        var d3dContext = _deviceResources.DeviceContext;
        var renderTargetView = _deviceResources.RenderTargetView;
        var depthStencilView = _deviceResources.DepthStencilView;

        //d3dContext.ClearRenderTargetView(renderTargetView, cleanColor);
        d3dContext.OMSetRenderTargets(renderTargetView, depthStencilView);
        d3dContext.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);

        var constantBufferChangesEveryFrame = new ConstantBufferChangesEveryFrame(_game.Camera.ViewMatrix);
        d3dContext.UpdateSubresource(constantBufferChangesEveryFrame, _constantBufferChangesEveryFrame);

        d3dContext.IASetInputLayout(_vertexLayout);
        d3dContext.VSSetConstantBuffers(0, 4,
            new ID3D11Buffer[] {
                _constantBufferNeverChanges,
                _constantBufferChangeOnResize,
                _constantBufferChangesEveryFrame,
                _constantBufferChangesEveryPrim
            });

        d3dContext.PSSetConstantBuffers(2, 2,
            new ID3D11Buffer[] {
                _constantBufferChangesEveryFrame,
                _constantBufferChangesEveryPrim
            });
        d3dContext.PSSetSampler(0, _samplerLinear);

        foreach (var renderObject in _game.RenderObjects)
        {
            renderObject.Render(d3dContext, _constantBufferChangesEveryPrim);
        }
    }

    #region load-textures

    private static readonly Dictionary<Guid, Guid> s_WICConvert = new()
    {
        // Note target GUID in this conversion table must be one of those directly supported formats (above).

        { PixelFormat.FormatBlackWhite,            PixelFormat.Format8bppGray }, // DXGI_FORMAT_R8_UNORM

        { PixelFormat.Format1bppIndexed,           PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
        { PixelFormat.Format2bppIndexed,           PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
        { PixelFormat.Format4bppIndexed,           PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
        { PixelFormat.Format8bppIndexed,           PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM

        { PixelFormat.Format2bppGray,              PixelFormat.Format8bppGray }, // DXGI_FORMAT_R8_UNORM
        { PixelFormat.Format4bppGray,              PixelFormat.Format8bppGray }, // DXGI_FORMAT_R8_UNORM

        { PixelFormat.Format16bppGrayFixedPoint,   PixelFormat.Format16bppGrayHalf }, // DXGI_FORMAT_R16_FLOAT
        { PixelFormat.Format32bppGrayFixedPoint,   PixelFormat.Format32bppGrayFloat }, // DXGI_FORMAT_R32_FLOAT

        { PixelFormat.Format16bppBGR555,           PixelFormat.Format16bppBGRA5551 }, // DXGI_FORMAT_B5G5R5A1_UNORM

        { PixelFormat.Format32bppBGR101010,        PixelFormat.Format32bppRGBA1010102 }, // DXGI_FORMAT_R10G10B10A2_UNORM

        { PixelFormat.Format24bppBGR,              PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
        { PixelFormat.Format24bppRGB,              PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
        { PixelFormat.Format32bppPBGRA,            PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
        { PixelFormat.Format32bppPRGBA,            PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM

        { PixelFormat.Format48bppRGB,              PixelFormat.Format64bppRGBA }, // DXGI_FORMAT_R16G16B16A16_UNORM
        { PixelFormat.Format48bppBGR,              PixelFormat.Format64bppRGBA }, // DXGI_FORMAT_R16G16B16A16_UNORM
        { PixelFormat.Format64bppBGRA,             PixelFormat.Format64bppRGBA }, // DXGI_FORMAT_R16G16B16A16_UNORM
        { PixelFormat.Format64bppPRGBA,            PixelFormat.Format64bppRGBA }, // DXGI_FORMAT_R16G16B16A16_UNORM
        { PixelFormat.Format64bppPBGRA,            PixelFormat.Format64bppRGBA }, // DXGI_FORMAT_R16G16B16A16_UNORM

        { PixelFormat.Format48bppRGBFixedPoint,    PixelFormat.Format64bppRGBAHalf }, // DXGI_FORMAT_R16G16B16A16_FLOAT
        { PixelFormat.Format48bppBGRFixedPoint,    PixelFormat.Format64bppRGBAHalf }, // DXGI_FORMAT_R16G16B16A16_FLOAT
        { PixelFormat.Format64bppRGBAFixedPoint,   PixelFormat.Format64bppRGBAHalf }, // DXGI_FORMAT_R16G16B16A16_FLOAT
        { PixelFormat.Format64bppBGRAFixedPoint,   PixelFormat.Format64bppRGBAHalf }, // DXGI_FORMAT_R16G16B16A16_FLOAT
        { PixelFormat.Format64bppRGBFixedPoint,    PixelFormat.Format64bppRGBAHalf }, // DXGI_FORMAT_R16G16B16A16_FLOAT
        { PixelFormat.Format64bppRGBHalf,          PixelFormat.Format64bppRGBAHalf }, // DXGI_FORMAT_R16G16B16A16_FLOAT
        { PixelFormat.Format48bppRGBHalf,          PixelFormat.Format64bppRGBAHalf }, // DXGI_FORMAT_R16G16B16A16_FLOAT

        { PixelFormat.Format128bppPRGBAFloat,      PixelFormat.Format128bppRGBAFloat }, // DXGI_FORMAT_R32G32B32A32_FLOAT
        { PixelFormat.Format128bppRGBFloat,        PixelFormat.Format128bppRGBAFloat }, // DXGI_FORMAT_R32G32B32A32_FLOAT
        { PixelFormat.Format128bppRGBAFixedPoint,  PixelFormat.Format128bppRGBAFloat }, // DXGI_FORMAT_R32G32B32A32_FLOAT
        { PixelFormat.Format128bppRGBFixedPoint,   PixelFormat.Format128bppRGBAFloat }, // DXGI_FORMAT_R32G32B32A32_FLOAT
        { PixelFormat.Format32bppRGBE,             PixelFormat.Format128bppRGBAFloat }, // DXGI_FORMAT_R32G32B32A32_FLOAT

        { PixelFormat.Format32bppCMYK,             PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
        { PixelFormat.Format64bppCMYK,             PixelFormat.Format64bppRGBA }, // DXGI_FORMAT_R16G16B16A16_UNORM
        { PixelFormat.Format40bppCMYKAlpha,        PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
        { PixelFormat.Format80bppCMYKAlpha,        PixelFormat.Format64bppRGBA }, // DXGI_FORMAT_R16G16B16A16_UNORM

        { PixelFormat.Format32bppRGB,              PixelFormat.Format32bppRGBA }, // DXGI_FORMAT_R8G8B8A8_UNORM
        { PixelFormat.Format64bppRGB,              PixelFormat.Format64bppRGBA }, // DXGI_FORMAT_R16G16B16A16_UNORM
        { PixelFormat.Format64bppPRGBAHalf,        PixelFormat.Format64bppRGBAHalf }, // DXGI_FORMAT_R16G16B16A16_FLOAT

        // We don't support n-channel formats
    };

    private static ID3D11Texture2D? LoadTexture(string fileName, ID3D11Device device, int width = 0, int height = 0)
    {
        //string assetsPath = Path.Combine(AppContext.BaseDirectory, "Textures");
        //string textureFile = Path.Combine(assetsPath, fileName);

        string textureFile = Path.Combine(AppContext.BaseDirectory, fileName);

        using var wicFactory = new IWICImagingFactory();
        using IWICBitmapDecoder decoder = wicFactory.CreateDecoderFromFileName(textureFile);
        using IWICBitmapFrameDecode frame = decoder.GetFrame(0);

        Size size = frame.Size;

        // Determine format
        Guid pixelFormat = frame.PixelFormat;
        Guid convertGUID = pixelFormat;

        bool useWIC2 = true;
        Format format = PixelFormat.ToDXGIFormat(pixelFormat);
        int bpp = 0;
        if (format == Format.Unknown)
        {
            if (pixelFormat == PixelFormat.Format96bppRGBFixedPoint)
            {
                if (useWIC2)
                {
                    convertGUID = PixelFormat.Format96bppRGBFixedPoint;
                    format = Format.R32G32B32_Float;
                    bpp = 96;
                }
                else
                {
                    convertGUID = PixelFormat.Format128bppRGBAFloat;
                    format = Format.R32G32B32A32_Float;
                    bpp = 128;
                }
            }
            else
            {
                foreach (KeyValuePair<Guid, Guid> item in s_WICConvert)
                {
                    if (item.Key == pixelFormat)
                    {
                        convertGUID = item.Value;

                        format = PixelFormat.ToDXGIFormat(item.Value);
                        Debug.Assert(format != Format.Unknown);
                        bpp = PixelFormat.WICBitsPerPixel(wicFactory, convertGUID);
                        break;
                    }
                }
            }

            if (format == Format.Unknown)
            {
                throw new InvalidOperationException("WICTextureLoader does not support all DXGI formats");
                //Debug.WriteLine("ERROR: WICTextureLoader does not support all DXGI formats (WIC GUID {%8.8lX-%4.4X-%4.4X-%2.2X%2.2X-%2.2X%2.2X%2.2X%2.2X%2.2X%2.2X}). Consider using DirectXTex.\n",
                //    pixelFormat.Data1, pixelFormat.Data2, pixelFormat.Data3,
                //    pixelFormat.Data4[0], pixelFormat.Data4[1], pixelFormat.Data4[2], pixelFormat.Data4[3],
                //    pixelFormat.Data4[4], pixelFormat.Data4[5], pixelFormat.Data4[6], pixelFormat.Data4[7]);
            }
        }
        else
        {
            // Convert BGRA8UNorm to RGBA8Norm
            if (pixelFormat == PixelFormat.Format32bppBGRA)
            {
                format = PixelFormat.ToDXGIFormat(PixelFormat.Format32bppRGBA);
                convertGUID = PixelFormat.Format32bppRGBA;
            }

            bpp = PixelFormat.WICBitsPerPixel(wicFactory, pixelFormat);
        }

        if (format == Format.R32G32B32_Float)
        {
            // Special case test for optional device support for autogen mipchains for R32G32B32_FLOAT
            FormatSupport fmtSupport = device.CheckFormatSupport(Format.R32G32B32_Float);
            if (!fmtSupport.HasFlag(FormatSupport.MipAutogen))
            {
                // Use R32G32B32A32_FLOAT instead which is required for Feature Level 10.0 and up
                convertGUID = PixelFormat.Format128bppRGBAFloat;
                format = Format.R32G32B32A32_Float;
                bpp = 128;
            }
        }

        // Verify our target format is supported by the current device
        // (handles WDDM 1.0 or WDDM 1.1 device driver cases as well as DirectX 11.0 Runtime without 16bpp format support)
        FormatSupport support = device.CheckFormatSupport(format);
        if (!support.HasFlag(FormatSupport.Texture2D))
        {
            // Fallback to RGBA 32-bit format which is supported by all devices
            convertGUID = PixelFormat.Format32bppRGBA;
            format = Format.R8G8B8A8_UNorm;
            bpp = 32;
        }

        int rowPitch = (size.Width * bpp + 7) / 8;
        int sizeInBytes = rowPitch * size.Height;

        byte[] pixels = new byte[sizeInBytes];

        if (width == 0)
            width = size.Width;

        if (height == 0)
            height = size.Height;

        // Load image data
        if (convertGUID == pixelFormat && size.Width == width && size.Height == height)
        {
            // No format conversion or resize needed
            frame.CopyPixels(rowPitch, pixels);
        }
        else if (size.Width != width || size.Height != height)
        {
            // Resize
            using IWICBitmapScaler scaler = wicFactory.CreateBitmapScaler();
            scaler.Initialize(frame, width, height, BitmapInterpolationMode.Fant);

            Guid pixelFormatScaler = scaler.PixelFormat;

            if (convertGUID == pixelFormatScaler)
            {
                // No format conversion needed
                scaler.CopyPixels(rowPitch, pixels);
            }
            else
            {
                using IWICFormatConverter converter = wicFactory.CreateFormatConverter();

                bool canConvert = converter.CanConvert(pixelFormatScaler, convertGUID);
                if (!canConvert)
                {
                    return null;
                }

                converter.Initialize(scaler, convertGUID, BitmapDitherType.ErrorDiffusion, null, 0, BitmapPaletteType.MedianCut);
                converter.CopyPixels(rowPitch, pixels);
            }
        }
        else
        {
            // Format conversion but no resize
            using IWICFormatConverter converter = wicFactory.CreateFormatConverter();

            bool canConvert = converter.CanConvert(pixelFormat, convertGUID);
            if (!canConvert)
            {
                return null;
            }

            converter.Initialize(frame, convertGUID, BitmapDitherType.ErrorDiffusion, null, 0, BitmapPaletteType.MedianCut);
            converter.CopyPixels(rowPitch, pixels);
        }

        return device.CreateTexture2D(pixels, format, size.Width, size.Height);
    }

    #endregion
}
