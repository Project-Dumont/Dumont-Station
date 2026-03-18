using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;

namespace Content.Server._Orion.Mobs;

public sealed class CriticalStateFeedbackSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MobStateComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(Entity<MobStateComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.OldMobState == args.NewMobState)
            return;

        var message = args.NewMobState switch
        {
            MobState.SoftCritical => Loc.GetString("mob-state-softcrit-enter"),
            MobState.HardCritical => Loc.GetString("mob-state-hardcrit-enter"),
            MobState.Alive when args.OldMobState is MobState.SoftCritical or MobState.HardCritical => Loc.GetString("mob-state-crit-exit"),
            _ => null,
        };

        if (message != null)
            _popup.PopupEntity(message, ent, ent, PopupType.LargeCaution);
    }
}
