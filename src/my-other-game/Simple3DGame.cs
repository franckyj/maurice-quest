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
        _renderObjects = new GameObject[1];
        _camera = new Camera();
        _camera.SetProjParams(MathF.PI / 2.0f, 1.0f, 0.01f, 1000.0f);
        _camera.SetViewParams(
            new Vector3(0.0f, 0f, 4.0f),
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

        _renderObjects[0] = new SphereObject();
    }
}
