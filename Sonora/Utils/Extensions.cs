using Melanchall.DryWetMidi.Multimedia;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Runtime.InteropServices;

namespace Sonora.Utils;

/// <summary>
/// Collection of helper methods.
/// </summary>
public static class Extensions
{
    #region Audio Devices Helpers

    /// <summary>
    /// Find all available DirectSound devices name.
    /// </summary>
    /// <returns>Name of available DirectSound devices.</returns>
    public static string[] GetDirectSoundNames()
    {
        List<string> names = new();
        foreach (var device in DirectSoundOut.Devices)
        {
            names.Add(device.Description);
        }
        return names.ToArray();
    }

    /// <summary>
    /// Get the DirectSound device guid from its name.
    /// </summary>
    /// <param name="name">Name of the device.</param>
    /// <returns>The guid of the DirectSound device.</returns>
    /// <exception cref="ArgumentException"></exception>
    internal static Guid DirectSoundNameToGuid(string name)
    {
        var device = DirectSoundOut.Devices.FirstOrDefault(d => d.Description == name);
        if (device == null)
        {
            throw new ArgumentException($"{name} DirectSound device guid not found.");
        }
        return device.Guid;
    }

    /// <summary>
    /// Get the DirectSound device name from its guid.
    /// </summary>
    /// <param name="guid">Guid of the device.</param>
    /// <returns>The name of the DirectSound device.</returns>
    /// <exception cref="ArgumentException"></exception>
    internal static string DirectSoundGuidToName(Guid guid)
    {
        var device = DirectSoundOut.Devices.FirstOrDefault(d => d.Guid == guid);
        if (device == null)
        {
            throw new ArgumentException($"DirectSound device with guid {guid} not found.");
        }
        return device.Description;
    }

    /// <summary>
    /// Find all installed ASIO driver names.
    /// </summary>
    /// <returns>Names of installed ASIO drivers.</returns>
    public static string[] GetAsioNames()
    {
        return AsioOut.GetDriverNames();
    }

    /// <summary>
    /// Find all active <see cref="MMDevice"/>'s.
    /// </summary>
    /// <returns>Found <see cref="MMDevice"/>'s.</returns>
    internal static MMDeviceCollection GetMMDevices()
    {
        // Find MM devices
        var deviceEnumerator = new MMDeviceEnumerator();
        var devices = deviceEnumerator.EnumerateAudioEndPoints(
            DataFlow.Render,
            DeviceState.Active);

        return devices;
    }

    /// <summary>
    /// Find all available WASAPI device names. 
    /// <br/> DON'T CALL EVERY FRAME!
    /// </summary>
    /// <returns>Name of found WASAPI devices.</returns>
    public static string[] GetWasapiNames()
    {
        var devices = GetMMDevices();
        var names = new List<string>();
        foreach (var device in devices)
        {
            names.Add(device.FriendlyName);
        }
        return names.ToArray();
    }

    #endregion

    #region Midi Devices Helpers

    /// <summary>
    /// Get all available input midi devices name.
    /// </summary>
    /// <returns>Input midi devices name.</returns>
    public static string[] GetMidiInputsName()
    {
        List<string> names = new();
        foreach (var device in InputDevice.GetAll())
        {
            names.Add(device.Name);
        }
        return names.ToArray();
    }

    /// <summary>
    /// Get all available output midi devices name.
    /// </summary>
    /// <returns>Output midi devices name.</returns>
    public static string[] GetMidiOutputsName()
    {
        List<string> names = new();
        foreach (var device in OutputDevice.GetAll())
        {
            names.Add(device.Name);
        }
        return names.ToArray();
    }

    #endregion

    /// <summary>
    /// Convert volume from range [-90, 6] to range [0, ~1].
    /// </summary>
    /// <param name="volume">Volume</param>
    /// <returns></returns>
    public static float ToLinearVolume(float volume)
    {
        return (float)Math.Pow(10, volume / 20);
    }

    /// <summary>
    /// Windows P/Invoke.
    /// </summary>
    internal class WinAPI
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public const int HWND_TOPMOST = -1;
        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_SHOWWINDOW = 0x0040;

        public const int GWL_STYLE = -16;
        public const int WS_MINIMIZEBOX = 0x00020000;
        public const int WS_MAXIMIZEBOX = 0x00010000;
        public const int WS_THICKFRAME = 0x00040000;
    }
}
