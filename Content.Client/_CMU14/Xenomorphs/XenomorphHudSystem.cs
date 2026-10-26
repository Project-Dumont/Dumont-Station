using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared._White.Xenomorphs.Plasma.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Rounding;
using Robust.Client.Graphics;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphHudSystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlays = default!;
    [Dependency] private MobThresholdSystem _thresholds = default!;
    [Dependency] private MobStateSystem _mobs = default!;

    public override void Initialize() => _overlays.AddOverlay(new XenomorphHudOverlay());
    public override void Shutdown() => _overlays.RemoveOverlay<XenomorphHudOverlay>();

    public bool CanSeeBars(EntityUid? viewer, EntityUid target)
        => HasComp<XenomorphComponent>(viewer) && HasComp<XenomorphComponent>(target) && !_mobs.IsDead(target);

    public string? GetHealthState(EntityUid uid)
    {
        if (!HasComp<XenomorphComponent>(uid) || _mobs.IsDead(uid) || !TryComp<DamageableComponent>(uid, out var damage)) return null;
        _thresholds.TryGetThresholdForState(uid, MobState.Critical, out var critical);
        _thresholds.TryGetThresholdForState(uid, MobState.Dead, out var dead);
        if (critical is { } crit && dead is { } death && (_mobs.IsCritical(uid) || damage.TotalDamage > crit))
        {
            var level = ContentHelpers.RoundToLevels((damage.TotalDamage - crit).Double(), (death - crit).Double(), 11);
            return "xenohealth-" + (level > 0 ? (level * 10).ToString() : "1");
        }
        var maximum = critical ?? dead;
        if (maximum is not { } max || max <= 0) return null;
        return "xenohealth" + ContentHelpers.RoundToLevels((max - damage.TotalDamage).Double(), max.Double(), 11) * 10;
    }

    public string? GetResourceState(EntityUid uid)
    {
        if (TryComp<XenomorphAciderComponent>(uid, out var acider))
            return ResourceState("xenoenergy", acider.Acid, 1000);
        if (TryComp<XenomorphValkyrieComponent>(uid, out var valkyrie))
            return ResourceState("xenoenergy", valkyrie.Fury, 200);
        if (TryComp<XenomorphHedgehogComponent>(uid, out var hedge))
            return ResourceState("xenoenergy", hedge.Shards, hedge.MaxShards);
        if (TryComp<XenomorphReaperComponent>(uid, out var reaper))
            return ResourceState("xenoenergy", reaper.Flesh, reaper.MaxFlesh);
        return TryComp<PlasmaVesselComponent>(uid, out var plasma)
            ? ResourceState("plasma", plasma.Plasma.Double(), plasma.MaxPlasma.Double()) : null;
    }

    public string? GetShieldState(EntityUid uid)
    {
        var shield = TryComp<XenomorphVanguardComponent>(uid, out var vanguard) ? vanguard.Shield
            : TryComp<XenomorphPraetorianComponent>(uid, out var praetorian) ? praetorian.Shield : 0;
        if (shield <= 0) return null;
        _thresholds.TryGetThresholdForState(uid, MobState.Critical, out var maximum);
        return maximum is { } max ? ResourceState("xenoshield", shield, max.Double()) : null;
    }

    public string? GetHealingState(EntityUid uid)
        => TryComp<XenomorphHealerComponent>(uid, out var healer)
            ? ResourceState("xenoenergy", healer.Transferred, healer.MaxTransferred) : null;

    private static string? ResourceState(string prefix, double amount, double maximum)
        => maximum > 0 ? prefix + ContentHelpers.RoundToLevels(amount, maximum, 11) * 10 : null;
}
