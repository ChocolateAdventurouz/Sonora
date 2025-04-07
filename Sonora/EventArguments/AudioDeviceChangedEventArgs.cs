namespace Sonora.EventArguments;

/// <summary>
/// Fired when <see cref="SonoraMain.Device"/> has been changed.
/// </summary>
internal class AudioDeviceChangedEventArgs : EventArgs
{
    /// <summary>
    /// The previous audio device.
    /// </summary>
    public AudioDevice OldAudioDevice { get; set; }
    
    /// <summary>
    /// The new audio device.
    /// </summary>
    public AudioDevice NewAudioDevice { get; set; }

    public AudioDeviceChangedEventArgs(AudioDevice oldAudioDevice, AudioDevice newAudioDevice)
    {
        OldAudioDevice = oldAudioDevice;
        NewAudioDevice = newAudioDevice;
    }
}
