using Aura;
using Aura.Clips;
using Aura.Tracks;
using Aura.Automations;

public class Program
{
    private static void Main()
    {
        AuraMain.Init(); // Initialize the framework
        AuraMain.CreateAudioDevice(AudioAPI.WaveOut); // Create an audio device using the WaveOut API

        // Create an audio clip from an audio file
        var audioClip = new AudioClip("sound.mp3");

        // Create an audio track
        var audioTrack = new AudioTrack();

        // Add the created audio clip to the audio track
        audioTrack.AddClip(audioClip);
        
        // Automate volume from -12 to 0 during the first six seconds
        audioClip.AddAutomationPoint(AutomationParameter.Volume, 0, -12);
        audioClip.AddAutomationPoint(AutomationParameter.Volume, 6, 0);

        // Automate pan from left to right during the first six seconds then move to the center, using a smoothed out interpolation
        audioClip.AddAutomationPoint(AutomationParameter.Pan, 0, -50, InterpolationType.Smooth);
        audioClip.AddAutomationPoint(AutomationParameter.Pan, 6, 50, InterpolationType.Smooth);
        audioClip.AddAutomationPoint(AutomationParameter.Pan, audioClip.Duration, 0);

        // Automate pitch between ±1 semitones for the entire duration of the playback
        for (double t = 0; t < audioClip.Duration; t += 0.2)
        {
            float pitch = (float)Math.Sin(t * 2) * 1; // ±1 semitones
            audioClip.AddAutomationPoint(AutomationParameter.Pitch, t, pitch);
        }
        
        // Start the clip playback
        audioClip.Play();

        while (audioClip.Playing)
        {
            Console.WriteLine($"Volume: {audioClip.Volume:n2} | Pan: {audioClip.Pan:n1} | Pitch: {audioClip.Pitch:n2}");
        }

        // Wait for a keypress before exiting the program
        Console.ReadKey();
    }
}