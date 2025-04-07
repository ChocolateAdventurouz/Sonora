namespace Sonora.EventArguments;

/// <summary>
/// Fired when <see cref="SonoraMain.MidiDevice"/> has been changed.
/// </summary>
internal class MidiDeviceChangedEventArgs : EventArgs
{
    /// <summary>
    /// The previous midi device.
    /// </summary>
    public MidiInputDevice OldMidiDevice { get; set; }

    /// <summary>
    /// The new midi device.
    /// </summary>
    public MidiInputDevice NewMidiDevice { get; set; }

    public MidiDeviceChangedEventArgs(MidiInputDevice oldMidiDevice, MidiInputDevice newMidiDevice)
    {
        OldMidiDevice = oldMidiDevice;
        NewMidiDevice = newMidiDevice;
    }
}
