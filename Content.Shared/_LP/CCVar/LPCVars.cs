using Robust.Shared.Configuration;

namespace Content.Shared._LP.CCVar;

public static class LPCVars
{
    public static readonly CVarDef<bool> CriticalSoundEnabled =
        CVarDef.Create("audio.critical_sound_enabled", true, CVar.ARCHIVE | CVar.CLIENT | CVar.REPLICATED);
}
