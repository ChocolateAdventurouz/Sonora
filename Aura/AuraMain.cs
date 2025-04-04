using Aura.EventArguments;
using Aura.Utils;
using Melanchall.DryWetMidi.Multimedia;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Aura;

/// <summary>
/// Core class of the Aura framework.
/// </summary>
public static class AuraMain
{
    private static bool _initialized;

    /// <summary>
    /// Sample Rate used by the Aura framework
    /// </summary>
    public static int SampleRate { get; private set; } = 44100;

    /// <summary>
    /// The <see cref="AudioDevice"/> used by the Aura framework.
    /// </summary>
    private static AudioDevice _device;

    /// <inheritdoc cref="_device"/>
    public static AudioDevice Device
    {
        get => _device;
        private set
        {
            var old = _device;
            _device = value;
            AudioDeviceChanged?.Invoke(null, new AudioDeviceChangedEventArgs(old, value));
        }
    }

    /// <summary>
    /// The <see cref="global::Aura.MidiInputDevice"/> used by the Aura framework.
    /// </summary>
    private static MidiInputDevice _midiDevice;

    /// <inheritdoc cref="_midiDevice"/>
    public static MidiInputDevice MidiDevice
    {
        get => _midiDevice;
        private set
        {
            var old = _midiDevice;
            _midiDevice = value;
            MidiDeviceChanged?.Invoke(null, new MidiDeviceChangedEventArgs(old, value));
        }
    }

    /// <summary>
    /// Event fired when the Aura framework <see cref="AudioDevice"/> is changed.
    /// </summary>
    internal static event EventHandler<AudioDeviceChangedEventArgs> AudioDeviceChanged;

    /// <summary>
    /// Event fired when the Aura framework <see cref="global::Aura.MidiInputDevice"/> is changed.
    /// </summary>
    internal static event EventHandler<MidiDeviceChangedEventArgs> MidiDeviceChanged;

    /// <summary>
    /// Initialize the Aura framework.
    /// Must be called only one time before doing any other operation.
    /// </summary>
    /// <param name="sampleRate">Sample Rate to use.</param>
    public static void Init(int sampleRate = 44100)
    {
        if (_initialized) 
            return;

        SampleRate = sampleRate;
        AudioDeviceChanged += (obj, e) => { 
            e.OldAudioDevice?.OutputDevice?.Dispose();
            Master.Init(SampleRate);
        };
        MidiDeviceChanged += (obj, e) =>
        {
            e.OldMidiDevice?.InDevice?.Dispose();
            e.NewMidiDevice?.InDevice?.StartEventsListening();
            if (e.NewMidiDevice?.InDevice != null)
            {
                e.NewMidiDevice.InDevice.EventReceived += (sender, e) =>
                {
                    foreach (var track in Master.Tracks)
                    {
                        if (track.ReceiveMidiInput)
                        {
                            track.PluginInstrument?.ReceiveMidiEvent(e.Event);
                        }
                    }
                };
            }
        };

        _initialized = true;
    }

    #region Audio devices creation

    /// <summary>
    /// Create a new <see cref="AudioDevice"/> using the specified audio API and the selected Windows audio device using default settings. Keep in mind that if choosing the ASIO API it will select the first found device which may not be the one you want.
    /// </summary>
    /// <param name="api">The audio API to be used</param>
    /// <exception cref="NotImplementedException"></exception>
    public static void CreateAudioDevice(AudioAPI api)
    {
        switch (api)
        {
            case AudioAPI.WaveOut:
                Device = new AudioDevice(new WaveOutEvent());
                break;
            case AudioAPI.DirectSound:
                Device = new AudioDevice(new DirectSoundOut());
                break;
            case AudioAPI.WASAPI:
                Device = new AudioDevice(new WasapiOut());
                break;
            case AudioAPI.ASIO:
                Device = new AudioDevice(new AsioOut());
                break;
        }
    }

    /// <summary>
    /// Create a new WaveOut <see cref="AudioDevice"/> using the default Windows audio device and optional desired latency.
    /// </summary>
    /// <param name="desiredLatency">Desired latency. Increase if hearing audio artifacts.</param>
    public static void CreateWaveOutDevice(int desiredLatency = 300)
    {
        Device = new AudioDevice(new WaveOutEvent(), desiredLatency);
    }

