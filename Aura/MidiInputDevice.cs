using Melanchall.DryWetMidi.Multimedia;

namespace Aura;

/// <summary>
/// Represent a midi input device.
/// </summary>
public sealed class MidiInputDevice
{
    /// <summary>
    /// The input midi device used by this midi device.
    /// </summary>
    public InputDevice? InDevice { get; private set; }

    /// <summary>
    /// The name of input device.
    /// </summary>
    public string DeviceName { get; private set; } = string.Empty;

    public MidiInputDevice(InputDevice? inDevice)
    {
        InDevice = inDevice;
        if (inDevice != null)
        {
            DeviceName = inDevice.Name;
        }
    }
}
