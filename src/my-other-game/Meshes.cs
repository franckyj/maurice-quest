using System.Numerics;
using System.Runtime.CompilerServices;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using static MyOtherGame.Vertices;

namespace MyOtherGame;

internal static class Meshes
{
    public abstract class MeshObject
    {
        protected ID3D11Buffer? VertexBuffer { get; init; } = null;
        protected ID3D11Buffer? IndexBuffer { get; init; } = null;
        protected int VertexCount { get; init; } = 0;
        protected int IndexCount { get; init; } = 0;

        public virtual void Render(in ID3D11DeviceContext context)
        {
            if (VertexBuffer == null || IndexBuffer == null) return;

            int stride = Unsafe.SizeOf<PNTVertex>();
            int offset = 0;

            context.IASetVertexBuffer(0, VertexBuffer, stride, offset);

            context.IASetIndexBuffer(IndexBuffer, Vortice.DXGI.Format.R16_UInt, 0);
            context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
            context.DrawIndexed(IndexCount, 0, 0);
        }
    }

    public class SphereMesh : MeshObject
    {
        public SphereMesh(in ID3D11Device device, int segments)
        {
            int slices = segments / 2;
            int numVertices = (slices + 1) * (segments + 1) + 1;
            int numIndices = slices * segments * 3 * 2;

            Span<PNTVertex> point = stackalloc PNTVertex[numVertices];
            Span<ushort> index = stackalloc ushort[numIndices];

            // To make the texture look right on the top and bottom of the sphere
            // each slice will have 'segments + 1' vertices.  The top and bottom
            // vertices will all be coincident, but have different U texture cooordinates.
            int p = 0;
            float segmentsFloat = segments;
            for (int a = 0; a <= slices; a++)
            {
                float angle1 = a / (float)slices * MathF.PI;
                float z = MathF.Cos(angle1);
                float r = MathF.Sin(angle1);
                for (int b = 0; b <= segments; b++)
                {
                    float angle2 = b / segmentsFloat * MathF.Tau;
                    var position = new Vector3(r * MathF.Cos(angle2), r * MathF.Sin(angle2), z);
                    point[p] = new PNTVertex(
                        position,
                        position,
                        new Vector2((1.0f - z) / 2.0f, b / segmentsFloat));
                    p++;
                }
            }
            VertexCount = p;

            p = 0;
            for (int a = 0; a < slices; a++)
            {
                int p1 = a * (segments + 1);
                int p2 = (a + 1) * (segments + 1);

                // Generate two triangles for each segment around the slice.
                for (int b = 0; b < segments; b++)
                {
                    if (a < (slices - 1))
                    {
                        // For all but the bottom slice add the triangle with one
                        // vertex in the a slice and two vertices in the a + 1 slice.
                        // Skip it for the bottom slice since the triangle would be
                        // degenerate as all the vertices in the bottom slice are coincident.
                        index[p] = (ushort)(b + p1);
                        index[p + 1] = (ushort)(b + p2);
                        index[p + 2] = (ushort)(b + p2 + 1);
                        p = p + 3;
                    }
                    if (a > 0)
                    {
                        // For all but the top slice add the triangle with two
                        // vertices in the a slice and one vertex in the a + 1 slice.
                        // Skip it for the top slice since the triangle would be
                        // degenerate as all the vertices in the top slice are coincident.
                        index[p] = (ushort)(b + p1);
                        index[p + 1] = (ushort)(b + p2 + 1);
                        index[p + 2] = (ushort)(b + p1 + 1);
                        p = p + 3;
                    }
                }
            }
            IndexCount = p;

            VertexBuffer = device.CreateBuffer(point, BindFlags.VertexBuffer);
            IndexBuffer = device.CreateBuffer(index, BindFlags.IndexBuffer);
        }
    }

