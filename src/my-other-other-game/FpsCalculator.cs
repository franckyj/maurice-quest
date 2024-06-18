namespace MyOtherOtherGame;

public readonly record struct Fps(long FPS, TimeSpan TimePerFrame);

internal class FpsCalculator
{
    private long _framesSeen = 0;
    private double _elapsedSeconds = 0;

    private Fps _currentFps;

    private readonly ITimeline _timer;

    public FpsCalculator(ITimeline timer)
    {
        _timer = timer ?? throw new ArgumentNullException(nameof(timer));
        _currentFps = new Fps(0, TimeSpan.Zero);
    }

    public Fps CalculateFrameStatistics()
    {
        _framesSeen++;
        if (_timer.GetTotalTime().TotalSeconds - _elapsedSeconds > 1.0d)
        {
            long fps = _framesSeen;
            double msPerFrame = 1000.0 / fps;

            _currentFps = new Fps(fps, TimeSpan.FromMilliseconds(msPerFrame));

            _framesSeen = 0;
            _elapsedSeconds++;
        }
        return _currentFps;
    }
}
