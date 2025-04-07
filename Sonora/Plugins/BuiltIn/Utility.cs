using Melanchall.DryWetMidi.Core;

namespace Sonora.Plugins.BuiltIn;

/// <summary>
/// Plugin which controls volume, pan, width and channel polarity of the provided audio source.
/// </summary>
public class Utility : IPlugin
{
    /// <inheritdoc/>
    public bool Enabled { get; set; } = true;

    /// <inheritdoc/>
    public string PluginName { get; set; } = "Utility";

    /// <inheritdoc/>
    public string PluginId { get; } = Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public PluginType PluginType => PluginType.Effect;

    private float _volume = 1.0f;

    /// <summary>
    /// Get or set the volume. 
    /// <br/> Default: 1. Range [0, inf.]
    /// </summary>
    public float Volume {
        get => _volume;
        set 
        {
            if (value < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _volume = value;
        }
    }

    private float _pan = 0.0f;

    /// <summary>
    /// Get or set the panning amount. 
    /// <br/> Default: 0. Range [-1, 1]
    /// </summary>
    public float Pan
    {
        get => _pan;
        set
        {
            if (value < -1f || value > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _pan = value;
        }
    }

    private float _width = 0.0f;

    /// <summary>
    /// Get or set the width. 
    /// <br/> 0: default, -100: mono. Range [-100, 400]
    /// </summary>
    public float Width
    {
        get => _width;
        set
        {
            if (value < -100f || value > 400f)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _width = value;
        }
    }

    /// <summary>
    /// Get or set mono audio signal.
    /// </summary>
    public bool Mono { get; set; }

    /// <summary>
    /// Invert the left channel.
    /// </summary>
    public bool InvertLeft { get; set; }

    /// <summary>
    /// Invert the right channel.
    /// </summary>
    public bool InvertRight { get; set; }

    /// <inheritdoc/>
    void IPlugin.Process(float[] inputBuffer, float[] outputBuffer, int samplesRead)
    {
        for (int i = 0; i < samplesRead; i += 2)
        {
            float left = inputBuffer[i];
            float right = inputBuffer[i + 1];

            if (Mono)
            {
                // Mix left and right into mono
                float monoSample = (left + right) / 2;
                left = monoSample;
                right = monoSample;
            }
            else
            {
                // Apply width adjustment
                float mid = (left + right) / 2; // Mono (mid) component
                float side = (left - right) / 2; // Stereo (side) component

                // Adjust side component based on width
                side *= (Width + 100f) / 100f;

                left = mid + side;
                right = mid - side;
            }

            // Invert phase
            if (InvertLeft)
                left = -left;
            if (InvertRight)
                right = -right;

            // Apply panning
            float panLeft = Pan <= 0 ? 1.0f : 1.0f - Pan;
            float panRight = Pan >= 0 ? 1.0f : 1.0f + Pan;

            // Apply volume and panning
            outputBuffer[i] = left * _volume * panLeft;
            outputBuffer[i + 1] = right * _volume * panRight;
        }
    }

    public void Dispose()
    {

    }

    void IPlugin.ReceiveMidiEvent(MidiEvent midiEvent)
    {

    }

    public void OpenPluginWindow()
    {

    }

    public void ClosePluginWindow()
    {

    }
}
