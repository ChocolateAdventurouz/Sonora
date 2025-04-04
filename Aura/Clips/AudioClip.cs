using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using Aura.SampleProviders;
using Aura.Tracks;
using Aura.Automations;
using Aura.Utils;

namespace Aura.Clips;

public class AudioClip : Clip
{
    private SoundTouchSampleProvider _soundtouchProvider;
    private StereoSampleProvider _stereoProvider;

    /// <summary>
    /// Create a new audio clip from file path.
    /// </summary>
    /// <param name="filePath">The path of the audio file.</param>
    
    public AudioClip(string filePath)
    {
        Name = Path.GetFileNameWithoutExtension(filePath);
        AudioFile = new AudioFileReader(filePath);

        VolumeChanged += (sender, e) => {
            if (IsPlaying()) {
                _stereoProvider.SetGain(Extensions.ToLinearVolume(e.NewValue));
            }
        };

        PanChanged += (sender, e) => {
            if (IsPlaying()) {
                _stereoProvider.Pan = e.NewValue / 50f;
            }
        };

        PitchChanged += (sender, e) => {
            if (IsPlaying()) {
                _soundtouchProvider.PitchSemiTones = e.NewValue;
            }
        };
    }

    /// <summary>
    /// Create a new audio clip from file path and parent track.
    /// </summary>
    /// <param name="filePath">The path of the audio file.</param>
    /// <param name="parentTrack">Track of this clip.</param>
    internal AudioClip(string filePath, Track? parentTrack)
    {
        Name = Path.GetFileNameWithoutExtension(filePath);
        AudioFile = new AudioFileReader(filePath);
        parentTrack.AddClip(this);

        VolumeChanged += (sender, e) => {
            if (IsPlaying()) {
                _stereoProvider?.SetGain(Extensions.ToLinearVolume(e.NewValue));
            }
        };

        PanChanged += (sender, e) => {
            if (IsPlaying()) {
                _stereoProvider.Pan = e.NewValue / 50f;
            }
        };

        PitchChanged += (sender, e) => {
            if (IsPlaying()) {
                _soundtouchProvider.PitchSemiTones = e.NewValue;
            }
        };
    }

