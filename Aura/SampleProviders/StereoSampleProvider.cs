using Aura.Utils;
using NAudio.Wave;

namespace Aura.SampleProviders;

/// <summary>
/// SampleProvider which control Volume and Pan of the provided audio source.
/// </summary>
internal class StereoSampleProvider : ISampleProvider
{
    private readonly ISampleProvider _source;

    /// <summary>
    /// Left channel volume
    /// </summary>
    public float LeftVolume { get; set; } = 1.0f;

    /// <summary>
    /// Right channel volume
    /// </summary>
    public float RightVolume { get; set; } = 1.0f;

    /// <summary>
    /// Pan
    /// <br/> Range [-1, 1]
    /// </summary>
    public float Pan { get; set; } = 0.0f; // -1.0f (left) to 1.0f (right)

    public StereoSampleProvider(ISampleProvider source)
    {
        if (source.WaveFormat.Channels != 2)
            throw new ArgumentException("Source must be stereo");
        this._source = source;
    }

    public WaveFormat WaveFormat => _source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = _source.Read(buffer, offset, count);
        for (int i = 0; i < samplesRead; i += 2)
        {
            float left = buffer[offset + i];
            float right = buffer[offset + i + 1];

            // Apply panning
            float panLeft = Pan <= 0 ? 1.0f : 1.0f - Pan;
            float panRight = Pan >= 0 ? 1.0f : 1.0f + Pan;

            // Apply volume and panning
            buffer[offset + i] = left * LeftVolume * panLeft;
            buffer[offset + i + 1] = right * RightVolume * panRight;
        }
        return samplesRead;
    }

    /// <summary>
    /// Set volume across both channels
    /// </summary>
    /// <param name="gain">Linear gain. [0, ~1]
    /// <br/> 
    /// Use <see cref="Extensions.ToLinearVolume(float)"/> to convert to linear volume.
    /// </param>
    public void SetGain(float gain)
    {
        LeftVolume = gain;
        RightVolume = gain;
    }
}
