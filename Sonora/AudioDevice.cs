using Sonora.Utils;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Sonora;

/// <summary>
/// Represent an output audio device.
/// </summary>
public sealed class AudioDevice
{
    /// <summary>
    /// The audio API used by this device.
    /// </summary>
    public AudioAPI Api { get; private set; }

    /// <summary>
    /// The output device used by this audio device.
    /// </summary>
    public IWavePlayer OutputDevice { get; private set; }

    /// <summary>
    /// The name of this audio device.
    /// </summary>
    public string DeviceName { get; private set; }

    #region WaveOut constructors

    /// <summary>
    /// Create a new <see cref="AudioDevice"/> with the WaveOut audio API.
    /// </summary>
    /// <param name="waveOutDevice">WaveOut.</param>
    internal AudioDevice(WaveOutEvent waveOutDevice)
    {
        OutputDevice = waveOutDevice;
        Api = AudioAPI.WaveOut;
        DeviceName = "WaveOut default device";
    }

    /// <summary>
    /// Create a new <see cref="AudioDevice"/> with the WaveOut audio API and a desired latency.
    /// </summary>
    /// <param name="waveOutDevice">WaveOut.</param>
    /// <param name="desiredLatency">The desidered latency. Increase if hearing audio artifacts.</param>
    internal AudioDevice(WaveOutEvent waveOutDevice, int desiredLatency = 300)
    {
        waveOutDevice.DesiredLatency = desiredLatency;
        OutputDevice = waveOutDevice;
        Api = AudioAPI.WaveOut;
        DeviceName = "WaveOut default device";
    }

    #endregion

    #region DirectSoundOut constructors

    /// <summary>
    /// Create a new <see cref="AudioDevice"/> with the DirectSound audio API.
    /// </summary>
    /// <param name="directSoundDevice">DirectSoundOut</param>
    internal AudioDevice(DirectSoundOut directSoundDevice) 
    { 
        OutputDevice = directSoundDevice;
        Api = AudioAPI.DirectSound;
        DeviceName = DirectSoundOut.Devices.First().Description;
    }

    /// <summary>
    /// Create a new <see cref="AudioDevice"/> with the DirectSound audio API and a desired latency.
    /// </summary>
    /// <param name="directSoundDeviceGuid">Guid of the DirectSound device.</param>
    /// <param name="desiredLatency">Desidered latency.</param>
    internal AudioDevice(Guid directSoundDeviceGuid, int desiredLatency = 50)
    {
        OutputDevice = new DirectSoundOut(directSoundDeviceGuid, desiredLatency);
        Api = AudioAPI.DirectSound;
        DeviceName = Extensions.DirectSoundGuidToName(directSoundDeviceGuid);
    }

    #endregion

    #region WasapiOut constructors

    /// <summary>
    /// Create a new <see cref="AudioDevice"/> with the WASAPI audio API.
    /// </summary>
    /// <param name="wasapiDevice">WasapiOut.</param>
    /// Provides lower latencies but prevents other applications from using the soundcard.
    internal AudioDevice(WasapiOut wasapiDevice)
    {
        OutputDevice = wasapiDevice;
        Api = AudioAPI.WASAPI;
        DeviceName = new MMDeviceEnumerator().GetDefaultAudioEndpoint(
            DataFlow.Render, 
            Role.Console)
            .FriendlyName;
    }

    /// <summary>
    /// Create a new <see cref="AudioDevice"/> with the WASAPI audio API.
    /// </summary>
    /// <param name="wasapiDevice">MM Device.</param>
    /// <param name="exclusiveMode">Request exclusive access to the sound card. Provides lower latencies but prevents other applications from using the soundcard.
    /// </param>
    /// <param name="latency">Request a latency to be used. Depending on the mode may not have any effect.</param>
    internal AudioDevice(MMDevice wasapiDevice, bool exclusiveMode = false, int latency = 200)
    {
        OutputDevice = new WasapiOut(wasapiDevice, exclusiveMode ?
            AudioClientShareMode.Exclusive : AudioClientShareMode.Shared,
            true, latency);
        DeviceName = wasapiDevice.FriendlyName;
        Api = AudioAPI.WASAPI;
    }

    #endregion

    #region AsioOut constructors

    /// <summary>
    /// Create a new <see cref="AudioDevice"/> with the ASIO audio API.
    /// </summary>
    /// <param name="asioDevice"></param>
    internal AudioDevice(AsioOut asioDevice)
    {
        OutputDevice = asioDevice;
        DeviceName = asioDevice.DriverName;
        Api = AudioAPI.ASIO;
    }

    #endregion
}
