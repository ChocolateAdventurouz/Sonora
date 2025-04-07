using NAudio.Wave;

namespace Sonora.SampleProviders;

/// <summary>
/// Simple provider which fills the buffer with zeroes if enabled.
/// </summary>
internal class TrackStateSampleProvider : ISampleProvider
{
    /// <summary>
    /// State of the SampleProvider.
    /// </summary>
    public bool Enabled { get; set; }

    private readonly ISampleProvider _source;
    public WaveFormat WaveFormat => _source.WaveFormat;

    public TrackStateSampleProvider(ISampleProvider source)
    {
        this._source = source;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = _source.Read(buffer, offset, count);
        for (int i = 0; i < samplesRead; i += 2)
        {
            float leftChannel = buffer[offset + i];
            float rightChannel = buffer[offset + i + 1];

            if (Enabled)
            {
                leftChannel = 0;
                rightChannel = 0;
            }

            buffer[offset + i] = leftChannel;
            buffer[offset + i + 1] = rightChannel;
        }
        return samplesRead;
    }
}
