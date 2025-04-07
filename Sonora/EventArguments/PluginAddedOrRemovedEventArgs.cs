using Sonora.Plugins;

namespace Sonora.EventArguments;

public class PluginAddedOrRemovedEventArgs : EventArgs
{
    /// <summary>
    /// The plugin added or removed from the track.
    /// </summary>
    public IPlugin Plugin { get; }

    public PluginAddedOrRemovedEventArgs(IPlugin plugin)
    {
        Plugin = plugin;
    }
}
