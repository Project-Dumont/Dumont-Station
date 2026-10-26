using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Atmos.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphAcidPillarSystem : EntitySystem
{
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private XenomorphSpitterSystem _spray = default!;
    private readonly HashSet<Entity<MobStateComponent>> _targets = new();

    private bool CanTarget(EntityUid target)
    {
        if (HasComp<Content.Server._White.Xenomorphs.FaceHugger.FaceHuggerComponent>(target) || _mobs.IsDead(target) || TryComp<BuckleComponent>(target, out var buckle) && HasComp<XenomorphNestComponent>(buckle.BuckledTo)) return false;
        if (HasComp<XenomorphComponent>(target)) return TryComp<FlammableComponent>(target, out var fire) && fire.OnFire;
        return _mobs.IsAlive(target) && !HasComp<StunnedComponent>(target)
            && (!TryComp<StandingStateComponent>(target, out var standing) || standing.Standing);
    }

    public override void Update(float frameTime)
    {
        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<XenomorphAcidPillarComponent>();
        while (query.MoveNext(out var uid, out var pillar))
        {
            if (now < pillar.Next || TerminatingOrDeleted(uid)) continue;
            pillar.Next = now + pillar.CheckEvery;
            _targets.Clear();
            _lookup.GetEntitiesInRange(Transform(uid).Coordinates, pillar.Range, _targets, LookupFlags.Uncontained);
            EntityUid? closest = null;
            var distance = pillar.Range;
            foreach (var target in _targets)
            {
                if (!CanTarget(target)) continue;
                var length = System.Numerics.Vector2.Distance(_transform.GetWorldPosition(uid), _transform.GetWorldPosition(target));
                if (length >= distance || !_interaction.InRangeUnobstructed(uid, target.Owner, pillar.Range)) continue;
                closest = target;
                distance = length;
            }
            if (closest is not { } victim) continue;
            if (!_spray.CreateSpray(uid, Transform(victim).Coordinates, pillar.Range, spreadDelay: pillar.AcidSpreadDelay.TotalSeconds)) continue;
            pillar.FiringUntil = now + TimeSpan.FromSeconds(.6);
            pillar.Next = now + pillar.Cooldown;
            Dirty(uid, pillar);
        }
    }
}
