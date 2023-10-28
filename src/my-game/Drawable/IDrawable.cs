using System.Numerics;
using Vortice.Direct3D11;

namespace MyGame.Drawable;

internal interface IDrawable
{
    void Update(
        float deltaTime,
        Matrix4x4 viewMatrix,
        Matrix4x4 projectionMatrix,
        ID3D11DeviceContext context,
        float mouseX,
        float mouseY,
        int width,
        int height
    );
    void Draw(ID3D11DeviceContext context);
}
