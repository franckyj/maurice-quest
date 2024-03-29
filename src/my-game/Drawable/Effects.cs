﻿using System.Diagnostics;
using System.Drawing;
using Vortice.DCommon;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using Vortice.WIC;
using PixelFormat = Vortice.WIC.PixelFormat;

namespace MyGame.Drawable;

internal static class Effects
{
    #region solid colors

    public unsafe class SolidColors
    {
        private static SolidColors _instance;

        public static SolidColors GetInstance(ID3D11Device device)
        {
            _instance ??= new SolidColors(device);
            return _instance;
        }

        public ID3D11Buffer ColorBuffer { get; private set; }

        public ID3D11VertexShader VertexShader { get; private set; }
        public ID3D11PixelShader PixelShader { get; private set; }

        public ID3D11InputLayout Layout { get; private set; }

        public SolidColors(ID3D11Device device)
        {
            Span<Color4> colors = stackalloc Color4[]
            {
                new Color4(1.0f, 0.0f, 0.0f, 1.0f),
                new Color4(0.0f, 1.0f, 0.0f, 1.0f),
                new Color4(0.0f, 0.0f, 1.0f, 1.0f),
                new Color4(1.0f, 1.0f, 0.0f, 1.0f),
                new Color4(1.0f, 0.0f, 1.0f, 1.0f),
                new Color4(0.0f, 1.0f, 1.0f, 1.0f)
            };
            ColorBuffer = device.CreateBuffer(colors, BindFlags.ConstantBuffer);

            var vertexShaderBlob = ShaderCompiler.CompileShader("assets/shaders/solidcolors.vs.hlsl", "Main", "vs_5_0");
            VertexShader = device.CreateVertexShader(vertexShaderBlob);

            var pixelShaderBlob = ShaderCompiler.CompileShader("assets/shaders/solidcolors.ps.hlsl", "Main", "ps_5_0");
            PixelShader = device.CreatePixelShader(pixelShaderBlob);

            var inputLayoutDescriptor = new InputElementDescription[]
            {
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
            };
            Layout = device.CreateInputLayout(inputLayoutDescriptor, vertexShaderBlob);
        }
    }

    #endregion

    #region blend colors

    public unsafe class BlendColors
    {
        private static BlendColors _instance;

        public static BlendColors GetInstance(ID3D11Device device)
        {
            _instance ??= new BlendColors(device);
            return _instance;
        }

        public ID3D11VertexShader VertexShader { get; private set; }
        public ID3D11PixelShader PixelShader { get; private set; }

        public ID3D11InputLayout Layout { get; private set; }

        public BlendColors(ID3D11Device device)
        {
            var vertexShaderBlob = ShaderCompiler.CompileShader("assets/shaders/blendcolors.vs.hlsl", "Main", "vs_5_0");
            VertexShader = device.CreateVertexShader(vertexShaderBlob);

            var pixelShaderBlob = ShaderCompiler.CompileShader("assets/shaders/blendcolors.ps.hlsl", "Main", "ps_5_0");
            PixelShader = device.CreatePixelShader(pixelShaderBlob);

            var inputLayoutDescriptor = new InputElementDescription[]
            {
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("COLOR", 0, Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
            };
            Layout = device.CreateInputLayout(inputLayoutDescriptor, vertexShaderBlob);
        }
    }

    #endregion

    #region textured

    //public unsafe class Textured
    //{
    //    private static Textured _instance;

    //    public static Textured GetInstance(ID3D11Device device)
    //    {
    //        _instance ??= new Textured(device);
    //        return _instance;
    //    }

    //    public ID3D11Buffer ColorBuffer { get; private set; }

    //    public ID3D11VertexShader VertexShader { get; private set; }
    //    public ID3D11PixelShader PixelShader { get; private set; }

    //    public ID3D11InputLayout Layout { get; private set; }

    //    public SolidColors(ID3D11Device device)
    //    {
    //        Span<Color4> colors = stackalloc Color4[]
    //        {
    //            new Color4(1.0f, 0.0f, 0.0f, 1.0f),
    //            new Color4(0.0f, 1.0f, 0.0f, 1.0f),
    //            new Color4(0.0f, 0.0f, 1.0f, 1.0f),
    //            new Color4(1.0f, 1.0f, 0.0f, 1.0f),
    //            new Color4(1.0f, 0.0f, 1.0f, 1.0f),
    //            new Color4(0.0f, 1.0f, 1.0f, 1.0f)
    //        };
    //        ColorBuffer = device.CreateBuffer(colors, BindFlags.ConstantBuffer);

    //        var vertexShaderBlob = ShaderCompiler.CompileShader("assets/shaders/solidcolors.vs.hlsl", "Main", "vs_5_0");
    //        VertexShader = device.CreateVertexShader(vertexShaderBlob);

    //        var pixelShaderBlob = ShaderCompiler.CompileShader("assets/shaders/solidcolors.ps.hlsl", "Main", "ps_5_0");
    //        PixelShader = device.CreatePixelShader(pixelShaderBlob);

    //        var inputLayoutDescriptor = new InputElementDescription[]
    //        {
    //            new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
    //        };
    //        Layout = device.CreateInputLayout(inputLayoutDescriptor, vertexShaderBlob);
    //    }
    //}

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
        string assetsPath = Path.Combine(AppContext.BaseDirectory, "Textures");
        string textureFile = Path.Combine(assetsPath, fileName);

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
