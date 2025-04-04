using Aura.Clips;
using Aura.Plugins;
using Aura.SampleProviders;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Aura.Tracks;

/// <summary>
/// Represent an audio track which can play audio files and process sound through plugins. 
/// </summary>
public sealed class AudioTrack : Track
{
    /// <inheritdoc/>
    public override event EventHandler<StreamVolumeEventArgs> VolumeMeasured;

    private WaveInEvent _inputDevice;
    private WaveFileWriter _waveFileWriter;
    private string _lastRecordPath;
    internal Dictionary<ISampleProvider, Clip> _mixerClips = new();

    /// <summary>
    /// Create a new audio track.
    /// </summary>
    public AudioTrack(string name = "")
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

        Mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(AuraMain.SampleRate, 2))
        {
            ReadFully = true
        };

        Mixer.MixerInputEnded += (sender, e) => { 
            if (_mixerClips.ContainsKey(e.SampleProvider))
            {
                _mixerClips[e.SampleProvider].PlaybackStopWatch.Stop();
                _mixerClips.Remove(e.SampleProvider);
            }
        };

        // Initialize plugins chain to apply plugins effects (returns audio processed by all track vst's)
        PluginChainSampleProvider = new PluginChainSampleProvider(Mixer);

        // Initialize custom StereoSampleProvider for volume and pan control
        StereoSampleProvider = new StereoSampleProvider(PluginChainSampleProvider);

        // Initialize MeteringSampleProvider for gain feedback
        MeteringSampleProvider = new MeteringSampleProvider(StereoSampleProvider);
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
        if (!Mixer.WaveFormat.Equals(clip.SampleProvider.WaveFormat))
        {
            // try resampling if sample rate doesn't match
            var resampler = new WdlResamplingSampleProvider(clip.SampleProvider, Mixer.WaveFormat.SampleRate);
            clip.SampleProvider = resampler;
        }
        Mixer.AddMixerInput(clip.SampleProvider);
        _mixerClips.Add(clip.SampleProvider, clip);
    }

    /// <inheritdoc/>
    public override void StartRecording(string destPath)
    {
        if (IsRecording)
            return;

        _lastRecordPath = destPath;

        _inputDevice = new WaveInEvent
        {
            WaveFormat = new WaveFormat(Mixer.WaveFormat.SampleRate, Mixer.WaveFormat.Channels)
        };

        _waveFileWriter = new WaveFileWriter(destPath, _inputDevice.WaveFormat);

        _inputDevice.DataAvailable += (s, e) =>
        {
            _waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
            _waveFileWriter.Flush();
        };

        _inputDevice.RecordingStopped += (s, e) =>
        {
            IsRecording = false;
        };

        _inputDevice.StartRecording();
        IsRecording = true;
    }

    /// <inheritdoc/>
    public override Clip? StopRecording()
    {
        if (!IsRecording)
            return null;

        if (_inputDevice != null)
        {
            _inputDevice.StopRecording();
            _inputDevice.Dispose();
            _inputDevice = null;
        }

        if (_waveFileWriter != null)
        {
            _waveFileWriter.Dispose();
            _waveFileWriter = null;
        }

        return new AudioClip(_lastRecordPath, this);
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
