using Melanchall.DryWetMidi.Core;
using SoundTouch;

namespace Aura.Plugins.BuiltIn;

/// <summary>
/// Plugin which controls the pitch of the provided audio source.
/// </summary>
public class PitchShifter : IPlugin
{
    /// <inheritdoc/>
    public bool Enabled { get; set; } = true;

    /// <inheritdoc/>
    public string PluginName { get; set; } = "PitchShifter";

    /// <inheritdoc/>
    public string PluginId { get; } = Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public PluginType PluginType => PluginType.Effect;

    private readonly SoundTouchProcessor _processor;
    private const int _channels = 2;

    /// <summary>
    /// Gets or sets pitch change in semi-tones compared to the original pitch
    /// (eg. -12 .. +12).
    /// </summary>
    public double PitchSemiTones
    {
        get => _processor.PitchSemiTones;
        set => _processor.PitchSemiTones = value;
    }

    public PitchShifter()
    {
        this._processor = new SoundTouchProcessor();

        _processor.Channels = _channels;
        _processor.SampleRate = AuraMain.SampleRate;
    }

    public void ClosePluginWindow()
    {
    }

    public void Dispose()
    {
    }

    public void OpenPluginWindow()
    {
    }

    void IPlugin.Process(float[] input, float[] output, int samplesRead)
    {
        _processor.PutSamples(input, samplesRead / _channels);
        _= _processor.ReceiveSamples(output, output.Length / _channels) * _channels;      
    }

    void IPlugin.ReceiveMidiEvent(MidiEvent midiEvent)
    {
    }
}
