using Sonora.SampleProviders;
using Sonora.Plugins;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Sonora.Clips;
using Sonora.EventArguments;
using Sonora.Utils;

namespace Sonora.Tracks;

/// <summary>
/// Represent an Audio, Midi or Group track.
/// </summary>
public abstract class Track
{
    private readonly List<Clip> _clips = new();

    /// <summary>
    /// The clips of this track.
    /// </summary>
    public IReadOnlyList<Clip> Clips => _clips;

    /// <summary>
    /// Event called whenever a clip is added to this track.
    /// </summary>
    public event EventHandler<ClipAddedOrRemovedEventArgs>? ClipAdded;

    /// <summary>
    /// Event called whenever a clip is removed from this track.
    /// </summary>
    public event EventHandler<ClipAddedOrRemovedEventArgs>? ClipRemoved;

    /// <summary>
    /// The plugin instrument of this track. (only used if it's a <see cref="MidiTrack"/>)
    /// </summary>
    public IPlugin PluginInstrument => PluginChainSampleProvider.PluginInstrument;

    /// <summary>
    /// The plugin effects of this track.
    /// </summary>
    public IReadOnlyList<IPlugin> PluginEffects => PluginChainSampleProvider.FxPlugins;

    /// <summary>
    /// Event called whenever a plugin is added to this track.
    /// </summary>
    public event EventHandler<PluginAddedOrRemovedEventArgs>? PluginAdded;

    /// <summary>
    /// Event called whenever a plugin is removed from this track.
    /// </summary>
    public event EventHandler<PluginAddedOrRemovedEventArgs>? PluginRemoved;

    /// <summary>
    /// Provides volume metering data.
    /// </summary>
    public abstract event EventHandler<StreamVolumeEventArgs> VolumeMeasured;

    #region SampleProviders

    /// <summary>
    /// Mixer of the track which allows to play multiple sounds at once.
    /// </summary>
    internal MixingSampleProvider Mixer { get; set; }

    /// <summary>
    /// SampleProvider which store and manage plugins.
    /// </summary>
    internal PluginChainSampleProvider PluginChainSampleProvider { get; set; }

    /// <summary>
    /// SamplePovider which allows to control the track Volume and Pan.
    /// </summary>
    internal StereoSampleProvider StereoSampleProvider { get; set; }

    /// <summary>
    /// SampleProvider which provides audio metering data.
    /// </summary>
    internal MeteringSampleProvider MeteringSampleProvider { get; set; }

    /// <summary>
    /// SampleProvider which zeroes the passthrough audio buffer when enabled.
    /// </summary>
    internal TrackStateSampleProvider TrackStateSampleProvider { get; set; }

    #endregion

    /// <summary>
    /// Name of the track.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    private bool _muted;

    /// <summary>
    /// Get or set the track mute state.
    /// </summary>
    public bool Muted
    {
        get => _muted;
        set
        {
            TrackStateSampleProvider.Enabled = value;
            _muted = value;
        }
    }

    private float _volume = 0.0f;