    /// <summary>
    /// Create a new DirectSound <see cref="AudioDevice"/> using the selected Windows audio device and optional desired latency.
    /// </summary>
    /// <param name="desiredLatency">Desired latency. Increase if hearing audio artifacts.</param>
    public static void CreateDirectSoundDevice(int desiredLatency = 50)
    {
        Device = new AudioDevice(new DirectSoundOut(desiredLatency));
    }

    /// <summary>
    /// Create a new DirectSound <see cref="AudioDevice"/> by its name and optional desired latency.
    /// </summary>
    /// <param name="deviceName">The DirectSound device name.
    /// <para/> Use <see cref="Extensions.GetDirectSoundNames"/> to get the available devices name.</param>
    /// <param name="desiredLatency">Desired latency. Increase if hearing audio artifacts.</param>
    public static void CreateDirectSoundDevice(string deviceName, int desiredLatency = 50)
    {
        Device = new AudioDevice(Extensions.DirectSoundNameToGuid(deviceName), desiredLatency);
    }

    /// <summary>
    /// Create a new WASAPI <see cref="AudioDevice"/> using the selected Windows audio device, mode and optional desired latency.
    /// </summary>
    /// <param name="exlusiveMode">Request exclusive access to the sound card.
    /// Provides lower latencies but prevents other applications from using the soundcard.</param>
    /// <param name="desiredLatency">Request a latency to be used. Depending on the mode may not have any effect.</param>
    public static void CreateWasapiDevice(bool exlusiveMode = false, int desiredLatency = 200)
    {
        Device = new AudioDevice(new WasapiOut(exlusiveMode ? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared, desiredLatency));
    }

    /// <summary>
    /// Create a new WASAPI <see cref="AudioDevice"/> by the specified device name, mode and optional desired latency.
    /// </summary>
    /// <param name="deviceName">The WASAPI device name.
    /// <para/> Use <see cref="Extensions.GetWasapiNames"/> to get the available devices name.</param>
    /// <param name="exclusiveMode">Request exclusive access to the sound card.
    /// Provides lower latencies but prevents other applications from using the soundcard.</param>
    /// <param name="desiredLatency">Request a latency to be used. Depending on the mode may not have any effect.</param>
    /// <exception cref="Exception">"Device not found.</exception>
    public static void CreateWasapiDevice(string deviceName, bool exclusiveMode = false, int desiredLatency = 200)
    {
        var mmDevices = Extensions.GetMMDevices();
        foreach (var mmDevice in mmDevices)
        {
            if (mmDevice.FriendlyName == deviceName)
            {
                Device = new AudioDevice(mmDevice, exclusiveMode, desiredLatency);
                return;
            }
        }
        throw new Exception($"{deviceName} WASAPI device not found.");
    }

    /// <summary>
    /// Create a new ASIO <see cref="AudioDevice"/> by the specified device name.
    /// </summary>
    /// <param name="deviceName">The ASIO device name.
    /// <para/> 
    /// Use <see cref="Extensions.GetAsioNames"/> to get the available devices name.
    /// </param>
    /// <exception cref="NotSupportedException">ASIO not supported.</exception>
    /// <exception cref="Exception">Device not found.</exception>
    public static void CreateAsioDevice(string deviceName)
    {
        if (!AsioOut.isSupported())
        {
            throw new NotSupportedException("ASIO isn't supported on this system.");
        }
        if (!AsioOut.GetDriverNames().Contains(deviceName))
        {
            throw new Exception($"{deviceName} ASIO device not found.");
        }
        Device = new AudioDevice(new AsioOut(deviceName));
    }

    #endregion

    /// <summary>
    /// Shows the ASIO control panel if the device uses the ASIO audio API.
    /// </summary>
    public static void AsioSettingsPanel()
    {
        if (Device == null || Device.OutputDevice == null)
            return;

        if (Device.OutputDevice is AsioOut asio)
        {
            asio.ShowControlPanel();
        }
    }

    /// <summary>
    /// Create a new <see cref="MidiInputDevice"/> by the specified input name.
    /// </summary>
    /// <param name="inputName">The input midi device name.
    /// <para/> Use <see cref="Extensions.GetMidiInputsName"/> to get all available devices name.
    /// </param>
    public static void CreateMidiInputDevice(string inputName)
    {
        MidiDevice = new MidiInputDevice(InputDevice.GetByName(inputName));
    }
}
