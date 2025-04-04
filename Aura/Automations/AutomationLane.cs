namespace Aura.Automations;

/// <summary>
/// Represent an automation lane for a specific parameter.
/// </summary>
public sealed class AutomationLane
{
    /// <summary>
    /// The parameter of this automation lane.
    /// </summary>
    public AutomationParameter Parameter { get; set; }

    /// <summary>
    /// The points in this automation lane.
    /// </summary>
    public List<AutomationPoint> Points { get; } = new();

    internal float GetValueAtTime(double time)
    {
        if (Points.Count == 0) return 0;
        if (Points.Count == 1) return Points[0].Value;

        // Find surrounding points
        var prev = Points.LastOrDefault(p => p.Time <= time);
        var next = Points.FirstOrDefault(p => p.Time > time);

        if (prev == null) return next.Value;
        if (next == null) return prev.Value;

        // Calculate interpolation factor (0-1)
        double factor = (time - prev.Time) / (next.Time - prev.Time);

        return Interpolate(prev.Value, next.Value, factor, prev.Interpolation);
    }

    private float Interpolate(float a, float b, double factor, InterpolationType type)
    {
        switch (type)
        {
            case InterpolationType.Linear:
                return (float)(a + (b - a) * factor);
            case InterpolationType.Step:
                return a;
            case InterpolationType.Smooth:
                // Cubic easing
                factor = factor * factor * (3 - 2 * factor);
                return (float)(a + (b - a) * factor);
            default:
                return a;
        }
    }
}
