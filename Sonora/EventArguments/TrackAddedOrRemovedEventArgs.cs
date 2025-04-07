using Sonora.Tracks;

namespace Sonora.EventArguments;

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
