using Content.Goobstation.Common.Carrying;
using Content.Goobstation.Common.Cuffs;
using Content.Goobstation.Common.Weapons;
using Content.Shared.Alert;
using Content.Shared.Gravity;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModWearerEffectsSystem : EntitySystem
{
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private SharedGravitySystem _gravity = default!;

    [SubscribeLocalEvent]
    private void OnCarryDuration(Entity<ModQuickCarryComponent> ent, ref ModifyCarryDurationEvent args)
    {
        args.Multiplier *= ent.Comp.Multiplier;
    }

    [SubscribeLocalEvent]
    private void OnCuffDuration(Entity<ModQuickCuffComponent> ent, ref ModifyCuffDurationEvent args)
    {
        args.Duration *= ent.Comp.Multiplier;
    }

    [SubscribeLocalEvent]
    private void OnShoveAttempt(Entity<ModShoveBlockerComponent> ent, ref ShoveAttemptEvent args)
    {
        args.Cancelled = true;
    }

    [SubscribeLocalEvent]
    private void OnAntiGravityStartup(Entity<ModAntiGravityComponent> ent, ref ComponentStartup args)
    {
        RefreshWeightlessAlert(ent);
    }

    [SubscribeLocalEvent]
    private void OnAntiGravityShutdown(Entity<ModAntiGravityComponent> ent, ref ComponentShutdown args)
    {
        if (!TerminatingOrDeleted(ent))
            RefreshWeightlessAlert(ent);
    }

    private void RefreshWeightlessAlert(EntityUid uid)
    {
        if (_gravity.IsWeightless(uid))
            _alerts.ShowAlert(uid, SharedGravitySystem.WeightlessAlert);
        else
            _alerts.ClearAlert(uid, SharedGravitySystem.WeightlessAlert);
    }

    [SubscribeLocalEvent]
    private void OnIsWeightless(Entity<ModAntiGravityComponent> ent, ref IsWeightlessEvent args)
    {
        if (ent.Comp.LifeStage > ComponentLifeStage.Running)
            return;

        args.IsWeightless = true;
        args.Handled = true;
    }
}
