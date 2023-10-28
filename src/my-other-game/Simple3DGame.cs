using System.Numerics;
using static MyOtherGame.GameObjects;

namespace MyOtherGame;

internal class Simple3DGame
{
    private Camera _camera;
    private GameObject[] _renderObjects;

    public Camera Camera => _camera;
    public IEnumerable<GameObject> RenderObjects => _renderObjects;

    public Simple3DGame()
    { }

    public void Initialize()
    {
        _camera = new Camera();
        //_camera.SetProjParams(MathF.PI / 4.0f, 1.0f, 1.0f, 100.0f);
        _camera.SetViewParams(
            new Vector3(0.0f, 0f, 45.0f),
            //m_player->Position(),             // Eye point in world coordinates.
            new Vector3(0.0f, 0.0f, 0.0f),      // Look at point in world coordinates.
            Vector3.UnitY                       // The Up vector for the camera.
        );

        //_camera.SetViewParams(
        //    new Vector3(0.0f, 1.3f, 4.0f),
        //    //m_player->Position(),             // Eye point in world coordinates.
        //    new Vector3(0.0f, -0.7f, 0.0f),     // Look at point in world coordinates.
        //    new Vector3(0.0f, -1.0f, 0.0f)      // The Up vector for the camera.
        //);

        _renderObjects = new GameObject[1];
        _renderObjects[0] = new SphereObject(
            new Vector3(30.0f, 0.0f, 0.0f),
            1.0f);
        //_renderObjects[1] = new CylinderObject(
        //    new Vector3(-4.0f, 0.0f, 0.0f),
        //    1.0f,
        //    Vector3.UnitY);
    }

    public void Update(float dt)
    {
        //var p = _renderObjects[0].Position;
        //p.X += dt * 5;
        //_renderObjects[0].Position = p;
        //((SphereObject)_renderObjects[0]).Update();
    }
}
