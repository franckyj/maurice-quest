using System.Numerics;
using System.Runtime.CompilerServices;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace MyGame.Drawable;

internal unsafe class Box : IDrawable
{
    private readonly ID3D11Buffer _transformBuffer;
    private readonly ID3D11Buffer _modelBuffer;
    private readonly ID3D11Buffer _viewBuffer;
    private readonly ID3D11Buffer _projectionBuffer;
    private readonly Shapes.Cube<VertexPosition> _shape;
    private readonly Effects.SolidColors _effect;

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

    public Box(Vector3 position, ID3D11Device device, int instance)
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
        _modelBuffer = device.CreateBuffer(sizeof(Matrix4x4), BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write);
        _viewBuffer = device.CreateBuffer(sizeof(Matrix4x4), BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write);
        //_projectionBuffer = device.CreateBuffer(sizeof(Matrix4x4), BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write);
        _projectionBuffer = device.CreateBuffer(sizeof(Matrix4x4), BindFlags.ConstantBuffer, ResourceUsage.Default, CpuAccessFlags.None);
        _shape = Shapes.Cube<VertexPosition>.GetInstance(device, static (pos, _) => new VertexPosition(pos));
        _effect = Effects.SolidColors.GetInstance(device);
    }

    public void Update(
        float deltaTime,
        Matrix4x4 viewMatrix,
        Matrix4x4 projectionMatrix,
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

        //var localTransform =
        //    Matrix4x4.CreateFromYawPitchRoll(_yaw, _pitch, _roll) *
        //    Matrix4x4.CreateTranslation(_radius, 0.0f, 0.0f) *
        //    Matrix4x4.CreateFromYawPitchRoll(_theta, _phi, _chi) *
        //    Matrix4x4.CreateTranslation(0.0f, 0.0f, -10.0f);

        //var position = new Vector3((mouseX - (width / 2.0f)) / 10.0f, (-(mouseY - (height / 2.0f))) / 10.0f, -10.0f);

        var localTransform =
            //Matrix4x4.CreateFromYawPitchRoll(_yaw, _pitch, _roll) *
            //Matrix4x4.CreateTranslation(_radius, 0.0f, 0.0f) *
            //Matrix4x4.CreateFromYawPitchRoll(_theta, _phi, _chi) *
            Matrix4x4.CreateWorld(_position, Vector3.UnitZ, Vector3.UnitY);
            //Matrix4x4.CreateViewport(0.0f, 0.0f, width, height, 1.0f, 100.0f) *
            //Matrix4x4.CreateTranslation((mouseX - (width / 2.0f)) / 10.0f, (-(mouseY - (height / 2.0f))) / 10.0f, -10.0f);

        MappedSubresource mappedResource1 = context.Map(_modelBuffer, MapMode.WriteDiscard);
        Unsafe.Copy(mappedResource1.DataPointer.ToPointer(), ref localTransform);
        context.Unmap(_modelBuffer, 0);

        MappedSubresource mappedResource2 = context.Map(_viewBuffer, MapMode.WriteDiscard);
        Unsafe.Copy(mappedResource2.DataPointer.ToPointer(), ref viewMatrix);
        context.Unmap(_viewBuffer, 0);

        //MappedSubresource mappedResource3 = context.Map(_projectionBuffer, MapMode.WriteDiscard);
        //Unsafe.Copy(mappedResource3.DataPointer.ToPointer(), ref projectionMatrix);
        //context.Unmap(_projectionBuffer, 0);

        context.UpdateSubresource(projectionMatrix, _projectionBuffer);

        var viewProjectionMatrix = Matrix4x4.Multiply(viewMatrix, projectionMatrix);
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
        context.VSSetConstantBuffer(1, _modelBuffer);
        context.VSSetConstantBuffer(2, _viewBuffer);
        context.VSSetConstantBuffer(3, _projectionBuffer);

        // pixel shader
        context.PSSetShader(_effect.PixelShader);
        context.PSSetConstantBuffer(0, _effect.ColorBuffer);

        context.DrawIndexed(_shape.IndexCount, 0, 0);
    }
}
