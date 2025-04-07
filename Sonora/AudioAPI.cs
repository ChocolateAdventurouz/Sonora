namespace Sonora;

/// <summary>
/// Audio APIs
/// </summary>
public enum AudioAPI
{
    /// <summary>
    /// WaveOut.
    /// </summary>
    WaveOut,

    /// <summary>
    /// DirectSound .
    /// </summary>
    DirectSound,

    /// <summary>
    /// Supported OS: <see langword=">="/> Windows Vista
    /// </summary>
    WASAPI,

    /// <summary>
    /// Standard for audio interface drivers. 
    /// <br/> Provides the lowest latencies.
    /// </summary>
    ASIO,
}
