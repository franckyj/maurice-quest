using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using Vortice.Direct3D11;

namespace MyGame.Drawable;

internal static class Shapes
{
    #region cube

    public class Cube<T> where T : unmanaged
    {
        private static Cube<T> _instance;

        public static Cube<T> GetInstance(ID3D11Device device, Func<Vector3, int, T> createFromPosition)
        {
            _instance ??= new Cube<T>(device, createFromPosition);
            return _instance;
        }

        public ID3D11Buffer VertexBuffer { get; private set; }
        public int VertexBufferStride { get; } = Unsafe.SizeOf<T>();
        public ID3D11Buffer IndexBuffer { get; private set; }
        public int IndexCount { get; private set; }

        protected Cube(ID3D11Device device, Func<Vector3, int, T> createFromPosition)
        {
            Span<T> vertices = stackalloc T[]
            {
                createFromPosition(new Vector3(-1.0f, -1.0f, 1.0f), 0),
                createFromPosition(new Vector3(1.0f, -1.0f, 1.0f), 1),
                createFromPosition(new Vector3(-1.0f, 1.0f, 1.0f), 2),
                createFromPosition(new Vector3(1.0f, 1.0f, 1.0f), 3),
                createFromPosition(new Vector3(-1.0f, -1.0f, -1.0f), 4),
                createFromPosition(new Vector3(1.0f, -1.0f, -1.0f), 5),
                createFromPosition(new Vector3(-1.0f, 1.0f, -1.0f), 6),
                createFromPosition(new Vector3(1.0f, 1.0f, -1.0f), 7)
            };

            var data = CreateBox(new Vector3(2), createFromPosition);
            //VertexBuffer = device.CreateBuffer(vertices, BindFlags.VertexBuffer);
            VertexBuffer = device.CreateBuffer(data.Item1, BindFlags.VertexBuffer);

            Span<ushort> indices = stackalloc ushort[]
            {
                0, 2, 1, 2, 3, 1,
                1, 3, 5, 3, 7, 5,
                2, 6, 3, 3, 6, 7,
                4, 5, 7, 4, 7, 6,
                0, 4, 2, 2, 4, 6,
                0, 1, 4, 1, 5, 4
            };
            //IndexBuffer = device.CreateBuffer(indices, BindFlags.IndexBuffer);
            IndexBuffer = device.CreateBuffer(data.Item2, BindFlags.IndexBuffer);
            //IndexCount = indices.Length;
            IndexCount = data.Item2.Length;
        }

        public static (T[], ushort[]) CreateBox(in Vector3 size, Func<Vector3, int, T> createFromPosition)
        {
            const int CubeFaceCount = 6;

            //List<VertexPositionNormalTexture> vertices = new();
            List<T> vertices = new();
            List<ushort> indices = new();

            Vector3[] faceNormals = new Vector3[]
            {
                Vector3.UnitZ,
                new Vector3(0.0f, 0.0f, -1.0f),
                Vector3.UnitX,
                new Vector3(-1.0f, 0.0f, 0.0f),
                Vector3.UnitY,
                new Vector3(0.0f, -1.0f, 0.0f)
            };

            //Vector3[] faceColors = new Vector3[CubeFaceCount]
            //{
            //    new Vector3(1.0f, 0.0f, 0.0f),
            //    new Vector3(0.0f, 1.0f, 0.0f),
            //    new Vector3(0.0f, 0.0f, 1.0f),
            //    new Vector3(1.0f, 1.0f, 1.0f),
            //    new Vector3(1.0f, 1.0f, 1.0f),
            //    new Vector3(1.0f, 1.0f, 1.0f)
            //};

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
                vertices.Add(createFromPosition(Vector3.Multiply(Vector3.Subtract(Vector3.Subtract(normal, side1), side2), tsize), 0));

                // (normal - side1 + side2) * tsize // normal // t1
                vertices.Add(createFromPosition(Vector3.Multiply(Vector3.Add(Vector3.Subtract(normal, side1), side2), tsize), 0));

                // (normal + side1 + side2) * tsize // normal // t2
                vertices.Add(createFromPosition(Vector3.Multiply(Vector3.Add(normal, Vector3.Add(side1, side2)), tsize), 0));

                // (normal + side1 - side2) * tsize // normal // t3
                vertices.Add(createFromPosition(Vector3.Multiply(Vector3.Subtract(Vector3.Add(normal, side1), side2), tsize), 0));

                vbase += 4;
            }

            return (vertices.ToArray(), indices.ToArray());
        }
    }

    #endregion

    #region pyramid

    public unsafe class Pyramid<T> where T : unmanaged
    {
        private static Pyramid<T> _instance;

        public static Pyramid<T> GetInstance(ID3D11Device device, Func<Vector3, int, T> createFromPosition)
        {
            _instance ??= new Pyramid<T>(device, createFromPosition);
            return _instance;
        }

        public ID3D11Buffer VertexBuffer { get; private set; }
        public int VertexBufferStride => sizeof(T);
        public ID3D11Buffer IndexBuffer { get; private set; }
        public int IndexCount { get; private set; }

        protected Pyramid(ID3D11Device device, Func<Vector3, int, T> createFromPosition)
        {
            Span<T> vertices = stackalloc T[]
            {
                createFromPosition(new Vector3(-1.0f, -1.0f, 1.0f), 0),
                createFromPosition(new Vector3(1.0f, -1.0f, 1.0f), 1),
                createFromPosition(new Vector3(-1.0f, 1.0f, 1.0f), 2),
                createFromPosition(new Vector3(1.0f, 1.0f, 1.0f), 3),
                createFromPosition(new Vector3(0.0f, 0.0f, -1.0f), 4)
            };
            VertexBuffer = device.CreateBuffer(vertices, BindFlags.VertexBuffer);

            Span<ushort> indices = stackalloc ushort[]
            {
                0, 2, 1, 2, 3, 1,
                0, 5, 2, 2, 5, 3,
                3, 5, 1, 1, 5, 0
            };
            IndexBuffer = device.CreateBuffer(indices, BindFlags.IndexBuffer);
            IndexCount = indices.Length;
        }
    }

    #endregion
}
