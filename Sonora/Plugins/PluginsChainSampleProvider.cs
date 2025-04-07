using Sonora.Plugins.VST;
using NAudio.Wave;

namespace Sonora.Plugins;

/// <summary>
/// Manage a sequence of audio processing plugins.
/// </summary>
internal class PluginChainSampleProvider : ISampleProvider
{
    private IPlugin _pluginInstrument;
    /// <summary>
    /// The Instrument of this chain if it's a midi track.
    /// </summary>
    public IPlugin PluginInstrument => _pluginInstrument;

    private List<IPlugin> _fxPlugins = new();
    /// <summary>
    /// Effects plugins chain.
    /// </summary>
    public List<IPlugin> FxPlugins => _fxPlugins;

    private readonly ISampleProvider source;
    public WaveFormat WaveFormat => source.WaveFormat;

    public PluginChainSampleProvider(ISampleProvider source)
    {
        this.source = source;
    }

    public void AddPlugin(IPlugin plugin)
    {
        if (plugin is VstPlugin vstPlugin && vstPlugin.PluginType == PluginType.Instrument)
        {
            // Dispose of the current instrument if it exists
            if (_pluginInstrument != null && _pluginInstrument is VstPlugin currentVstInstrument)
            {
                currentVstInstrument.DisposeVST(vstPlugin.PluginWindow.Handle != currentVstInstrument.PluginWindow.Handle);
            }

            _pluginInstrument = plugin;
        }
        else if (plugin.PluginType == PluginType.Instrument)
        {
            _pluginInstrument?.Dispose();
            _pluginInstrument = plugin;
        }
        else
        {
            _fxPlugins.Add(plugin);
        }
    }

    public bool RemovePlugin(IPlugin target)
    {
        if (target == null)
            return false;

        if (target == _pluginInstrument)
        {
            target.Dispose();
            _pluginInstrument = null;
        }
        else
        {
            bool removed = _fxPlugins.Remove(target);
            target.Dispose();
            return removed;
        }
        return true;
    }

    public bool SwapFxPlugins(int index1, int index2)
    {
        if (index1 < 0 || index1 >= _fxPlugins.Count ||
            index2 < 0 || index2 >= _fxPlugins.Count)
        {
            return false;
        }

        (_fxPlugins[index1], _fxPlugins[index2]) = (_fxPlugins[index2], _fxPlugins[index1]);
        return true;
    }

    private void ProcessAudio(IPlugin plugin, ref float[] buffer, int offset, int count, int samplesRead)
    {
        // Create a temporary buffer to hold the processed data
        float[] tempBuffer = new float[count];

        // Copy the current buffer data to the temporary buffer
        Array.Copy(buffer, offset, tempBuffer, 0, samplesRead);

        // Process the data through the plugin
        plugin.Process(tempBuffer, tempBuffer, samplesRead);

        // Copy the processed data back to the original buffer
        Array.Copy(tempBuffer, 0, buffer, offset, samplesRead);
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = source.Read(buffer, offset, count);

        if (_pluginInstrument != null)
        {
            // Process plugin sound only if plugin is enabled
            if (_pluginInstrument.Enabled)
            {
                ProcessAudio(_pluginInstrument, ref buffer, offset, count, samplesRead);
            }
        }

        // Apply plugins audio processing in the list order
        foreach (var plugin in _fxPlugins.ToList())
        {
            // Skip plugin audio processing if not enabled
            if (!plugin.Enabled)
                continue;

            ProcessAudio(plugin, ref buffer, offset, count, samplesRead);
        }

        return samplesRead;
    }
}
