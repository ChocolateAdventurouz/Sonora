namespace Aura.EventArguments;

public class StateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Previous state.
    /// </summary>
    public bool OldState { get; }

    /// <summary>
    /// New state.
    /// </summary>
    public bool NewState { get; }

    public StateChangedEventArgs(bool oldState, bool newState)
    {
        OldState = oldState;
        NewState = newState;
    }
}
