namespace MyOtherGame;

public enum GameState
{
    Starting,
    Running,
    Stopping,
    Stopped
}

internal class GameMain
{
    private readonly DeviceResources _deviceResources;
    private readonly GameRenderer _renderer;
    private readonly Simple3DGame _game;
    private GameState _gameState;

    public GameMain(DeviceResources deviceResources, GameRenderer renderer)
    {
        _deviceResources = deviceResources;
        _renderer = renderer;
        _game = new Simple3DGame();

        _game.Initialize();
        _renderer.CreateGameDeviceResources(_game);
        _renderer.FinalizeCreateGameDeviceResources();

        _gameState = GameState.Starting;
    }

    public void Run()
    {
        while (_gameState != GameState.Stopped)
        {
            // necessary as to not freeze the window
            GLFW.Glfw.PollEvents();

            //var escape = Glfw.GetKey(Window, Keys.Escape);
            //if (escape == InputState.Press)
            //    running = false;

            //Glfw.GetCursorPosition(Window, out double mouseX, out double mouseY);

            Update();
            _renderer.Render();
            _deviceResources.Present();
        }
    }

    public void Stop()
    {
        _gameState = GameState.Stopped;
    }

    private void Update()
    {

    }
}
