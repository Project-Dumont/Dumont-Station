using Robust.Shared.Configuration;
using Content.Shared._LP.CCVar;
using Content.Shared._LP.Mobs.Events;

namespace Content.Client._LP.Mobs.Systems;

public sealed partial class CritStateAudioSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _configuration = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_configuration, LPCVars.CriticalSoundEnabled, _ => RaiseNetworkEvent(new CritStateAudioEvent()));
    }
}
