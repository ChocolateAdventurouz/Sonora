using Aura.SampleProviders;
using Aura.Tracks;
using Aura.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Aura;

/// <summary>
/// Audio endpoint.
/// </summary>
public static class Master
{
    private static readonly List<Track> _tracks = new();

    /// <summary>
    /// Tracks added to the master mixer.
    /// </summary>
    public static IReadOnlyList<Track> Tracks => _tracks;

    private static MixingSampleProvider _masterMixer;
    private static StereoSampleProvider _stereoSampleProvider;
    private static MeteringSampleProvider _meteringProvider;

    public static event EventHandler<StreamVolumeEventArgs> VolumeMeasured;

    private static float _volume = 0.0f;

    /// <summary>
    /// Control the master volume in dB.
    /// <para/>
    /// <br/> Default = 0
    /// <br/> Range [-90, 6]
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if volume is out of range</exception>
    public static float Volume 
    { 
        get => _volume;
        set 
        { 
            if (value < -90f || value > 6f)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Volume should be between -90 and 6.");
            }

            _volume = value;
            _stereoSampleProvider.SetGain(Extensions.ToLinearVolume(value));
        }
    }

    private static float _pan = 0.0f;

    /// <summary>
    /// Control the master pan.
    /// <para/>
    /// <br/> Default = 0
    /// <br/> Range [-50, 50]
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if pan is out of range</exception>
    public static float Pan
    {
        get => _pan;
        set
        {
            if (value < -50f || value > 50f)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Pan should be between -50 and 50.");
            }

            _pan = value;
            _stereoSampleProvider.Pan = _pan / 50f;
        }
    }

    internal static void Init(int sampleRate)
    {
        // Create the mixer
        _masterMixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2))
        {
            ReadFully = true
        };

        // Create the stereoSampleProvide to allow Volume and Pan control
        _stereoSampleProvider = new StereoSampleProvider(_masterMixer);

        // Create the meteringProvider to provide metering data
        _meteringProvider = new MeteringSampleProvider(_stereoSampleProvider);
        _meteringProvider.StreamVolume += (s, e) => VolumeMeasured?.Invoke(null, e);

        // Start the framework audio device
        AuraMain.Device.OutputDevice.Init(_meteringProvider);
        AuraMain.Device.OutputDevice.Play();
    }

    /// <summary>
    /// Add a track to the master mixer.
    /// </summary>
    /// <param name="track">The track to add.</param>
    public static void AddTrack(Track track)
    {
        _tracks.Add(track);
        _masterMixer.AddMixerInput(track.GetTrackAudio());
    }

    /// <summary>
    /// Remove a track from the master mixer.
    /// </summary>
    /// <param name="track">The track to remove.</param>
    public static void RemoveTrack(Track track)
    {
        _tracks.Remove(track);
        _masterMixer.RemoveMixerInput(track.GetTrackAudio());
    }

    /// <summary>
    /// Remove all tracks from the master mixer.
    /// </summary>
    public static void RemoveAllTracks()
    {
        foreach (var track in _tracks.ToList())
        {
            track.StopSounds();
            _tracks.Remove(track);
        }
        _masterMixer.RemoveAllMixerInputs();
    }

    /// <summary>
    /// Stops all playing clips of each track.
    /// </summary>
    public static void StopSounds()
    {
        foreach (var track in Tracks)
        {
            track.StopSounds();
        }
    }
}
