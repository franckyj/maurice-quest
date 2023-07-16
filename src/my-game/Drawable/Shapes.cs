using System.Numerics;
using Vortice.Direct3D11;

namespace MyGame.Drawable;

internal static class Shapes
{
    #region cube

    public unsafe class Cube<T> where T : unmanaged
    {
        private static Cube<T> _instance;

        public static Cube<T> GetInstance(ID3D11Device device, Func<Vector3, int, T> createFromPosition)
        {
            _instance ??= new Cube<T>(device, createFromPosition);
            return _instance;
        }

        public ID3D11Buffer VertexBuffer { get; private set; }
        public int VertexBufferStride => sizeof(T);
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
            VertexBuffer = device.CreateBuffer(vertices, BindFlags.VertexBuffer);

            Span<ushort> indices = stackalloc ushort[]
            {
                0, 2, 1, 2, 3, 1,
                1, 3, 5, 3, 7, 5,
                2, 6, 3, 3, 6, 7,
                4, 5, 7, 4, 7, 6,
                0, 4, 2, 2, 4, 6,
                0, 1, 4, 1, 5, 4
            };
            IndexBuffer = device.CreateBuffer(indices, BindFlags.IndexBuffer);
            IndexCount = indices.Length;
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
