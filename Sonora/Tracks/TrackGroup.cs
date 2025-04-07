using Sonora.Clips;
using Sonora.EventArguments;
using Sonora.Plugins;
using Sonora.SampleProviders;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Sonora.Tracks;

/// <summary>
/// Represent a group of tracks.
/// </summary>
public class TrackGroup : Track
{
    /// <inheritdoc/>
    public override event EventHandler<StreamVolumeEventArgs> VolumeMeasured;

    /// <summary>
    /// Event called when a track has been added to this track group.
    /// </summary>
    public event EventHandler<TrackAddedOrRemovedEventArgs>? TrackAdded;

    /// <summary>
    /// Event called when a track has been removed from this track group.
    /// </summary>
    public event EventHandler<TrackAddedOrRemovedEventArgs>? TrackRemoved;

    private readonly List<Track> _tracks = new();

    /// <summary>
    /// The tracks of this group.
    /// </summary>
    public IReadOnlyList<Track> Tracks => _tracks;

    /// <summary>
    /// Create a new track group.
    /// </summary>
    public TrackGroup(string name = "")
    {
        Name = name;

        TrackAdded += (sender, e) => {
            if (e.Track.IsInGroup)
            {
                e.Track.TrackGroup.RemoveTrackFromGroup(e.Track, false);
            }
            e.Track.TrackGroup = this;
        };

        TrackRemoved += (sender, e) => {
            e.Track.TrackGroup = null;
        };

        Mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(SonoraMain.SampleRate, 2))
        {
            ReadFully = true
        };

        // Initialize Vst's chain to apply vst's effects (returns audio processed by all track vst's)
        PluginChainSampleProvider = new PluginChainSampleProvider(Mixer);

        // Initialize custom StereoSampleProvider for volume and pan control
        StereoSampleProvider = new StereoSampleProvider(PluginChainSampleProvider);

        // Initialize MeteringSampleProvider for gain feedback
        MeteringSampleProvider = new MeteringSampleProvider(StereoSampleProvider, 100);
        MeteringSampleProvider.StreamVolume += (s, e) => VolumeMeasured?.Invoke(this, e);

        // Store left and right audio channels gain data
        VolumeMeasured += (sender, e) =>
        {
            LeftChannelGain = e.MaxSampleValues[0];
            RightChannelGain = e.MaxSampleValues[1];
        };

        // Fills the buffer with zeroes if track is disabled
        TrackStateSampleProvider = new TrackStateSampleProvider(MeteringSampleProvider);

        // Add this track to the master mixer
        Master.AddTrack(this);
    }

    /// <summary>
    /// Add a track to this group.
    /// </summary>
    /// <param name="track">The track to add.</param>
    /// <exception cref="ArgumentException"></exception>
    public void AddTrackToGroup(Track track)
    {
        if (track.IsTrackGroup)
        {
            throw new ArgumentException($"Cannot nest track groups.", nameof(track));
        }

        if (Tracks.Contains(track))
        {
            throw new ArgumentException($"Track is already present in this track group.", nameof(track));
        }

        Master.RemoveTrack(track); // remove track from master mixer since it will point to the group instead
        Mixer.AddMixerInput(track.GetTrackAudio()); // make track point to group mixer
        _tracks.Add(track);

        TrackAdded?.Invoke(this, new TrackAddedOrRemovedEventArgs(track));
    }

    /// <summary>
    /// Remove a track from this group.
    /// </summary>
    /// <param name="track">The track to remove.</param>
    /// <param name="pointToMaster">Make the removed track point to the master mixer. (should be true for most cases)</param>
    /// <exception cref="ArgumentException"></exception>
    public void RemoveTrackFromGroup(Track track, bool pointToMaster = true)
    {
        if (track.IsTrackGroup)
        {
            throw new ArgumentException($"Cannot nest track groups.", nameof(track));
        }

        if (!Tracks.Contains(track))
        {
            throw new ArgumentException($"Track isn't present in this track group.", nameof(track));
        }
        else
        {
            Mixer.RemoveMixerInput(track.GetTrackAudio()); // make the track not point the group mixer anymore
            if (pointToMaster)
                Master.AddTrack(track); // make the track point the master mixer
            _tracks.Remove(track);

            TrackRemoved?.Invoke(this, new TrackAddedOrRemovedEventArgs(track));
        }
    }

    internal override void FireClip(Clip clip)
    {
        throw new InvalidOperationException("Track group cannot play sounds.");
    }

    public override void StartRecording(string destPath)
    {
        throw new InvalidOperationException("Track group cannot record.");
    }

    public override Clip? StopRecording()
    {
        throw new InvalidOperationException("Track group cannot record.");
    }

    /// <inheritdoc/>
    public override void StopSounds()
    {
        foreach (var track in Tracks)
        {
            track.StopSounds();
        }
    }
}