    /// <inheritdoc/>
    public override void Play()
    {
        if (!IsInTrack)
        {
            throw new Exception("Clip must be inside a track to be played.");
        }

        if (IsPlaying() || !Enabled)
            return;

        // Apply pitch and tempo
        _soundtouchProvider = new SoundTouchSampleProvider(AudioFile)
        {
            PitchSemiTones = Pitch,
            Tempo = Speed
        };

        // Apply time offsets
        double playDuration = Duration;
        AudioFile.CurrentTime = TimeSpan.FromSeconds(StartMarker); // faster than SkipOver

        // Apply fading In/Out
        var fade = new FadeInOutSampleProvider(_soundtouchProvider);
        if (FadeIn > 0)
            fade.BeginFadeIn(FadeIn * 1000);

        // Kinda bad but seems to works for now
        if (FadeOut > 0)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (PlaybackStopWatch.Elapsed.TotalSeconds >= playDuration - FadeOut)
                    {
                        fade.BeginFadeOut(FadeOut * 1000);
                        Console.WriteLine($"Time: {CurrentTime:n3}");
                        break;
                    }
                }
            });
        }

        // Apply volume and pan
        _stereoProvider = new StereoSampleProvider(fade)
        {
            Pan = Pan / 50f
        };
        _stereoProvider.SetGain(Extensions.ToLinearVolume(Volume));

        // Play the audio file
        SampleProvider = _stereoProvider;
        Track.FireClip(this);

        // Start playback timer
        PlaybackStopWatch.Restart();
        PlaybackStopWatch.SetTime(TimeSpan.FromSeconds(StartMarker));

        // Start automations
        StartAutomations();
    }

    /// <inheritdoc/>
    public override void Stop()
    {
        AudioFile.CurrentTime = TimeSpan.Zero;
        Track?.Mixer.RemoveMixerInput(SampleProvider);
        if (IsInTrack && SampleProvider != null)
        {
            // bad implementation
            var audioTrack = Track as AudioTrack;
            audioTrack._mixerClips.Remove(SampleProvider);
        }
        AutomationCTS?.Cancel();
        PlaybackStopWatch.Reset();
    }

    /// <inheritdoc/>
    protected override void Stop(bool resetTimer)
    {
        AudioFile.CurrentTime = TimeSpan.Zero;
        Track?.Mixer.RemoveMixerInput(SampleProvider);
        if (IsInTrack && SampleProvider != null)
        {
            // bad implementation
            var audioTrack = Track as AudioTrack;
            audioTrack._mixerClips.Remove(SampleProvider);
        }
        AutomationCTS?.Cancel();

        if (resetTimer)
            PlaybackStopWatch.Reset();
        else 
            PlaybackStopWatch.Stop();
    }

    /// <inheritdoc/>
    public override void Seek(double time)
    {
        if (!IsPlaying())
        {
            throw new InvalidOperationException("Cannot seek if clip is not playing.");
        }

        if (time < StartMarker)
        {
            throw new ArgumentOutOfRangeException(nameof(time), $"Seek time must be between {StartMarker:n2} and {GetDuration():n2}. (file duration)");
        }

        if (time >= EndMarker && EndMarker != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(time), $"Seek time must be between {StartMarker:n2} and {EndMarker:n2}. (EndMarker)");
        }

        if (time > GetDuration() && EndMarker == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(time), $"Seek time must be between {StartMarker:n2} and {GetDuration():n2}. (file duration)");
        }

        AudioFile.CurrentTime = TimeSpan.FromSeconds(time);
        PlaybackStopWatch.SetTime(TimeSpan.FromSeconds(time));
    }

    /// <inheritdoc/>
    protected override string GetFilePath()
    {
        return AudioFile.FileName;
    }

    /// <inheritdoc/>
    public override double GetDuration()
    {
        return AudioFile.TotalTime.TotalSeconds;
    }

    /// <inheritdoc/>
    public override Clip Split(double seconds)
    {
        if (seconds <= 0 || (seconds >= EndMarker && EndMarker != 0))
        {
            throw new ArgumentOutOfRangeException(nameof(seconds), 
                $"Cannot split clip out of its range. Split must be between {StartMarker:n2} and {EndMarker:n2}. (EndMarker)");
        }

        this.EndMarker = seconds;
        return new AudioClip(FilePath, this.Track) {
            StartMarker = seconds,

            // Copy settings
            Enabled = this.Enabled,
            Name = this.Name,
            Pan = this.Pan,
            Pitch = this.Pitch,
            Speed = this.Speed,
            Volume = this.Volume,
        }.CopyAutomationsFrom(this);
    }

    /// <inheritdoc/>
    public override (Clip middle, Clip right) SplitFromTo(double start, double end)
    {
        if (end <= start)
        {
            throw new ArgumentOutOfRangeException(nameof(end),
                $"Split end must be greater than split start.");
        }

        if (start <= StartMarker || (start >= EndMarker && EndMarker != 0))
        {
            throw new ArgumentOutOfRangeException(nameof(start),
                $"Cannot split clip out of its range. Split start must be between {StartMarker:n2} and {EndMarker:n2}. (EndMarker)");
        }

        if (end >= EndMarker && EndMarker != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(end),
                $"Cannot split clip out of its range. Split end must be between {start:n2} and {EndMarker:n2}. (EndMarker)");
        }

        this.EndMarker = start;
        return (
            new AudioClip(FilePath, this.Track) { 
                StartMarker = start, 
                EndMarker = end,

                // Copy settings
                Enabled = this.Enabled,
                Name = this.Name,
                Pan = this.Pan,
                Pitch = this.Pitch,
                Speed = this.Speed,
                Volume = this.Volume,
            }.CopyAutomationsFrom(this), 
            new AudioClip(FilePath, this.Track) { 
                StartMarker = end,

                // Copy settings
                Enabled = this.Enabled,
                Name = this.Name,
                Pan = this.Pan,
                Pitch = this.Pitch,
                Speed = this.Speed,
                Volume = this.Volume,
            }.CopyAutomationsFrom(this)
        );
    }

    /// <inheritdoc/>
    public override Clip CutOut(double start, double end)
    {
        if (end <= start)
        {
            throw new ArgumentOutOfRangeException(nameof(end),
                $"Cut end must be greater than cut start.");
        }

        if (start <= StartMarker || (start >= EndMarker && EndMarker != 0))
        {
            throw new ArgumentOutOfRangeException(nameof(start),
                $"Cannot cut clip out of its range. Cut start must be between {StartMarker:n2} and {EndMarker:n2}. (EndMarker)");
        }

        if (end >= EndMarker && EndMarker != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(end),
                $"Cannot cut clip out of its range. Cut end must be between {start:n2} and {EndMarker:n2}. (EndMarker)");
        }

        this.EndMarker = start;
        return new AudioClip(FilePath, this.Track) { 
            StartMarker = end,

            // Copy settings
            Enabled = this.Enabled,
            Name = this.Name,
            Pan = this.Pan,
            Pitch = this.Pitch,
            Speed = this.Speed,
            Volume = this.Volume,
        }.CopyAutomationsFrom(this);
    }

    /// <inheritdoc/>
    public override void Reverse(string destDirectory)
    {
        if (IsPlaying())
            return;

        if (!Directory.Exists(destDirectory))
            throw new DirectoryNotFoundException($"Directory not found: {destDirectory}");

        AudioFile?.Dispose();

        string purged = Path.GetFileNameWithoutExtension(FilePath).Replace("_rev", string.Empty);
        string tmpFile = Path.Combine(destDirectory, $"{purged}_rev.wav");

        // Use a temporary file first (to avoid locking issues)
        string tempFile = Path.Combine(destDirectory, $"{purged}_rev_tmp.wav");

        try
        {
            // Read and reverse audio
            using (var reader = new AudioFileReader(FilePath))
            {
                var samples = new float[reader.Length / (reader.WaveFormat.BitsPerSample / 8)];
                int samplesRead = reader.Read(samples, 0, samples.Length);
                ReverseSamples(samples, reader.WaveFormat.Channels);

                // Write to TEMPORARY file first
                using (var writer = new WaveFileWriter(tempFile, reader.WaveFormat))
                {
                    writer.WriteSamples(samples, 0, samplesRead);
                }
            }

            // Delete target file if it exists (avoid "in use" errors)
            if (File.Exists(tmpFile))
            {
                File.Delete(tmpFile); // Force delete (may throw if still locked)
            }

            // Rename temp file to final file
            File.Move(tempFile, tmpFile);
        }
        catch (IOException ex)
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);

            throw new IOException("Failed to reverse audio (file locked?): " + ex.Message);
        }

        // Load the reversed file
        AudioFile = new AudioFileReader(tmpFile);
    }

    private static void ReverseSamples(float[] samples, int channels)
    {
        // Ensure we reverse while keeping multi-channel samples together
        for (int i = 0; i < samples.Length / 2; i += channels)
        {
            for (int c = 0; c < channels; c++)
            {
                int leftPos = i + c;
                int rightPos = samples.Length - (i + channels) + c;

                // Swap samples
                float temp = samples[leftPos];
                samples[leftPos] = samples[rightPos];
                samples[rightPos] = temp;
            }
        }
    }

    public override Clip Duplicate()
    {
        return new AudioClip(FilePath, this.Track)
        {
            Enabled = this.Enabled,
            EndMarker = this.EndMarker,
            FadeIn = this.FadeIn,
            FadeOut = this.FadeOut,
            Name = this.Name,
            Pan = this.Pan,
            Pitch = this.Pitch,
            Speed = this.Speed,
            StartMarker = this.StartMarker,
            Time = this.Time,
            Volume = this.Volume
        }.CopyAutomationsFrom(this);
    }

    /// <inheritdoc/>
    protected override void ApplyParameter(AutomationParameter parameter, float value)
    {
        switch (parameter)
        {
            case AutomationParameter.Volume:
                Volume = value;
                _stereoProvider?.SetGain(Extensions.ToLinearVolume(value));
                break;
            case AutomationParameter.Pan:
                Pan = value;
                if (_stereoProvider != null) _stereoProvider.Pan = value / 50f;
                break;
            case AutomationParameter.Pitch:
                Pitch = value;
                if (_soundtouchProvider != null) _soundtouchProvider.PitchSemiTones = value;
                break;
        }
    }
}
