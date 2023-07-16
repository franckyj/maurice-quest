using System.Numerics;
using System.Runtime.CompilerServices;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace MyGame.Drawable;

internal unsafe class Pyramid : IDrawable
{
    private readonly ID3D11Buffer _transformBuffer;
    private readonly Shapes.Pyramid<VertexPositionColor> _shape;
    private readonly Effects.BlendColors _effect;

    private Vector3 _position;

    private readonly float _radius;
    private readonly float _droll;
    private readonly float _dpitch;
    private readonly float _dyaw;
    private readonly float _dtheta;
    private readonly float _dphi;
    private readonly float _dchi;

    private float _roll;
    private float _pitch;
    private float _yaw;

    private float _theta;
    private float _phi;
    private float _chi;

    public Pyramid(Vector3 position, ID3D11Device device, int instance)
    {
        var random = new Random(instance);
        _radius = 6.0f + random.NextSingle() * (20.0f - 6.0f);
        _droll = random.NextSingle() * (float)Math.PI * 2.0f;
        _dpitch = random.NextSingle() * (float)Math.PI * 2.0f;
        _dyaw = random.NextSingle() * (float)Math.PI * 2.0f;
        _dphi = random.NextSingle() * (float)Math.PI * 0.3f;
        _dtheta = random.NextSingle() * (float)Math.PI * 0.3f;
        _dchi = random.NextSingle() * (float)Math.PI * 0.3f;
        _phi = random.NextSingle() * (float)Math.PI * 2.0f;
        _theta = random.NextSingle() * (float)Math.PI * 2.0f;
        _chi = random.NextSingle() * (float)Math.PI * 2.0f;

        _position = position;

        _transformBuffer = device.CreateBuffer(sizeof(Matrix4x4), BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write);

        _shape = Shapes.Pyramid<VertexPositionColor>.GetInstance(device, static (pos, i) =>
        {
            Span<Color4> colors = stackalloc Color4[]
            {
                new Color4(1.0f, 0.0f, 0.0f, 1.0f),
                new Color4(0.0f, 1.0f, 0.0f, 1.0f),
                new Color4(0.0f, 0.0f, 1.0f, 1.0f),
                new Color4(1.0f, 1.0f, 0.0f, 1.0f),
                new Color4(1.0f, 0.0f, 1.0f, 1.0f)
            };
            return new VertexPositionColor(pos, colors[i]);
        });
        _effect = Effects.BlendColors.GetInstance(device);
    }

    public void Update(
        float deltaTime,
        Matrix4x4 viewProjectionMatrix,
        ID3D11DeviceContext context,
        float mouseX,
        float mouseY,
        int width,
        int height)
    {
        _roll += deltaTime * _droll;
        _pitch += deltaTime * _dpitch;
        _yaw += deltaTime * _dyaw;
        _theta += deltaTime * _dtheta;
        _phi += deltaTime * _dphi;
        _chi += deltaTime * _dchi;

        var localTransform =
            Matrix4x4.CreateFromYawPitchRoll(_yaw, _pitch, _roll) *
            Matrix4x4.CreateTranslation(_radius, 0.0f, 0.0f) *
            Matrix4x4.CreateFromYawPitchRoll(_theta, _phi, _chi) *
            Matrix4x4.CreateTranslation(0.0f, 0.0f, -10.0f);

        var finalTransform = Matrix4x4.Multiply(localTransform, viewProjectionMatrix);
        MappedSubresource mappedResource = context.Map(_transformBuffer, MapMode.WriteDiscard);
        Unsafe.Copy(mappedResource.DataPointer.ToPointer(), ref finalTransform);
        context.Unmap(_transformBuffer, 0);
    }

    public void Draw(ID3D11DeviceContext context)
    {
        context.IASetInputLayout(_effect.Layout);
        context.IASetIndexBuffer(_shape.IndexBuffer, Format.R16_UInt, 0);
        context.IASetVertexBuffer(0, _shape.VertexBuffer, _shape.VertexBufferStride, 0);
        context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

        // vertex shader
        context.VSSetShader(_effect.VertexShader);
        context.VSSetConstantBuffer(0, _transformBuffer);

        // pixel shader
        context.PSSetShader(_effect.PixelShader);

        context.DrawIndexed(_shape.IndexCount, 0, 0);
    }
}
