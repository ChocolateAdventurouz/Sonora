namespace Aura.EventArguments;

public class TimeChangedEventArgs : EventArgs
{
    /// <summary>
    /// Previous time in seconds.
    /// </summary>
    public double OldTime { get; }

    /// <summary>
    /// New time in seconds.
    /// </summary>
    public double NewTime { get; }

    public TimeChangedEventArgs(double oldTime, double newTime)
    {
        OldTime = oldTime;
        NewTime = newTime;
    }
}
