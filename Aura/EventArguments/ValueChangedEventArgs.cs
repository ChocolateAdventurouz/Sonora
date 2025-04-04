namespace Aura.EventArguments;

public class ValueChangedEventArgs : EventArgs
{
    /// <summary>
    /// The previous value.
    /// </summary>
    public float OldValue { get; }

    /// <summary>
    /// The new value.
    /// </summary>
    public float NewValue { get; }

    public ValueChangedEventArgs(float oldValue, float newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}
