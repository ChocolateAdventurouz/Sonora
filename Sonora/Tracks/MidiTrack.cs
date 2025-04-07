using Sonora.Clips;
using Sonora.Plugins;
using Sonora.SampleProviders;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Sonora.Tracks;

/// <summary>
/// Represent a midi track which can play midi files and process sound through plugins.  
/// </summary>
public sealed class MidiTrack : Track
{
    /// <inheritdoc/>
    public override event EventHandler<StreamVolumeEventArgs> VolumeMeasured;

    private static Recording? _recInstance;
    private string _lastRecordPath = string.Empty;

    /// <summary>
    /// Create a new midi track.
    /// </summary>
    public MidiTrack(string name = "")
    {
        Name = name;

        ClipAdded += (sender, e) => {
            if (e.Clip.IsInTrack) // if clip is already in a track, remove it
            {
                e.Clip.Track.RemoveClip(e.Clip);
            }
            e.Clip.Track = this;
        };

        ClipRemoved += (sender, e) => {
            e.Clip.Stop();
            e.Clip.Track = null;
        };

        Mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(SonoraMain.SampleRate, 2))
        {
            ReadFully = true
        };

        // Initialize plugins chain to apply plugins effects (returns audio processed by all track vst's)
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

    /// <inheritdoc/>
    internal override void FireClip(Clip clip)
    {
        if (clip.MidiFile == null)
        {
            throw new ArgumentException($"Clip midi data was null.", nameof(clip));
        }

        // Send MIDI events to plugins
        clip.MidiPlayback.EventPlayed += (sender, e) =>
        {
            PluginChainSampleProvider.PluginInstrument?.ReceiveMidiEvent(e.Event);
        };

        clip.MidiPlayback.Start();
    }

    /// <inheritdoc/>
    public override void StartRecording(string destPath)
    {
        if (IsRecording)
            return;

        if (SonoraMain.MidiDevice == null || SonoraMain.MidiDevice.InDevice == null)
        {
            throw new Exception("To start recording select a midi input device with: SonoraMain.CreateMidiDevice().");
        }
        _lastRecordPath = destPath;
        _recInstance = new Recording(TempoMap.Default, SonoraMain.MidiDevice.InDevice);
        _recInstance.Stopped += (obj, e) => { 
            IsRecording = false;
        };
        _recInstance.Start();
        IsRecording = true;
    }

    /// <inheritdoc/>
    public override Clip? StopRecording()
    {
        if (!IsRecording)
            return null;

        _recInstance?.Stop();
        _recInstance?.Dispose();
        _recInstance?.ToFile().Write(_lastRecordPath, true);

        return new MidiClip(_lastRecordPath, this);
    }

    /// <inheritdoc/>
    public override void StopSounds()
    {
        foreach (var clip in Clips)
        {
            clip.Stop();
        }
    }
}
