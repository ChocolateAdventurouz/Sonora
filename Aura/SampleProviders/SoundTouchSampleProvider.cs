using NAudio.Wave;
using SoundTouch;

namespace Aura.SampleProviders;

/// <summary>
/// Wrapper around SoundTouchWaveProvider. Controls pitch, rate and tempo of the provided audio source.
/// </summary>
public class SoundTouchSampleProvider : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly SoundTouchProcessor _processor;
    private readonly float[] _sourceBuffer;
    private readonly float[] _outputBuffer;
    private int _outputBufferPos;
    private int _outputBufferAvailable;

    public SoundTouchSampleProvider(ISampleProvider source, int bufferSize = 4096)
    {
        this._source = source;
        this._processor = new SoundTouchProcessor();
        this._sourceBuffer = new float[bufferSize];
        this._outputBuffer = new float[bufferSize * 2]; // Extra space for processed samples

        _processor.Channels = source.WaveFormat.Channels;
        _processor.SampleRate = source.WaveFormat.SampleRate;
    }

    public WaveFormat WaveFormat => _source.WaveFormat;

    /// <summary>
    /// Gets or sets pitch change in semi-tones compared to the original pitch
    /// (eg. -12 .. +12).
    /// </summary>
    public double PitchSemiTones
    {
        get => _processor.PitchSemiTones;
        set => _processor.PitchSemiTones = value;
    }

    /// <summary>
    /// Gets or sets the tempo value (e.g 0.5 = half speed, 2 = twice speed).
    /// </summary>
    public double Tempo
    {
        get => _processor.Tempo;
        set => _processor.Tempo = value;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = 0;

        while (samplesRead < count)
        {
            if (_outputBufferAvailable > 0)
            {         
                // Copy from our output buffer
                int toCopy = Math.Min(_outputBufferAvailable, count - samplesRead);
                Array.Copy(_outputBuffer, _outputBufferPos, buffer, offset + samplesRead, toCopy);
                samplesRead += toCopy;
                _outputBufferPos += toCopy;
                _outputBufferAvailable -= toCopy;
            }
            else
            {
                // Need to process more samples
                int samplesToRead = Math.Min(_sourceBuffer.Length, count * 2); // Read enough for processing
                int sourceSamples = _source.Read(_sourceBuffer, 0, samplesToRead);

                if (sourceSamples == 0) break; // End of source

                // Process samples
                _processor.PutSamples(_sourceBuffer, sourceSamples / WaveFormat.Channels);

                // Get processed samples
                _outputBufferAvailable = _processor.ReceiveSamples(_outputBuffer, _outputBuffer.Length / WaveFormat.Channels) * WaveFormat.Channels;
                _outputBufferPos = 0;
            }
        }

        return samplesRead;
    }
}
