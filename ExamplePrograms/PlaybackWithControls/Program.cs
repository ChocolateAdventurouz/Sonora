using Aura;
using Aura.Clips;
using Aura.Tracks;

public class Program
{
    private const string _menu = "\n\nA: start clip playback\nS: stop clip playback\nV: change clip volume\n" +
        "P: change clip pan\nT: change clip pitch\nM: mute/unmute audio track\nEsc: exit\n";

    private static void Main()
    {
        AuraMain.Init(); // Initialize the framework

        // Create an audio device using the WASAPI API in exclusive mode
        AuraMain.CreateWasapiDevice(true);

        // Create an audio clip from an audio file
        var audioClip = new AudioClip("sound.mp3");

        // Create an audio track with a name
        var audioTrack = new AudioTrack("MyTrackName");

        // Add the created audio clip to the audio track
        audioTrack.AddClip(audioClip);

        // Start the clip playback
        audioClip.Play();

        bool exit = false;
        while (!exit)
        {
            string status = $"Playing: {audioClip.Playing} | Volume: {audioClip.Volume}dB | Pan: {audioClip.Pan} " +
                    $"| Pitch: {audioClip.Pitch} | Muted: {audioTrack.Muted}";

            Console.WriteLine(status);
            Console.WriteLine(_menu);

            var option = Console.ReadKey(true);
            switch (option.Key) 
            { 
                case ConsoleKey.V: // Change clip volume
                    Console.WriteLine("Input a value between -90 and 6");
                    if (float.TryParse(Console.ReadLine(), out var vol))
                    {
                        audioClip.Volume = vol;
                        Console.WriteLine($"Clip volume set to {vol}");
                    }
                    else
                        Console.WriteLine("Invalid value");
                    break;
                case ConsoleKey.P: // Change clip pan
                    Console.WriteLine("Input a value between -50 and 50");
                    if (float.TryParse(Console.ReadLine(), out var pan))
                    {
                        audioClip.Pan = pan;
                        Console.WriteLine($"Clip pan set to {pan}");
                    }
                    else
                        Console.WriteLine("Invalid value");
                    break;
                case ConsoleKey.T: // Change clip pitch
                    Console.WriteLine("Input a value in semitones");
                    if (float.TryParse(Console.ReadLine(), out var pitch))
                    {
                        audioClip.Pitch = pitch;
                        Console.WriteLine($"Clip pitch set to {pitch} semitones");
                    }
                    else
                        Console.WriteLine("Invalid value");
                    break;
                case ConsoleKey.A: // Start clip playback
                    audioClip.Play();
                    Console.WriteLine("Clip playback started.");
                    break;
                case ConsoleKey.S: // Stop clip playback
                    audioClip.Stop();
                    Console.WriteLine("Clip playback stopped.");
                    break;
                case ConsoleKey.M: // Mute or unmute the audio track
                    audioTrack.Muted = !audioTrack.Muted;
                    string res = audioTrack.Muted ? "is" : "is not";
                    Console.WriteLine($"Audio track {res} muted.");
                    break;
                case ConsoleKey.Escape: // Exit the program
                    exit = true;
                    break;
            }
        }
    }
}