using Robust.Shared.Configuration;

namespace Content.Shared._LP.CCVar;

[CVarDefs]
public static class LPCVars
{
    /// <summary>
    /// Sound effects in critical condition. Yep, that's it.
    /// </summary>
    public static readonly CVarDef<bool> CriticalSoundEnabled =
        CVarDef.Create("audio.critical_sound_enabled", true, CVar.ARCHIVE | CVar.CLIENT | CVar.REPLICATED);
}