    /// <summary>
    /// Control the track volume in dB.
    /// <para/>
    /// <br/> Default = 0
    /// <br/> Range [-90, 6]
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if volume is out of range</exception>
    public float Volume
    {
        get => _volume;
        set
        {
            if (value < -90f || value > 6f)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Volume should be between -90 and 6.");
            }

            _volume = value;
            StereoSampleProvider.SetGain(Extensions.ToLinearVolume(value));
        }
    }

    private float _pan = 0.0f;

    /// <summary>
    /// Control the track pan.
    /// <para/>
    /// <br/> Default = 0
    /// <br/> Range [-50, 50]
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if pan is out of range</exception>
    public float Pan
    {
        get => _pan;
        set
        {
            if (value < -50f || value > 50f)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Pan should be between -50 and 50.");
            }

            _pan = value;
            StereoSampleProvider.Pan = _pan / 50f;
        }
    }

    /// <summary>
    /// If set to true, the plugin instrument of the track will receive midi events from the <see cref="MidiInputDevice"/>.
    /// </summary>
    public bool ReceiveMidiInput { get; set; }

    /// <summary>
    /// Check if the track is recording.
    /// </summary>
    public bool IsRecording { get; protected set; }

    /// <summary>
    /// Return true if this is an audio track.
    /// </summary>
    public bool IsAudioTrack => this is AudioTrack;

    /// <summary>
    /// Return true if this is a midi track.
    /// </summary>
    public bool IsMidiTrack => this is MidiTrack;

    /// <summary>
    /// Return true if this is a track group.
    /// </summary>
    public bool IsTrackGroup => this is TrackGroup;

    /// <summary>
    /// The group of this track if any.
    /// </summary>
    public TrackGroup? TrackGroup { get; internal set; }

    /// <summary>
    /// Return true if this track is inside a track group.
    /// </summary>
    public bool IsInGroup => TrackGroup != null;

    /// <summary>
    /// Get the current gain of the left audio channel for this track.
    /// <br/> Useful for volume metering UI.
    /// </summary>
    public float LeftChannelGain { get; protected set; }

    /// <summary>
    /// Get the current gain of the right audio channel for this track.
    /// <br/> Useful for volume metering UI.
    /// </summary>
    public float RightChannelGain { get; protected set; }

    /// <summary>
    /// Returns the track final sample provider.
    /// </summary>
    internal ISampleProvider GetTrackAudio()
    {
        return TrackStateSampleProvider;
    }

    /// <summary>
    /// Start the playback of a clip.
    /// </summary>
    /// <param name="clip"></param>
    internal abstract void FireClip(Clip clip);

    /// <summary>
    /// Start recording audio from the input device or midi from midi input device.
    /// </summary>
    /// <param name="destPath">Path where the recorded file will be saved on <see cref="StopRecording"/>.</param>
    public abstract void StartRecording(string destPath);

    /// <summary>
    /// Stop recording and create a new clip from the recorded data.
    /// </summary>
    /// <returns>A new clip created from the recorded data.</returns>
    public abstract Clip? StopRecording();

    /// <summary>
    /// Stop all playing track sounds.
    /// </summary>
    public abstract void StopSounds();

    /// <summary>
    /// Add a clip to this track.
    /// </summary>
    /// <param name="clip">The clip to add.</param>
    /// <returns>A <see cref="OperationResult"/> representing the success or failure of the operation.</returns>
    public OperationResult AddClip(Clip clip)
    {
        if (Clips.Contains(clip))
        {
            return OperationResult.Failure($"Clip {clip.Id} is already present in this track. Create a new one using: new {clip.GetType().Name}()");
        }

        if ((IsAudioTrack && clip.IsAudioClip) || (IsMidiTrack && clip.IsMidiClip))
        {
            _clips.Add(clip);
            ClipAdded?.Invoke(this, new ClipAddedOrRemovedEventArgs(clip));
            return OperationResult.Success("Clip added successfully.");
        }
        return OperationResult.Failure($"Cannot add clip of type {clip.GetType()} to track of type {this.GetType()}.");
    }

    /// <summary>
    /// Remove a clip from this track.
    /// </summary>
    /// <param name="clip">The clip to remove.</param>
    /// <returns>True if the clip was removed, False if not found or couldn't be removed.</returns>
    public bool RemoveClip(Clip clip)
    {
        bool removed = _clips.Remove(clip);
        if (removed) 
        {
            ClipRemoved?.Invoke(this, new ClipAddedOrRemovedEventArgs(clip));
        }
        return removed;
    }

    /// <summary>
    /// Remove all clips from this track.
    /// </summary>
    public void RemoveAllClips()
    {
        foreach (var clip in _clips.ToList())
        {
            bool removed = _clips.Remove(clip);
            if (removed)
            {
                ClipRemoved?.Invoke(this, new ClipAddedOrRemovedEventArgs(clip));
            }
        }      
    }

    /// <summary>
    /// Add a plugin to the track plugins chain.
    /// </summary>
    /// <param name="plugin">The plugin to add.</param>
    /// <returns>A <see cref="OperationResult"/> representing the success or failure of the operation.</returns>
    public OperationResult AddPlugin(IPlugin plugin)
    {
        if (PluginChainSampleProvider.PluginInstrument == plugin || PluginChainSampleProvider.FxPlugins.Contains(plugin))
        {
            return OperationResult.Failure($"Plugin instance already present in this track. Create a new one using: new {plugin.GetType().Name}().");
        }

        if (IsAudioTrack && plugin.PluginType == PluginType.Instrument)
        {
            return OperationResult.Failure("Cannot add an instrument plugin to an audio track.");
        }
        PluginChainSampleProvider.AddPlugin(plugin);
        PluginAdded?.Invoke(this, new PluginAddedOrRemovedEventArgs(plugin));
        return OperationResult.Success("Plugin added successfully.");
    }

    /// <summary>
    /// Remove a plugin from the track plugins chain.
    /// </summary>
    /// <param name="plugin">The plugin to remove.</param>
    public void RemovePlugin(IPlugin plugin)
    {
        bool removed = PluginChainSampleProvider.RemovePlugin(plugin);
        if (removed)
        {
            PluginRemoved?.Invoke(this, new PluginAddedOrRemovedEventArgs(plugin));
        }
    }

    /// <summary>
    /// Swap two effect plugins by index.
    /// </summary>
    /// <param name="index1">Index of the first plugin in the plugins chain.</param>
    /// <param name="index2">Index of the second plugin in the plugins chain.</param>
    /// <returns>A <see cref="OperationResult"/> representing the success or failure of the operation.</returns>
    public OperationResult SwapFxPlugins(int index1, int index2)
    {
        if (PluginChainSampleProvider.SwapFxPlugins(index1, index2))
        {
            return OperationResult.Success("FX plugins swapped successfully.");
        }
        return OperationResult.Failure("Invalid plugin indices specified.");
    }

    /// <summary>
    /// Remove all plugins from the track plugins chain.
    /// </summary>
    public void RemoveAllPlugins()
    {
        RemovePlugin(PluginChainSampleProvider.PluginInstrument);
        foreach (var plugin in PluginChainSampleProvider.FxPlugins.ToList())
        {
            RemovePlugin(plugin);
        }
    }
}
