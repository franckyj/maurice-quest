﻿using System.Numerics;
using Vortice.Direct3D11;
using static MyOtherGame.ConstantBuffers;
using static MyOtherGame.Meshes;

namespace MyOtherGame;

internal static class GameObjects
{
    public enum TargetId
    {
        Unknown = -1,
        WorldFloor = 80001,
        WorldCeiling = 80002,
        WorldWalls = 80003
    }

    public class GameObject
    {
        public GameObject(
            Vector3 position,
            Vector3 velocity,
            Matrix4x4 modelMatrix,
            TargetId targetId = TargetId.Unknown)
        {
            Position = position;
            Velocity = velocity;
            ModelMatrix = modelMatrix;
            TargetId = targetId;
        }

        public GameObject()
            : this(Vector3.Zero, Vector3.Zero, Matrix4x4.Identity, TargetId.Unknown)
        { }

        public MeshObject? Mesh { get; set; }
        public Material? Material { get; set; }


        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public Matrix4x4 ModelMatrix { get; set; }

        public TargetId TargetId { get; protected set; }

        public void UpdatePosition()
        {

        }

        public virtual void Render(in ID3D11DeviceContext context, in ID3D11Buffer primitiveConstantBuffer)
        {
            if (Mesh == null || Material == null) return;

            //var constantBuffer = new ConstantBufferChangesEveryPrim(ModelMatrix);
            var constantBuffer = new ConstantBufferChangesEveryPrim
            {
                WorldMatrix = ModelMatrix
            };

            // TODO use ref?
            Material.SetupRender(context, constantBuffer);
            context.UpdateSubresource(constantBuffer, primitiveConstantBuffer);
            Mesh.Render(context);
        }
    }

    public class SphereObject : GameObject
    {
        private float _radius;
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

        public float Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                Update();
            }
        }

        public SphereObject(TargetId targetId = TargetId.Unknown)
            : this(Vector3.Zero, 1.0f, targetId)
        { }

        public SphereObject(Vector3 position, float radius, TargetId targetId = TargetId.Unknown)
        {
            Position = position;
            Radius = radius;
            TargetId = targetId;

            var random = new Random(1);
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
        }

        public void Update()
        {
            ModelMatrix =
                //Matrix4x4.CreateScale(_radius, _radius, _radius) *
                Matrix4x4.CreateTranslation(Position);
            //ModelMatrix = Matrix4x4.CreateTranslation(Position);
        }

        public override void Render(in ID3D11DeviceContext context, in ID3D11Buffer primitiveConstantBuffer)
        {
            const float deltaTime = 1.0f / 60.0f;

            _roll += deltaTime * _droll;
            _pitch += deltaTime * _dpitch;
            _yaw += deltaTime * _dyaw;
            _theta += deltaTime * _dtheta;
            _phi += deltaTime * _dphi;
            _chi += deltaTime * _dchi;

            //ModelMatrix =
                //Matrix4x4.CreateFromYawPitchRoll(_yaw, _pitch, _roll) *
                //Matrix4x4.CreateTranslation(_radius, 0.0f, 0.0f) *
                //Matrix4x4.CreateFromYawPitchRoll(_theta, _phi, _chi) *
                //Matrix4x4.CreateTranslation(Position);

            base.Render(context, primitiveConstantBuffer);
        }
    }

    public class CylinderObject : GameObject
    {
        private Vector3 _axis;
        private float _length;
        private float _radius;
        private Matrix4x4 _rotationMatrix;

        public Vector3 Axis { get => _axis; }
        public float Length { get => _length; }
        public float Radius { get => _radius; }
        public Matrix4x4 RotationMatrix { get => _rotationMatrix; }

        public CylinderObject(TargetId targetId = TargetId.Unknown)
            : this(Vector3.Zero, 1.0f, Vector3.Zero, targetId)
        { }

        public CylinderObject(
            Vector3 position,
            float radius,
            Vector3 direction,
            TargetId targetId = TargetId.Unknown)
        {
            Position = position;
            _radius = radius;
            TargetId = targetId;

            // store the length and axis of the vector
            _length = direction.Length();
            _axis = Vector3.Normalize(direction);

            var dot = Vector3.Dot(Vector3.UnitZ, _axis);
            dot = Math.Clamp(dot, -1.0f, 1.0f);
            var angle = MathF.Acos(dot);

            var mat = Matrix4x4.Identity;
            if (angle * angle > 0.025)
            {
                var axis = Vector3.Cross(Vector3.UnitZ, _axis);
                mat = Matrix4x4.CreateFromAxisAngle(axis, angle);
            }
            _rotationMatrix = mat;
            // S * R * T
            ModelMatrix =
                Matrix4x4.CreateTranslation(Position);
            //*
            //mat *
            //Matrix4x4.CreateScale(_radius, _radius, _length);

            //ModelMatrix = Matrix4x4
            //    .CreateScale(_radius, _radius, _length) *
            //    mat *
            //    Matrix4x4.CreateTranslation(Position);
        }
    }
}