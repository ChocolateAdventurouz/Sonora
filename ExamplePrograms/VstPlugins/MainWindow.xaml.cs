using Aura;
using Aura.Plugins.VST;
using Aura.Tracks;
using System.Windows;

namespace VstPlugins
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AuraMain.Init(); // Initialize the framework
            AuraMain.CreateAudioDevice(AudioAPI.WASAPI); // Create an audio device using the WASAPI API
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var midiTrack = new MidiTrack(); // Create a midi track
            var vsti = new VstPlugin("vst/BassMidiVsti.dll", VstFlags.AlwaysOnTop); // Load a VST
            
            midiTrack.AddPlugin(vsti); // Add the VST to the midi track
        }
    }
}