    public class CylinderMesh : MeshObject
    {
        public CylinderMesh(in ID3D11Device device, int segments)
        {
            int numVertices = 6 * (segments + 1) + 1;
            int numIndices = 3 * segments * 3 * 2;

            Span<PNTVertex> point = stackalloc PNTVertex[numVertices];
            Span<ushort> index = stackalloc ushort[numIndices];

            int p = 0;
            float segmentsFloat = segments;

            // Top center point (multiple points for texture coordinates).
            for (int a = 0; a <= segments; a++)
            {
                var position = new Vector3(0.0f, 0.0f, 1.0f);
                point[p] = new PNTVertex(
                    position,
                    position,
                    new Vector2(a / segmentsFloat, 0.0f));
                p++;
            }

            // Top edge of cylinder: Normals point up for lighting of top surface.
            for (int a = 0; a <= segments; a++)
            {
                float angle2 = a / segmentsFloat * MathF.Tau;
                var position = new Vector3(MathF.Cos(angle2), MathF.Sin(angle2), 1.0f);
                point[p] = new PNTVertex(
                    position,
                    new Vector3(0.0f, 0.0f, 1.0f),
                    new Vector2(a / segmentsFloat, 0.0f));
                p++;
            }

            // Top edge of cylinder: Normals point out for lighting of the side surface.
            for (int a = 0; a <= segments; a++)
            {
                float angle2 = a / segmentsFloat * MathF.Tau;
                var position = new Vector3(MathF.Cos(angle2), MathF.Sin(angle2), 1.0f);
                point[p] = new PNTVertex(
                    position,
                    position with { Z = 0.0f },
                    new Vector2(a / segmentsFloat, 0.0f));
                p++;
            }

            // Bottom edge of cylinder: Normals point out for lighting of the side surface.
            for (int a = 0; a <= segments; a++)
            {
                float angle2 = a / segmentsFloat * MathF.Tau;
                var position = new Vector3(MathF.Cos(angle2), MathF.Sin(angle2), 0.0f);
                point[p] = new PNTVertex(
                    position,
                    position,
                    new Vector2(a / segmentsFloat, 1.0f));
                p++;
            }

            // Bottom edge of cylinder: Normals point down for lighting of the bottom surface.
            for (int a = 0; a <= segments; a++)
            {
                float angle2 = a / segmentsFloat * MathF.Tau;
                var position = new Vector3(MathF.Cos(angle2), MathF.Sin(angle2), 0.0f);
                point[p] = new PNTVertex(
                    position,
                    new Vector3(0.0f, 0.0f, -1.0f),
                    new Vector2(a / segmentsFloat, 1.0f));
                p++;
            }
            // Bottom center of cylinder: Normals point down for lighting on the bottom surface.
            for (int a = 0; a <= segments; a++)
            {
                float angle2 = a / segmentsFloat * MathF.Tau;
                point[p] = new PNTVertex(
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(0.0f, 0.0f, -1.0f),
                    new Vector2(a / segmentsFloat, 1.0f));
                p++;
            }
            VertexCount = p;

            p = 0;
            for (short a = 0; a < 6; a += 2)
            {
                int p1 = a * (segments + 1);
                int p2 = (a + 1) * (segments + 1);
                for (short b = 0; b < segments; b++)
                {
                    if (a < 4)
                    {
                        index[p] = (ushort)(b + p1);
                        index[p + 1] = (ushort)(b + p2);
                        index[p + 2] = (ushort)(b + p2 + 1);
                        p = p + 3;
                    }
                    if (a > 0)
                    {
                        index[p] = (ushort)(b + p1);
                        index[p + 1] = (ushort)(b + p2 + 1);
                        index[p + 2] = (ushort)(b + p1 + 1);
                        p = p + 3;
                    }
                }
            }
            IndexCount = p;

            VertexBuffer = device.CreateBuffer(point, BindFlags.VertexBuffer);
            IndexBuffer = device.CreateBuffer(index, BindFlags.IndexBuffer);
        }
    }

    public class FaceMesh : MeshObject
    {
        public FaceMesh(in ID3D11Device device)
        {
            Span<PNTVertex> vertices = stackalloc PNTVertex[4]
            {
                new PNTVertex(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(1.0f, 1.0f)),
                new PNTVertex(new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0.0f, 1.0f)),
                new PNTVertex(new Vector3(1.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0.0f, 0.0f)),
                new PNTVertex(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(1.0f, 0.0f))
            };
            VertexCount = 4;
            VertexBuffer = device.CreateBuffer(vertices, BindFlags.VertexBuffer);

            Span<ushort> indices = stackalloc ushort[]
            {
                0, 1, 2,
                0, 2, 3,
                0, 2, 1,
                0, 3, 2
            };
            IndexCount = 12;
            IndexBuffer = device.CreateBuffer(indices, BindFlags.IndexBuffer);
        }
    }

    public class WorldFloorMesh : MeshObject
    {
        public WorldFloorMesh(in ID3D11Device device)
        {
            Span<PNTVertex> vertices = stackalloc PNTVertex[4]
            {
                new PNTVertex(new Vector3(-4.0f, -3.0f,  6.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 0.0f)),
                new PNTVertex(new Vector3( 4.0f, -3.0f,  6.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 0.0f)),
                new PNTVertex(new Vector3(-4.0f, -3.0f, -6.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 1.5f)),
                new PNTVertex(new Vector3( 4.0f, -3.0f, -6.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 1.5f))
            };
            VertexCount = 4;
            VertexBuffer = device.CreateBuffer(vertices, BindFlags.VertexBuffer);

            Span<ushort> indices = stackalloc ushort[]
            {
                0, 1, 2,
                1, 3, 2
            };
            IndexCount = 6;
            IndexBuffer = device.CreateBuffer(indices, BindFlags.IndexBuffer);
        }
    }

    public class CubeMesh : MeshObject
    {
        public CubeMesh(in ID3D11Device device)
        {
            Span<PNTVertex> vertices = stackalloc PNTVertex[]
            {
                new PNTVertex(new Vector3(-1.0f, -1.0f, 1.0f), Vector3.UnitY, Vector2.UnitX),
                new PNTVertex(new Vector3(1.0f, -1.0f, 1.0f), Vector3.UnitY, Vector2.UnitX),
                new PNTVertex(new Vector3(-1.0f, 1.0f, 1.0f), Vector3.UnitY, Vector2.UnitX),
                new PNTVertex(new Vector3(1.0f, 1.0f, 1.0f), Vector3.UnitY, Vector2.UnitX),
                new PNTVertex(new Vector3(-1.0f, -1.0f, -1.0f), Vector3.UnitY, Vector2.UnitX),
                new PNTVertex(new Vector3(1.0f, -1.0f, -1.0f), Vector3.UnitY, Vector2.UnitX),
                new PNTVertex(new Vector3(-1.0f, 1.0f, -1.0f), Vector3.UnitY, Vector2.UnitX),
                new PNTVertex(new Vector3(1.0f, 1.0f, -1.0f), Vector3.UnitY, Vector2.UnitX)
            };
            VertexCount = vertices.Length;
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
            IndexCount = indices.Length;
            IndexBuffer = device.CreateBuffer(indices, BindFlags.IndexBuffer);
        }
    }
}
