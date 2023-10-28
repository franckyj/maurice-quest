namespace MyOtherOtherGame;

internal sealed class Timer
{
    private long _startTime;
    private long _totalIdleTime;
    private long _pausedTime;
    private long _currentTime;
    private long _previousTime;
    private long _deltaTime;

    private bool _isStopped;

    /// <summary>
    /// Returns the total time since the game started: `(now - start) - totalIdle`.
    /// </summary>
    /// <returns>Total time since the game started</returns>
    public TimeSpan GetTotalTime()
    {
        if (_isStopped) return new TimeSpan(_pausedTime - _startTime - _totalIdleTime);

        return new TimeSpan(_currentTime - _startTime - _totalIdleTime);
    }

    /// <summary>
    /// Returns the time elapsed between two frames.
    /// Delta time is updated during the game loop.
    /// </summary>
    /// <returns>The time elapsed between two frames</returns>
    public TimeSpan GetDeltaTime()
    {
        return new TimeSpan(_deltaTime);
    }

    /// <summary>
    /// Resets the timer.
    /// </summary>
    public void Reset()
    {
        var now = DateTime.Now.Ticks;

        _startTime = now;
        _previousTime = now;
        _pausedTime = 0;
        _isStopped = false;
    }

    /// <summary>
    /// Starts the timer (if it is not already running).
    /// </summary>
    public void Start()
    {
        if (!_isStopped) return;

        var now = DateTime.Now.Ticks;
        _totalIdleTime = now - _pausedTime;

        _previousTime = now;
        _pausedTime = 0;
        _isStopped = false;
    }

    /// <summary>
    /// Ticks the timer, i.e. it computes the time that has elapsed between two frames.
    /// </summary>
    public void Tick()
    {
        if (_isStopped)
        {
            _deltaTime = 0;
        }
        else
        {
            var now = DateTime.Now.Ticks;

            _currentTime = now;
            _deltaTime = _currentTime - _previousTime;
            _previousTime = _currentTime;

            if (_deltaTime < 0) _deltaTime = 0;
        }
    }

    /// <summary>
    /// Stops the timer (if it is currently running).
    /// </summary>
    public void Stop()
    {
        if (_isStopped) return;

        var now = DateTime.Now.Ticks;

        _pausedTime = now;
        _isStopped = true;
    }
}
