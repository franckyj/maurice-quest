using System.Numerics;
using Vortice.Mathematics;

namespace MyOtherOtherGame.Graphics;

internal class Camera
{
    public Matrix4x4 ViewMatrix { get; private set; }
    public Matrix4x4 ProjectionMatrix { get; private set; }
    public Vector3 Position { get; private set; }
    public Vector3 LookAt { get; private set; }
    public static Vector3 Up => Vector3.UnitY;

    public float FieldOfView { get; private set; }
    public float AspectRatio { get; private set; }

    public float Width { get; private set; }
    public float Height { get; private set; }

    public float NearPlane { get; private set; }
    public float FarPlane { get; private set; }

    private Matrix4x4 _isometric;

    public Camera()
    {
        _isometric = Matrix4x4.CreateFromYawPitchRoll(
            MathHelper.ToRadians(45),
            MathHelper.ToRadians(-30),
            0);

        SetIsometric(Vector3.Zero);
    }

    public void SetIsometric(Vector3 position)
    {
        Position = position;

        //ViewMatrix = Matrix4x4.CreateWorld(position, -Vector3.UnitZ, Vector3.UnitY);
        //ViewMatrix = Matrix4x4.Multiply(_isometric, ViewMatrix);

        //Matrix4x4.Invert(ViewMatrix, out var inverse);
        //ViewMatrix = inverse;

        var cameraWorld = Matrix4x4.CreateWorld(position, Vector3.Transform(-Vector3.UnitZ, _isometric), Vector3.Transform(Vector3.UnitY, _isometric));
        Matrix4x4.Invert(cameraWorld, out Matrix4x4 view);
        ViewMatrix = view;
    }

    //public void SetViewParams(in Vector3 position, in Vector3 lookAt, in Vector3 up)
    //{
    //    Position = position;
    //    LookAt = lookAt;
    //    Up = up;

    //    // Calculate the view matrix.
    //    ViewMatrix = Matrix4x4.CreateLookAt(Position, LookAt, Up);
    //}

    public void SetPerspective(in float fieldOfView, in float aspectRatio, in float nearPlane, in float farPlane)
    {
        FieldOfView = fieldOfView;
        AspectRatio = aspectRatio;
        NearPlane = nearPlane;
        FarPlane = farPlane;

        ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
            FieldOfView,
            AspectRatio,
            NearPlane,
            FarPlane);
    }

    public void SetOrthographic(in float width, in float aspectRatio, in float nearPlane, in float farPlane)
    {
        Width = width;
        Height = width / aspectRatio;
        NearPlane = nearPlane;
        FarPlane = farPlane;

        ProjectionMatrix = Matrix4x4.CreateOrthographic(
            width,
            Height,
            NearPlane,
            FarPlane);
    }

    //public void UpdateAspectRatio(float aspectRatio)
    //{
    //    SetProjParams(FieldOfView, aspectRatio, NearPlane, FarPlane);
    //}

    //public void SetLookDirection(in Vector3 lookDirection)
    //{
    //    Vector3 lookAt = Vector3.Add(Position, lookDirection);
    //    SetViewParams(Position, lookAt, Up);
    //}

    public void SetPosition(in Vector3 position)
    {
        //SetViewParams(position, LookAt, Up);
        SetIsometric(position);
    }
}
