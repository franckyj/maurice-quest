using System.Numerics;

namespace MyOtherGame
{
    internal class Camera
    {
        public Matrix4x4 ViewMatrix { get; private set; }
        public Matrix4x4 ProjectionMatrix { get; private set; }

        public Matrix4x4 InverseView { get; private set; }

        public Vector3 Eye { get; private set; }
        public Vector3 LookAt { get; private set; }
        public static Vector3 Up { get; private set; }
        public float CameraYawAngle { get; private set; }
        public float CameraPitchAngle { get; private set; }

        public float FieldOfView { get; private set; }
        public float AspectRatio { get; private set; }
        public float NearPlane { get; private set; }
        public float FarPlane { get; private set; }

        public Camera()
        {
            // Setup the view matrix.
            SetViewParams(
                // default eye position
                new Vector3(0.0f, 0.0f, 0.0f),

                // default look at position
                new Vector3(0.0f, 0.0f, 1.0f),

                // default up vector
                Vector3.UnitY
                );

            // setup the projection matrix
            SetProjParams(MathF.PI / 4, 1.0f, 1.0f, 1000.0f);
        }

        public void SetViewParams(in Vector3 eye, in Vector3 lookAt, in Vector3 up)
        {
            Eye = eye;
            LookAt = lookAt;
            Up = up;

            // Calculate the view matrix.
            ViewMatrix = Matrix4x4.CreateLookAt(Eye, LookAt, Up);
            Matrix4x4.Invert(ViewMatrix, out var inverseView);
            InverseView = inverseView;

            // The axis basis vectors and camera position are stored inside the
            // position matrix in the 4 rows of the camera's world matrix.
            // To figure out the yaw/pitch of the camera, we just need the Z basis vector.

            //XMFLOAT3 zBasis;
            //XMStoreFloat3(&zBasis, inverseView.r[2]);

            //float len = sqrtf(zBasis.z * zBasis.z + zBasis.x * zBasis.x);
            //m_cameraPitchAngle = atan2f(zBasis.y, len);

            var x = inverseView.M31;
            var y = inverseView.M32;
            var z = inverseView.M33;
            CameraYawAngle = MathF.Atan2(x, z);

            float len = MathF.Sqrt(z * z + x * x);
            CameraPitchAngle = MathF.Atan2(y, len);
        }

        public void SetProjParams(in float fieldOfView, in float aspectRatio, in float nearPlane, in float farPlane)
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

        public void UpdateAspectRatio(float aspectRatio)
        {
            SetProjParams(FieldOfView, aspectRatio, NearPlane, FarPlane);
        }

        public void SetLookDirection(in Vector3 lookDirection)
        {
            Vector3 lookAt = Vector3.Add(Eye, lookDirection);
            SetViewParams(Eye, lookAt, Up);
        }

        public void SetEyePosition(in Vector3 eyePosition)
        {
            SetViewParams(eyePosition, LookAt, Up);
        }
    }
}
