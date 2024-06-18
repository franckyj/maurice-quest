using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using MyOtherOtherGame.Assets;
using MyOtherOtherGame.Graphics;
using MyOtherOtherGame.Voxels;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.WIC;
using static MyOtherOtherGame.Graphics.ConstantBuffers;
using static MyOtherOtherGame.Graphics.Vertices;

namespace MyOtherOtherGame;

internal class MyGame : D3D11Application
{
    private readonly Camera _camera;

    // buffers
    private ID3D11Buffer _constantBufferNeverChanges;
    private ID3D11Buffer _constantBufferChangeOnResize;
    private ID3D11Buffer _constantBufferChangesEveryFrame;
    private ID3D11Buffer _constantBufferChangesEveryPrim;

    private ID3D11ShaderResourceView _texture;

    private ID3D11SamplerState _samplerLinear;
    private ID3D11VertexShader _vertexShader;
    private ID3D11PixelShader _pixelShader;
    private ID3D11InputLayout _vertexLayout;

    private readonly WorldRenderer _renderer;

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
            1000);
        _camera.SetViewParams(new Vector3(50, 50, 50), Vector3.Zero, Vector3.UnitY);

        CreateGameDeviceResources();

        _renderer = new WorldRenderer(_d3d);

        BindInput();
    }

    private void CreateGameDeviceResources()
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
            size,
            BindFlags.ConstantBuffer,
            ResourceUsage.Default,
            CpuAccessFlags.None);

        size = (Unsafe.SizeOf<ConstantBufferChangesEveryFrame>() + 15) / 16 * 16;
        //size = Unsafe.SizeOf<ConstantBufferChangesEveryFrame>();
        _constantBufferChangesEveryFrame = device.CreateBuffer(
            size,
            BindFlags.ConstantBuffer,
            ResourceUsage.Default,
            CpuAccessFlags.None);

        size = (Unsafe.SizeOf<ConstantBufferChangesEveryPrim>() + 15) / 16 * 16;
        //size = Unsafe.SizeOf<ConstantBufferChangesEveryPrim>();
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

        var texture = LoadTexture("assets/images/cellceiling.dds", device);
        _texture = device.CreateShaderResourceView(texture);
    }

    public override void Render(double farseer)
    {
        _d3d.BeginFrame();

        var constantBufferChangesEveryFrame = new ConstantBufferChangesEveryFrame(_camera.ViewMatrix);
        _d3d.DeviceContext.UpdateSubresource(constantBufferChangesEveryFrame, _constantBufferChangesEveryFrame);

        _d3d.DeviceContext.IASetInputLayout(_vertexLayout);
        _d3d.DeviceContext.VSSetConstantBuffers(0, 4,
            new ID3D11Buffer[] {
                _constantBufferNeverChanges,
                _constantBufferChangeOnResize,
                _constantBufferChangesEveryFrame,
                _constantBufferChangesEveryPrim,
            });

        _d3d.DeviceContext.PSSetConstantBuffers(2, 2,
            new ID3D11Buffer[] {
                _constantBufferChangesEveryFrame,
                _constantBufferChangesEveryPrim
            });
        _d3d.DeviceContext.PSSetSampler(0, _samplerLinear);

        //Matrix4x4 viewProj = Matrix4x4.Multiply(_camera.ViewMatrix, _camera.ProjectionMatrix);

        // set the shaders
        _d3d.DeviceContext.VSSetShader(_vertexShader, null, 0);
        _d3d.DeviceContext.PSSetShader(_pixelShader, null, 0);

        _d3d.DeviceContext.PSSetShaderResource(0, _texture);

        _renderer.Render(_constantBufferChangesEveryPrim);

        _d3d.Present();
    }

    public override void Update(TimeSpan deltaTime)
    {
        // do nothing
    }

    private void BindInput()
    {
        //_input.SetOnLeftPressed(() =>
        //{
        //    _gameObjects[0].Position = _gameObjects[0].Position with { X = _gameObjects[0].Position.X - 1 };
        //    _gameObjects[0].UpdateModelMatrix();
        //});
        //_input.SetOnRightPressed(() =>
        //{
        //    _gameObjects[0].Position = _gameObjects[0].Position with { X = _gameObjects[0].Position.X + 1 };
        //    _gameObjects[0].UpdateModelMatrix();
        //});
        //_input.SetOnUpPressed(() =>
        //{
        //    _gameObjects[0].Position = _gameObjects[0].Position with { Y = _gameObjects[0].Position.Y + 1 };
        //    _gameObjects[0].UpdateModelMatrix();
        //});
        //_input.SetOnDownPressed(() =>
        //{
        //    _gameObjects[0].Position = _gameObjects[0].Position with { Y = _gameObjects[0].Position.Y - 1 };
        //    _gameObjects[0].UpdateModelMatrix();
        //});
        _input.SetOnZoomInPressed(() =>
        {
            var newEye = new Vector3(_camera.Eye.X - 3, _camera.Eye.Y - 3, _camera.Eye.Z - 3);
            _camera.SetEyePosition(newEye);
        });
        _input.SetOnZoomOutPressed(() =>
        {
            var newEye = new Vector3(_camera.Eye.X + 3, _camera.Eye.Y + 3, _camera.Eye.Z + 3);
            _camera.SetEyePosition(newEye);
        });
        //_input.SetOnDragRotating(rotation =>
        //{
        //    _gameObjects[0].Rotation = Vector3.Add(_gameObjects[0].Rotation, rotation);
        //    _gameObjects[0].UpdateModelMatrix();
        //});
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
