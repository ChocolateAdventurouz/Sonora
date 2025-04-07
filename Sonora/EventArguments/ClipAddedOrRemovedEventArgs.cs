using Sonora.Clips;

namespace Sonora.EventArguments;

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
