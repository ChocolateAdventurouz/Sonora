namespace Sonora.Automations;

/// <summary>
/// Represent a point in an <see cref="AutomationLane"/>.
/// </summary>
public sealed class AutomationPoint
{
    /// <summary>
    /// Time of the point in seconds.
    /// </summary>
    public double Time { get; set; }

    /// <summary>
    /// Value at point.
    /// </summary>
    public float Value { get; set; }

    /// <summary>
    /// The interpolation type to use.
    /// </summary>
    public InterpolationType Interpolation { get; set; } = InterpolationType.Linear;
}
