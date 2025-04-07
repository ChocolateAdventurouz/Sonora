using Jacobi.Vst.Core;
using Jacobi.Vst.Host.Interop;
using Melanchall.DryWetMidi.Core;
using Veldrid;
using Veldrid.Sdl2;
using static Sonora.Utils.Extensions;

namespace Sonora.Plugins.VST;

public class VstPlugin : IPlugin
{
    #region Interface Properties

    /// <inheritdoc/>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Name of the VST plugin. (coming from dll name)
    /// </summary>
    public string PluginName { get; set; }

    /// <inheritdoc/>
    public string PluginId { get; private set; } = Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public PluginType PluginType { get; private set; }

    #endregion

    #region Internal Properties

    /// <summary>
    /// Process audio buffer through the VST plugin.
    /// </summary>
    internal VstAudioProcessor VstProcessor { get; private set; }

    /// <summary>
    /// Handle VST midi events.
    /// </summary>
    internal VstMidiHandler MidiHandler { get; private set; }

    /// <summary>
    /// Allow to send midi events to the VST from computer keyboard.
    /// </summary>
    internal VstVirtualKeyboard VirtualKeyboard { get; private set; }

    /// <summary>
    /// Plugin context.
    /// </summary>
    internal VstPluginContext PluginContext { get; private set; }

    /// <summary>
    /// Plugin window.
    /// </summary>
    internal Sdl2Window PluginWindow { get; private set; }

    #endregion

    /// <summary>
    /// Flags to control VST behaviours.
    /// </summary>
    public VstFlags VstFlags { get; private set; }

    /// <summary>
    /// Load and open a new VST plugin instance.
    /// </summary>
    /// <param name="pluginPath">Path of the plugin .dll file.</param>
    /// <param name="flags">Optional flags to control plugin window behaviours.</param>
    public VstPlugin(string pluginPath, VstFlags flags = VstFlags.None)
    {
        VstFlags = flags;
        VirtualKeyboard = new VstVirtualKeyboard(this);
        PluginContext = LoadPlugin(pluginPath);
        PluginName = Path.GetFileNameWithoutExtension(pluginPath);
        PluginType = PluginContext.PluginInfo.Flags.HasFlag(VstPluginFlags.IsSynth) ? PluginType.Instrument : PluginType.Effect;
        VstProcessor = new VstAudioProcessor(this);
        MidiHandler = new VstMidiHandler(this);
    }

    private void HostCmdStub_PluginCalled(object sender, PluginCalledEventArgs e)
    {
        var hostCmdStub = (HostCommandStub)sender;

        // can be null when called from inside the plugin main entry point.
        if (hostCmdStub.PluginContext.PluginInfo != null)
        {
            Console.WriteLine("Plugin " + hostCmdStub.PluginContext.PluginInfo.PluginID + " called:" + e.Message);
        }
        else
        {
            Console.WriteLine("The loading Plugin called:" + e.Message);
        }
    }

    private void HostCmdStub_SizeWindow(object sender, SizeWindowEventArgs e)
    {
        PluginWindow.Width = e.Width;
        PluginWindow.Height = e.Height;
    }

    private VstPluginContext LoadPlugin(string pluginPath)
    {
        try
        {
            var hostCmdStub = new HostCommandStub();
            hostCmdStub.PluginCalled += new EventHandler<PluginCalledEventArgs>(HostCmdStub_PluginCalled);
            hostCmdStub.SizeWindow += new EventHandler<SizeWindowEventArgs>(HostCmdStub_SizeWindow);
            var ctx = VstPluginContext.Create(pluginPath, hostCmdStub);

            // add custom data to the context
            ctx.Set("PluginPath", pluginPath);
            ctx.Set("HostCmdStub", hostCmdStub);

            // actually open the plugin itself
            ctx.PluginCommandStub.Commands.Open();

            // We check if plugin returns rect data; if it doesn't we try to populate it with by opening the editor with dummy handle
            var rect = ctx.PluginCommandStub.Commands.EditorGetRect(out var rectangle);
            System.Drawing.Rectangle pluginRect = new();
            bool rectWasFound = rect;
            if (!rectWasFound)
            {
                ctx.PluginCommandStub.Commands.EditorOpen(IntPtr.Zero); // Open dummy editor, may works for some plugins to populate the rectangle data
                rect = ctx.PluginCommandStub.Commands.EditorGetRect(out var dummyRect);
                ctx.PluginCommandStub.Commands.EditorClose(); // Destroy the dummy editor
                pluginRect = dummyRect;
            }
            else
                pluginRect = rectangle;

            // Check if the plugin has an editor
            if (rect)
            {
                // Create a host window for the editor
                string windowTitle = Path.GetFileNameWithoutExtension(pluginPath);
                IntPtr hwnd = CreateWindow(windowTitle, pluginRect.Width, pluginRect.Height);

                // Attach the editor to the window
                ctx.PluginCommandStub.Commands.EditorOpen(hwnd);

                StartEditorIdle();
                Console.WriteLine("Plugin editor opened successfully.");
            }
            else
            {
                Console.WriteLine("The plugin does not have an editor.");
            }

            return ctx;
        }
        catch (Exception e)
        {
            throw new Exception($"Could not load VST plugin: {e.Message}");
        }
    }

