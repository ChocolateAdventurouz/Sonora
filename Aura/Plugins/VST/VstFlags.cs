namespace Aura.Plugins.VST;

/// <summary>
/// Flags to control VST behaviours.
/// </summary>
public enum VstFlags
{
    None,

    /// <summary>
    /// Make the window always stay on top.
    /// </summary>
    AlwaysOnTop,

    /// <summary>
    /// Remove the minimize button.
    /// </summary>
    NoMinimize,

    /// <summary>
    /// Allow to play VST instruments from computer keyboard.
    /// <para/>
    /// Change octave with [z]/[x] and velocity with [c]/[v].
    /// </summary>
    ComputerKeyboard,
}
