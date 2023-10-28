using GLFW;

namespace MyOtherGame;

public enum GameState
{
    Starting,
    Running,
    Paused,
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

    public void Run(NativeWindow? window)
    {
        if (window == null) return;

        _gameState = GameState.Running;
        while (_gameState != GameState.Stopped)
        {
            // necessary as to not freeze the window
            GLFW.Glfw.PollEvents();

            var space = Glfw.GetKey(window, Keys.Space);
            //Console.WriteLine(space.ToString());
            if (space == InputState.Press)
                Pause();
            //TogglePause();

            //Glfw.GetCursorPosition(Window, out double mouseX, out double mouseY);

            if (_gameState == GameState.Paused) continue;

            Update();
            _renderer.Render();
            _deviceResources.Present();
        }
    }

    public void TogglePause()
    {
        Console.WriteLine("toggle pause");

        if (_gameState != GameState.Paused) Pause();
        else Unpause();
    }

    public void Pause()
    {
        _gameState = GameState.Paused;
        Console.WriteLine("paused");
    }

    public void Unpause()
    {
        _gameState = GameState.Running;
        Console.WriteLine("unpaused");
    }

    public void Stop()
    {
        _gameState = GameState.Stopped;
    }

    private void Update()
    {
        var dt = 1.0f / 60.0f;
        _game.Update(dt);
    }
}
