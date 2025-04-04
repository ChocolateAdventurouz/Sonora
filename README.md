<p align="center">
  <img src="https://i.imgur.com/gSr1Wwh.png" width="250" height="250" />
</p>

# Aura ![GitHub License](https://img.shields.io/github/license/ImAxel0/Aura?&style=for-the-badge) [![Github All Releases](https://img.shields.io/github/v/release/ImAxel0/Aura?&style=for-the-badge)]()

### .NET Audio Framework for audio and MIDI playback, editing, and plugin integration.

---

Aura is a .NET audio framework which provides a structured workflow for handling audio/MIDI clips, tracks, and real-time processing, making it ideal for digital audio applications.

[**Documentation**](https://aura-docs-9zp.pages.dev) 📙

## Core Features :rocket:
- **Audio and Midi clips**: create Audio and Midi clips, tweak their options, place them in tracks and play them.

- **Audio and Midi tracks**: support for Audio and Midi tracks allows to group clips, apply effects to all and build audio pipelines.

- **Virtual Studio Technology (VST)**: plug and play VST2 Plugins support. Process the audio signal or generate it from external and built in plugins.

- **Automations**: automate clips parameters like volume and pan during playback.

## Highlights :star2:
- **Built on Robust Libraries**: the framework makes use of popular open source libraries like NAudio, DryWetMidi, SoundTouch.Net and VST.NET.

- **Wide Use Cases**: build whatever you think of, be it a simple audio player, a sound editor or a DAW like application.

- **Open Source**: everyone can contribute to it extending the framework capabilities or help in bug fixing and improving current features.

## Supported OS 🖥️
Aura is available on Windows devices only. There is no cross platform support as most of the used libraries have partial cross platform capabilities or not at all.

## Installation ⬇️

### Requirements
- **.NET 6.0 SDK**: the framework is built on the .NET 6 version for better compatibility with some of the used libraries, but should also works on later versions, although it has not been tested.

The easiest way to get started is to install the NuGet package with the NuGet Package Manager you can find on Visual Studio in `Tools` > `NuGet Package Manager` > `Package Manager Console`, running the command below. For others IDE use the relative package manager.

```bash
Install-Package Aura.NET
```

If you are gonna use VST plugins in your app, you will also have to install the `VST.NET2-Host` package.

```bash
Install-Package VST.NET2-Host
```

After installing the needed packages, make your project target the `x64` platform.

> Target platform `x64` is required because the framework was built on that target to support VST plugins. For reference, read the first lines [here](https://obiwanjacobi.github.io/vst.net/GettingStarted.html).

## Getting started ✍️
Here is a basic example that shows how to play an audio file by creating an audio clip from it and adding it to an audio track.
```cs
private static void Main()
{
    AuraMain.Init(); // Initialize the framework

    // Create an audio device using the WaveOut API
    AuraMain.CreateAudioDevice(AudioAPI.WaveOut);

    // Create an audio clip from an audio file
    var audioClip = new AudioClip("path/to/sound.wav");

    // Create an audio track with a name
    var audioTrack = new AudioTrack("MyTrackName");

    // Add the created audio clip to the audio track
    audioTrack.AddClip(audioClip);

    // Start the clip playback
    audioClip.Play();

    // Wait for a keypress before exiting the program
    Console.ReadKey();
}
```

## License 🔑
This project is MIT-licensed, but uses the following LGPL-2.1 libraries:  
- [VST.NET](https://github.com/obiwanjacobi/vst.net)  
- [SoundTouch.Net](https://github.com/owoudenberg/soundtouch.net)  

Users must comply with the LGPL-2.1 terms when using these dependencies.  

## Used Libraries 📚
This project makes use of the following libraries. A special thank to those:
- [NAudio](https://github.com/naudio/NAudio), for the audio integration.
- [DryWetMidi](https://github.com/melanchall/drywetmidi), for the midi integration.
- [VST.NET](https://github.com/obiwanjacobi/vst.net), for the VST plugins integration.
- [SoundTouch.Net](https://github.com/owoudenberg/soundtouch.net), for the pitch and tempo sound effects.
- [Veldrid](https://github.com/veldrid/veldrid), for the creation and management of VST plugin windows.
