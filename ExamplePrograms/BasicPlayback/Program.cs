using Sonora;
using Sonora.Clips;
using Sonora.Tracks;

public class Program
{ 
    private static void Main()
    {
        SonoraMain.Init(); // Initialize the framework
        SonoraMain.CreateAudioDevice(AudioAPI.WaveOut); // Create an audio device using the WaveOut API

        // Create an audio clip from an audio file
        var audioClip = new AudioClip("sound.mp3");

        // Create an audio track with a name
        var audioTrack = new AudioTrack("MyTrackName");

        // Add the created audio clip to the audio track
        audioTrack.AddClip(audioClip);

        // Start the clip playback
        audioClip.Play();

        // Wait for a keypress before exiting the program
        Console.ReadKey();
    }
}