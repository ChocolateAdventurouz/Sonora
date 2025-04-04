namespace Aura.EventArguments;

public class NameChangedEventArgs : EventArgs
{
    /// <summary>
    /// The previous name.
    /// </summary>
    public string OldName { get; }

    /// <summary>
    /// The new name.
    /// </summary>
    public string NewName { get; }

    public NameChangedEventArgs(string oldName, string newName)
    {
        OldName = oldName;
        NewName = newName;
    }
}
