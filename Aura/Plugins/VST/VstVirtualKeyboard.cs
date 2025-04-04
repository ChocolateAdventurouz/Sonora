using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Common;
using Veldrid;

namespace Aura.Plugins.VST;

/// <inheritdoc cref="VstPlugin.VirtualKeyboard"/>
internal class VstVirtualKeyboard
{
    private VstPlugin _vst;
    private static int _octaveShift = 0;
    private static int _velocity = 100;

    public VstVirtualKeyboard(VstPlugin vstPlugin)
    {
        _vst = vstPlugin;
    }

    private readonly Dictionary<Key, int> _keyNoteMap = new()
    {
        { Key.A, 60 }, // C4
        { Key.W, 61 }, // C#4
        { Key.S, 62 }, // D4
        { Key.E, 63 }, // D#4
        { Key.D, 64 }, // E4
        { Key.F, 65 }, // F4
        { Key.T, 66 }, // F#4
        { Key.G, 67 }, // G4
        { Key.Y, 68 }, // G#4
        { Key.H, 69 }, // A4
        { Key.U, 70 }, // A#4
        { Key.J, 71 }, // B4
        { Key.K, 72 }, // C5
    };

    public void OnVstKeyPress(KeyEvent ev)
    {
        if (ev.Repeat)
            return;

        if (_keyNoteMap.ContainsKey(ev.Key))
        {
            _vst.MidiHandler.HandleMidiEvent(new NoteOnEvent((SevenBitNumber)(_keyNoteMap[ev.Key] + _octaveShift), (SevenBitNumber)_velocity));
        }

        if (ev.Key == Key.Z)
        {
            ShiftOctave(-12);
        }

        if (ev.Key == Key.X)
        {
            ShiftOctave(+12);
        }

        if (ev.Key == Key.C)
        {
            ShiftVelocity(-10);
        }

        if (ev.Key == Key.V)
        {
            ShiftVelocity(+10);
        }
    }

    public void OnVstKeyRelease(KeyEvent ev)
    {
        if (ev.Repeat)
            return;

        if (_keyNoteMap.ContainsKey(ev.Key))
        {
            _vst.MidiHandler.HandleMidiEvent(new NoteOffEvent((SevenBitNumber)(_keyNoteMap[ev.Key] + _octaveShift), (SevenBitNumber)0));
        }
    }

    private void ShiftOctave(int amount)
    {
        _vst.MidiHandler.SendAllNotesOff(0);

        _octaveShift += amount;
        _octaveShift = Math.Clamp(_octaveShift, -36, 36);
    }

    private void ShiftVelocity(int amount)
    {
        _velocity += amount;
        _velocity = Math.Clamp(_velocity, 7, 127);
    }
}
