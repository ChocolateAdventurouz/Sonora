using Aura.Tracks;

namespace Aura.EventArguments;

public class TrackAddedOrRemovedEventArgs : EventArgs
{
    /// <summary>
    /// The track that is added or removed.
    /// </summary>
    public Track Track { get; }

    public TrackAddedOrRemovedEventArgs(Track track)
    {
        Track = track;
    }
}
