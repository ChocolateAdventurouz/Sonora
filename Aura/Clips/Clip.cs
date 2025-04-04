using Aura.Automations;
using Aura.EventArguments;
using Aura.SampleProviders;
using Aura.Tracks;
using Aura.Utils;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using NAudio.Wave;

namespace Aura.Clips;

/// <summary>
/// Represent an audio or midi clip.
/// </summary>
public abstract class Clip
{
    /// <summary>
    /// Clip unique Id.
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString();

    private string _name = string.Empty;
    /// <summary>
    /// Clip name. (file name if created from a file path, else empty string)
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            var oldName = _name;
            if (_name != value)
            {
                _name = value;
                NameChanged?.Invoke(this, new NameChangedEventArgs(oldName, value));
            }
        }
    }

    /// <summary>
    /// Event called when <see cref="Name"/> property has been changed.
    /// </summary>
    public event EventHandler<NameChangedEventArgs>? NameChanged;

    /// <summary>
    /// File path if any, else empty string.
    /// </summary>
    public string FilePath => GetFilePath();

    private bool _enabled = true;
    /// <summary>
    /// Clip state. Clip won't be played if disabled.
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set
        {
            var oldState = _enabled;
            if (_enabled != value)
            {
                _enabled = value;
                EnableChanged?.Invoke(this, new StateChangedEventArgs(oldState, value));
            }
        }
    }

    /// <summary>
    /// Event called when <see cref="Enabled"/> property has been changed.
    /// </summary>
    public event EventHandler<StateChangedEventArgs>? EnableChanged;

    private double _time = 0.0;
    /// <summary>
    /// Clip timeline time in seconds. (not used by the framework but useful for the user to place clips on a timeline)
    /// </summary>
    public double Time
    {
        get => _time;
        set
        {
            var oldTime = _time;
            if (_time != value)
            {
                _time = value;
                TimeChanged?.Invoke(this, new TimeChangedEventArgs(oldTime, value));
            }
        }
    }

    /// <summary>
    /// Even called when <see cref="Time"/> property has been changed.
    /// </summary>
    public event EventHandler<TimeChangedEventArgs>? TimeChanged;

    /// <summary>
    /// Clip current playback time.
    /// </summary>
    public double CurrentTime => PlaybackStopWatch.Elapsed.TotalSeconds;

    //(GetDuration() - StartMarker - (EndMarker - StartMarker)) / Speed;
    /// <summary>
    /// Clip duration in seconds taking into account the <see cref="StartMarker"/>, <see cref="EndMarker"/> and <see cref="Speed"/>.
    /// </summary>
    public double Duration => EndMarker == 0 
        ? (GetDuration() - StartMarker) / Speed 
        : (EndMarker - StartMarker) / Speed;

    /// <inheritdoc cref="IsPlaying"/>
    public bool Playing => IsPlaying();

    private double _startMarker = 0.0;
    /// <summary>
    /// Clip starting time in seconds relative to its content.
    /// </summary>
    public double StartMarker
    {
        get => _startMarker;
        set
        {
            var totalDuration = GetDuration();
            if (value < 0 || value >= totalDuration)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"StartMarker must be between 0 and {totalDuration:n2}. (file duration)");
            }

            if (EndMarker > 0 && value >= EndMarker)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"StartMarker must be between 0 and {EndMarker:n2}. (EndMarker)");
            }

            var oldStartOffset = _startMarker;
            if (_startMarker != value)
            {
                _startMarker = value;
                StartMarkerChanged?.Invoke(this, new TimeChangedEventArgs(oldStartOffset, value));
            }
        }
    }

    /// <summary>
    /// Event called when <see cref="StartMarker"/> property has been changed.
    /// </summary>
    public event EventHandler<TimeChangedEventArgs>? StartMarkerChanged;

    private double _endMarker = 0.0;
    /// <summary>
    /// Clip ending time in seconds relative to its content. (Zero means play to end)
    /// </summary>
    public double EndMarker
    {
        get => _endMarker;
        set
        {
            var totalDuration = GetDuration();
            if (value < 0 || value > totalDuration || (StartMarker > 0 && value <= StartMarker && value != 0))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"EndMarker must be Zero or between {StartMarker} and {totalDuration:n2}. (file duration)");
            }

            var oldEndMarker = _endMarker;
            if (_endMarker != value)
            {
                _endMarker = value;
                EndMarkerChanged?.Invoke(this, new TimeChangedEventArgs(oldEndMarker, value));
            }
        }
    }

    /// <summary>
    /// Event called when <see cref="EndMarker"/> property has been changed.
    /// </summary>
    public event EventHandler<TimeChangedEventArgs>? EndMarkerChanged;

    private double _fadeIn = 0.0;
    /// <summary>
    /// Fade in time in seconds. (audio clips only)
    /// </summary>
    public double FadeIn
    {
        get => _fadeIn;
        set
        {
            var totalDuration = GetDuration();
            if (value < 0 || value > totalDuration)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"FadeIn must be between 0 and {totalDuration:n2} (file duration).");
            }

            if (IsMidiClip)
            {
                throw new InvalidOperationException("Cannot set fade in of midi clips.");
            }

            _fadeIn = value;
        }
    }

    private double _fadeOut = 0.0;
    /// <summary>
    /// Fade out time in seconds. (audio clips only)
    /// </summary>
    public double FadeOut
    {
        get => _fadeOut;
        set
        {
            var totalDuration = GetDuration();
            if (value < 0 || (FadeIn + value) >= totalDuration)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"FadeOut is too large. After fading in {FadeIn:n2}s, only {totalDuration - FadeIn:n2}s remain.");
            }

            if (IsMidiClip)
            {
                throw new InvalidOperationException("Cannot set fade out of midi clips.");
            }

            _fadeOut = value;
        }
    }

    private float _volume = 0.0f;
    /// <summary>
    /// Clip volume in dB. (audio clips only)
    /// <para/>
    /// <br/> Default = 0
    /// <br/> Range [-90, 6]
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Threw if volume is out of range</exception>
    public float Volume
    {
        get => _volume;
        set
        {
            if (value < -90f || value > 6f)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Volume should be between -90 and 6.");
            }

            if (IsMidiClip)
            {
                throw new InvalidOperationException("Cannot set volume of midi clips.");
            }

            var oldVolume = _volume;
            if (_volume != value)
            {
                _volume = value;
                VolumeChanged?.Invoke(this, new ValueChangedEventArgs(oldVolume, value));
            }       
        }
    }

    /// <summary>
    /// Event called whenever <see cref="Volume"/> property has been changed.
    /// </summary>
    public event EventHandler<ValueChangedEventArgs>? VolumeChanged;

    private float _pan = 0.0f;
    /// <summary>
    /// Clip pan amount. (audio clips only)
    /// <para/>
    /// <br/> Default = 0
    /// <br/> Range [-50, 50]
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Threw if pan is out of range</exception>
    public float Pan
    {
        get => _pan;
        set
        {
            if (value < -50f || value > 50f)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Pan should be between -50 and 50.");
            }

            if (IsMidiClip)
            {
                throw new InvalidOperationException("Cannot set pan of midi clips.");
            }

            var oldPan = _pan;
            if (_pan != value)
            {
                _pan = value;
                PanChanged?.Invoke(this, new ValueChangedEventArgs(oldPan, value));
            }
        }
    }

    /// <summary>
    /// Event called whenever <see cref="Pan"/> property has been changed.
    /// </summary>
    public event EventHandler<ValueChangedEventArgs>? PanChanged;

    private float _pitch = 0.0f;
    /// <summary>
    /// <inheritdoc cref="SoundTouchSampleProvider.PitchSemiTones"/>
    /// (audio clips only)
    /// </summary>
    public float Pitch
    {
        get => _pitch;
        set
        {
            if (IsMidiClip)
            {
                throw new InvalidOperationException("Cannot set pitch of midi clips.");
            }

            var oldPitch = _pitch;
            if (_pitch != value)
            {
                _pitch = value;
                PitchChanged?.Invoke(this, new ValueChangedEventArgs(oldPitch, value));
            }
        }
    }

    /// <summary>
    /// Event called whenever <see cref="Pitch"/> property has been changed.
    /// </summary>
    public event EventHandler<ValueChangedEventArgs>? PitchChanged;

    private float _speed = 1.0f;
    /// <summary>
    /// <inheritdoc cref="SoundTouchSampleProvider.Tempo"/>
    /// (audio clips only)
    /// <br/> Cannot be changed during playback.
    /// </summary>
    public float Speed
    {
        get => _speed;
        set
        {
            if (IsMidiClip)
            {
                throw new InvalidOperationException("Cannot set speed of midi clips.");
            }

            if (value <= 0)
            {
                throw new InvalidOperationException("Speed must be greater than zero.");
            }

            if (IsPlaying())
            {
                throw new InvalidOperationException("Cannot set speed during playback.");
            }

            _speed = value;
        }
    }

    /// <summary>
    /// Returns true if this is an audio clip.
    /// </summary>
    public bool IsAudioClip => this is AudioClip;

    /// <summary>
    /// Returns true if this is a midi clip.
    /// </summary>
    public bool IsMidiClip => this is MidiClip;

    /// <summary>
    /// The track of this clip.
    /// </summary>
    public Track? Track { get; internal set; }

    /// <summary>
    /// Returns true if this clip is in a track.
    /// </summary>
    public bool IsInTrack => Track != null;

    /// <summary>
    /// Final SampleProvider if this is an audio clip.
    /// </summary>
    internal ISampleProvider? SampleProvider { get; set; }

    private AudioFileReader? _audioFile;
    /// <summary>
    /// Audio file if this is an audio clip.
    /// </summary>
    public AudioFileReader? AudioFile
    {
        get => _audioFile;
        protected set
        {
            _audioFile = value;
        }
    }

    /// <summary>
    /// Midi playback if this is a midi clip.
    /// </summary>
    internal Playback? MidiPlayback { get; set; }

    private MidiFile? _midiFile;
    /// <summary>
    /// Midi file data if this is a midi clip.
    /// </summary>
    public MidiFile? MidiFile
    {
        get => _midiFile;
        protected set
        {
            _midiFile = value;
        }
    }

    /// <summary>
    /// Keep track of the current playback time.
    /// </summary>
    internal AdjustableStopwatch PlaybackStopWatch { get; set; } = new();

    private readonly Dictionary<AutomationParameter, AutomationLane> _automations = new();
    /// <summary>
    /// Automations of this clip.
    /// </summary>
    public IReadOnlyDictionary<AutomationParameter, AutomationLane> Automations => _automations;

    /// <summary>
    /// Automation cancellation token source.
    /// </summary>
    protected CancellationTokenSource? AutomationCTS { get; set; }

    #region Abstract methods

    /// <summary>
    /// Get file path if any.
    /// </summary>
    /// <returns>The path of the file used by this clip.</returns>
    protected abstract string GetFilePath();

    /// <summary>
    /// Get the clip real duration in seconds without accounting starting/ending offsets.
    /// </summary>
    /// <returns></returns>
    public abstract double GetDuration();

    /// <summary>
    /// Split the clip at the specified time.
    /// </summary>
    /// <param name="seconds">Seconds at which the clip will be splitted.</param>
    /// <returns>A new clip representing the rightmost splitted part.</returns>
    public abstract Clip Split(double seconds);

    /// <summary>
    /// Split the clip from time to time.
    /// </summary>
    /// <param name="start">Seconds at which the clip split will start.</param>
    /// <param name="end">Seconds at which the clip split will end.</param>
    /// <returns>Two new clips representing the middle and rightmost splitted parts.</returns>
    public abstract (Clip middle, Clip right) SplitFromTo(double start, double end);

    /// <summary>
    /// Cut out a part from the clip.
    /// </summary>
    /// <param name="start">Starting time in seconds of the cut.</param>
    /// <param name="end">Ending time in seconds of the cut.</param>
    /// <returns>A new clip representing the rightmost part.</returns>
    public abstract Clip CutOut(double start, double end);

    /// <summary>
    /// Reverse the clip. (audio clips only)
    /// </summary>
    /// <param name="destDirectory">Directory where the reversed file will be saved.</param>
    public abstract void Reverse(string destDirectory);

    /// <summary>
    /// Create a copy of the clip.
    /// </summary>
    /// <returns>An unique copy of the clip.</returns>
    public abstract Clip Duplicate();

    /// <summary>
    /// Start this clip playback.
    /// </summary>
    public abstract void Play();

    /// <summary>
    /// Stop this clip playback.
    /// </summary>
    public abstract void Stop();

    /// <summary>
    /// Stop this clip playback and optionally reset or stop the playback timer. (used internally for automations)
    /// </summary>
    /// <param name="resetTimer"></param>
    protected abstract void Stop(bool resetTimer);

    /// <summary>
    /// Go to a specific time during playback.
    /// </summary>
    /// <param name="time">The time to go to in seconds.</param>
    public abstract void Seek(double time);

    /// <summary>
    /// Apply an automated parameter.
    /// </summary>
    /// <param name="parameter">The parameter type.</param>
    /// <param name="value">The value of the parameter.</param>
    protected abstract void ApplyParameter(AutomationParameter parameter, float value);

    #endregion

    #region Shared methods

    /// <summary>
    /// Return true if the clip is playing.
    /// </summary>
    /// <returns>The clip playback state.</returns>
    internal bool IsPlaying()
    {
        if (!IsInTrack)
            return false;

        if (IsAudioClip)
        {
            return Track.Mixer.MixerInputs.Contains(SampleProvider);
        }
        else if (IsMidiClip)
        {
            if (MidiPlayback == null)
            {
                return false;
            }
            return MidiPlayback.IsRunning;
        }
        return false;
    }

    /// <summary>
    /// Add an automation point for a parameter at a specified time.
    /// </summary>
    /// <param name="parameter">The parameter to automate.</param>
    /// <param name="time">Time in seconds at which to place the automation point.</param>
    /// <param name="value">The value of the parameter at point time.</param>
    /// <param name="interpolation">The interpolation type used for going to the next automation point.</param>
    public void AddAutomationPoint(AutomationParameter parameter, double time, float value, InterpolationType interpolation = InterpolationType.Linear)
    {
        if (!Automations.TryGetValue(parameter, out var track))
        {
            track = new AutomationLane { Parameter = parameter };
            _automations[parameter] = track;
        }

        // If there is already a point at passed time, update it
        if (track.Points.Any(p => p.Time == time))
        {
            var to_update = track.Points.First(value => value.Time == time);
            to_update.Value = value;
            to_update.Interpolation = interpolation;
        }
        else
        {
            track.Points.Add(new AutomationPoint
            {
                Time = time,
                Value = value,
                Interpolation = interpolation
            });
        }

        // Keep points sorted by time
        track.Points.Sort((a, b) => a.Time.CompareTo(b.Time));
    }

    /// <summary>
    /// Clear automation for a parameter.
    /// </summary>
    /// <param name="parameter">The automated parameter to remove.</param>
    public void ClearAutomation(AutomationParameter parameter)
    {
        _automations.Remove(parameter);
    }

    /// <summary>
    /// Clear all automations for this clip.
    /// </summary>
    public void ClearAllAutomations() => _automations.Clear();

    /// <summary>
    /// Process automation points during playback.
    /// </summary>
    /// <param name="duration"></param>
    protected void StartAutomations()
    {      
        AutomationCTS?.Cancel();
        AutomationCTS = new CancellationTokenSource();      

        Task.Run(async () =>
        {
            try
            {
                while (!AutomationCTS.IsCancellationRequested && IsPlaying())
                {
                    double currentTime = PlaybackStopWatch.Elapsed.TotalSeconds;
                    
                    // Process each automation lane
                    foreach (var (parameter, lane) in Automations)
                    {                    
                        float value = lane.GetValueAtTime(currentTime);
                        ApplyParameter(parameter, value);
                    }

                    // Stop the clip if EndMarker is reached (this is used when seeking to a new time)
                    if (currentTime >= EndMarker && EndMarker > 0)
                    {
                        Stop(false);
                    }

                    await Task.Delay(16, AutomationCTS.Token); // ~60fps update rate
                }
            }
            catch (TaskCanceledException)
            {
                // Normal cancellation
            }

        }, AutomationCTS.Token);
    }

    /// <summary>
    /// Copy all automations from another clip to this clip.
    /// </summary>
    /// <param name="other">The clip from which to copy the automations.</param>
    /// <returns>This clip with the other clip automations.</returns>
    protected Clip CopyAutomationsFrom(Clip other)
    {
        foreach (var automation in other.Automations)
        {
            var parameter = automation.Key;
            foreach (var point in automation.Value.Points)
            {
                AddAutomationPoint(parameter, point.Time, point.Value, point.Interpolation);
            }          
        }
        return this;
    }

    #endregion
}
