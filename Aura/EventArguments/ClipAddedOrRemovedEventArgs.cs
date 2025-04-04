using Aura.Clips;

namespace Aura.EventArguments;

public class ClipAddedOrRemovedEventArgs : EventArgs
{
    /// <summary>
    /// The clip added or removed from the track.
    /// </summary>
    public Clip Clip { get; set; }

    public ClipAddedOrRemovedEventArgs(Clip clip)
    {
        Clip = clip;
    }
}
