using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Buckle.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Content.Shared.Interaction;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphRestHealingSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    private readonly HashSet<Entity<XenomorphNestComponent>> _nests = new();
    private TimeSpan _nextHeal;

    public override void Update(float frameTime)
    {
        if (_timing.CurTime < _nextHeal)
            return;

        _nextHeal = _timing.CurTime + TimeSpan.FromSeconds(1);
        var query = EntityQueryEnumerator<XenomorphRestComponent, XenomorphComponent>();
        while (query.MoveNext(out var uid, out var rest, out var xeno))
        {
            if (!rest.Resting || !_mobState.IsAlive(uid) || xeno.WeedHeal == null)
                continue;

            var nested = TryComp<BuckleComponent>(uid, out var buckle)
                && HasComp<XenomorphNestComponent>(buckle.BuckledTo);
            if (xeno.OnWeed || nested || OnNest(uid))
                _damage.TryChangeDamage(uid, xeno.WeedHeal);
        }
    }

    private bool OnNest(EntityUid uid)
    {
        _nests.Clear();
        _lookup.GetEntitiesInRange(Transform(uid).Coordinates, 0.6f, _nests);
        foreach (var nest in _nests)
        {
            if (Transform(uid).GridUid == Transform(nest).GridUid
                && _interaction.InRangeUnobstructed(uid, nest.Owner, range: 0.6f))
                return true;
        }
        return false;
    }

}