    /// <summary>
    /// Make the window not resizable and remove the maximize button. Optionally also remove the minimize button.
    /// </summary>
    /// <param name="windowHandle">The handle to the window.</param>
    /// <param name="removeMinimize">True to remove the minimize button.</param>
    private void SetupWindow(IntPtr windowHandle, bool removeMinimize)
    {
        int style = WinAPI.GetWindowLong(windowHandle, WinAPI.GWL_STYLE);
        if (removeMinimize)
        {
            style &= ~WinAPI.WS_MINIMIZEBOX; // Remove minimize button
        }
        style &= ~WinAPI.WS_MAXIMIZEBOX; // Remove maximize button
        style &= ~WinAPI.WS_THICKFRAME; // Make the window non-resizable (except from plugin controls)
        WinAPI.SetWindowLong(windowHandle, WinAPI.GWL_STYLE, style);
    }

    private IntPtr CreateWindow(string title, int width, int height)
    {
        PluginWindow = new Sdl2Window(title, 400, 400, width, height, SDL_WindowFlags.Resizable | SDL_WindowFlags.SkipTaskbar, false);
        PluginWindow.Closing += () =>
        {
            if (PluginWindow.Exists)
            {
                PluginContext.PluginCommandStub?.Commands.EditorClose();
            }
        };

        if (VstFlags.HasFlag(VstFlags.ComputerKeyboard))
        {
            PluginWindow.KeyDown += VirtualKeyboard.OnVstKeyPress;
            PluginWindow.KeyUp += VirtualKeyboard.OnVstKeyRelease;
        }

        var topMost = VstFlags.HasFlag(VstFlags.AlwaysOnTop) ? WinAPI.HWND_TOPMOST : 0;
        WinAPI.SetWindowPos(PluginWindow.Handle, new IntPtr(topMost), 0, 0, 0, 0, 
            WinAPI.SWP_NOMOVE | WinAPI.SWP_NOSIZE | WinAPI.SWP_NOACTIVATE | WinAPI.SWP_SHOWWINDOW);

        SetupWindow(PluginWindow.Handle, VstFlags.HasFlag(VstFlags.NoMinimize));

        return PluginWindow.Handle;
    }

    /// <summary>
    /// Call an event whenever the plugin window has focus and a key is pressed.
    /// </summary>
    /// <param name="ev">The event method which handle the key press behaviour.</param>
    public void AddKeyDownEvent(Action<KeyEvent> ev)
    {
        PluginWindow.KeyDown += ev;
    }

    /// <summary>
    /// Call an event whenever the plugin window has focus and a key is released.
    /// </summary>
    /// <param name="ev">The event method which handle the key release behaviour.</param>
    public void AddKeyUpEvent(Action<KeyEvent> ev)
    {
        PluginWindow.KeyUp += ev;
    }

    /// <summary>
    /// Remove a registered key down event.
    /// </summary>
    /// <param name="ev">The event to remove.</param>
    public void RemoveKeyDownEvent(Action<KeyEvent> ev)
    {
        PluginWindow.KeyDown -= ev;
    }

    /// <summary>
    /// Remove a registered key up event.
    /// </summary>
    /// <param name="ev">The event to remove.</param>
    public void RemoveKeyUpEvent(Action<KeyEvent> ev)
    {
        PluginWindow.KeyUp -= ev;
    }

    /// <summary>
    /// Makes the plugin ui updated accordingly to control changes
    /// </summary>
    private void StartEditorIdle()
    {
        Task.Run(async () =>
        {
            while (PluginWindow.Exists)
            {
                PluginContext?.PluginCommandStub.Commands.EditorIdle();
                PluginWindow.PumpEvents();
                await Task.Delay(16);
            }
        });
    }

    private void RecreateWindow()
    {
        // Check if the plugin has an editor
        var rect = PluginContext.PluginCommandStub.Commands.EditorGetRect(out var rectange);
        if (rect)
        {
            // Create a host window for the editor
            string windowTitle = Path.GetFileNameWithoutExtension(PluginContext.Find<string>("PluginPath"));
            IntPtr hwnd = CreateWindow(windowTitle, rectange.Width, rectange.Height);

            // Attach the editor to the window
            PluginContext.PluginCommandStub.Commands.EditorOpen(hwnd);

            StartEditorIdle();
            Console.WriteLine("Plugin editor opened successfully.");
        }
        else
        {
            Console.WriteLine("The plugin does not have an editor.");
        }
    }
    
    private void OpenWindow()
    {
        if (!PluginWindow.Exists)
        {
            int x = PluginWindow.X;
            int y = PluginWindow.Y;
            RecreateWindow();
            PluginWindow.X = x;
            PluginWindow.Y = y;
        }
    }

    private void CloseWindow()
    {
        PluginWindow?.Close();
    }

    public void DisposeVST(bool closeWindow = true)
    {
        VstProcessor.DeleteRequested = true;
        if (closeWindow)
        {
            PluginWindow?.Close();
        }
        PluginContext?.Dispose();
    }
    
    /// <inheritdoc/>
    void IPlugin.Process(float[] input, float[] output, int samplesRead)
    {
        VstProcessor.Process(input, output, samplesRead);
    }

    /// <summary>
    /// Toggle the VST plugin state.
    /// </summary>
    public void Toggle()
    {
        Enabled = !Enabled;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        DisposeVST();
    }

    /// <inheritdoc/>
    public void ReceiveMidiEvent(MidiEvent midiEvent)
    {
        MidiHandler.HandleMidiEvent(midiEvent);
    }

    /// <inheritdoc/>
    public void OpenPluginWindow()
    {
        OpenWindow();
    }

    /// <inheritdoc/>
    public void ClosePluginWindow()
    {
        CloseWindow();
    }
}
