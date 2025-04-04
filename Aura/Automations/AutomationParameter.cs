namespace Aura.Automations;

/// <summary>
/// Parameters that can be automated.
/// </summary>
public enum AutomationParameter
{
    /// <summary>
    /// Audio clips. (-90, 6)
    /// <br/>
    /// Midi clips. (0, 127)
    /// </summary>
    Volume,

    /// <summary>
    /// Audio clips. (-50, 50)
    /// <br/>
    /// Midi clips. (0, 127)
    /// </summary>
    Pan,

    /// <summary>
    /// Balance for midi clips. (0, 127)
    /// </summary>
    Balance,

    /// <summary>
    /// Pitch in semitones for audio clips. (inf, inf)
    /// </summary>
    Pitch,

    /// <summary>
    /// Sustain pedal for midi clips. (>= 64 is ON)
    /// </summary>
    SustainPedal,

    /// <summary>
    /// Portamento for midi clips. (ON/OFF)
    /// </summary>
    Portamento,

    /// <summary>
    /// Sostenuto pedal for midi clips. (ON/OFF)
    /// </summary>
    SostenutoPedal,

    /// <summary>
    /// Soft pedal for midi clips. (ON/OFF)
    /// </summary>
    SoftPedal,

    /// <summary>
    /// Legato pedal for midi clips. (ON/OFF)
    /// </summary>
    LegatoPedal,

    /// <summary>
    /// Modulation for midi clips. (0, 127)
    /// </summary>
    Modulation
}
