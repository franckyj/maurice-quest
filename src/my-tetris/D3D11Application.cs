using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MyTetris;

internal abstract class D3D11Application : IDisposable
{
    public const int MaxSkipFrames = 10;

    public static TimeSpan DeltaTime = TimeSpan.FromMilliseconds(1000.0 / 240.0);

    private readonly ILogger<D3D11Application> _logger;
    private readonly ITimeline _timer;
    private readonly FpsCalculator _fpsCalculator;

    private bool _showFps;
    private bool _isPaused;
    private bool _disposed;

    protected readonly Window _window;
    protected readonly Input _input;
    protected readonly Direct3D _d3d;

    public D3D11Application(
        ILogger<D3D11Application> logger,
        string title)
    {
        _logger = logger ?? new NullLogger<D3D11Application>();
        _logger.LogDebug("About to initialize...");

        _timer = new RealTimeline();
        _window = new Window(this, title);
        _input = new Input(_window);
        _fpsCalculator = new FpsCalculator(_timer);

        // create d3d stuff
        _d3d = new Direct3D();
        _d3d.SetWindow(_window.WindowHandle, _window.ClientWindowWidth, _window.ClientWindowHeight);

        _input.SetOnShowFpsPressed(() => { _showFps = !_showFps; });

        _logger.LogDebug("Initialization completed!");
    }

    public void Run()
    {
        _timer.Reset();

        // stores the time accumulated by the rendered
        TimeSpan accumulatedTime = TimeSpan.Zero;

        // enter main event loop
        bool continueRunning = true;
        while (continueRunning)
        {
            _timer.Tick();

            if (!_isPaused)
            {
                CalculateFrameStatistics();

                // acquire input
                _input.UpdateInput();

                // accumulate the elapsed time since the last frame
                accumulatedTime += _timer.GetDeltaTime();

                // the number of completed loops while updating the game
                // now update the game logic with fixed dt as often as possible
                int loopCount = 0;
                while (accumulatedTime >= DeltaTime && loopCount < MaxSkipFrames)
                {
                    Update(DeltaTime);
                    accumulatedTime -= DeltaTime;
                    loopCount++;
                }

                Render(accumulatedTime.TotalMilliseconds / DeltaTime.TotalMilliseconds);
            }
        }
    }

    public abstract void Update(TimeSpan deltaTime);

    public abstract void Render(double farseer);

    public void Pause()
    {
        _isPaused = true;
        _timer.Stop();
    }

    public void Unpause()
    {
        _isPaused = false;
        _timer.Start();
    }

    public void Resize()
    {
    }

    public void Stop()
    {
    }

    public void CalculateFrameStatistics()
    {
        var fps = _fpsCalculator.CalculateFrameStatistics();

        if (_showFps)
        {
            Console.WriteLine(fps.FPS.ToString("d3"));
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _window.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
