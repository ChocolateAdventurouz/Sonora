using System.Diagnostics;

namespace Aura.Utils;

/// <summary>
/// An adjustable stopwatch.
/// </summary>
public class AdjustableStopwatch : Stopwatch
{
    private Stopwatch _stopwatch = new Stopwatch();
    private TimeSpan _offset = TimeSpan.Zero;

    public TimeSpan Elapsed => _stopwatch.Elapsed + _offset;
    public bool IsRunning => _stopwatch.IsRunning;

    public void Start() => _stopwatch.Start();
    public void Stop() => _stopwatch.Stop();
    public void Reset()
    {
        _stopwatch.Reset();
        _offset = TimeSpan.Zero;
    }

    public void Restart()
    {
        _stopwatch.Restart();
        _offset = TimeSpan.Zero;
    }

    // Add time to the stopwatch
    public void AddTime(TimeSpan timeToAdd)
    {
        if (IsRunning)
        {
            _stopwatch.Stop();
            _offset += timeToAdd;
            _stopwatch.Start();
        }
        else
        {
            _offset += timeToAdd;
        }
    }

    // Set time explicitly
    public void SetTime(TimeSpan newTime)
    {
        if (IsRunning)
        {
            _stopwatch.Restart();
            _offset = newTime;
        }
        else
        {
            _stopwatch.Reset();
            _offset = newTime;
        }
    }
}
