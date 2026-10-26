using Content.Server._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphResinFloorComponent : Component;

[RegisterComponent]
public sealed partial class XenomorphResinSpikesComponent : Component
{
    public readonly Dictionary<EntityUid, TimeSpan> Hits = new();
}

public sealed partial class XenomorphResinFloorSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphResinSpikesComponent, StartCollideEvent>(OnCollide);
    }
    private void OnCollide(Entity<XenomorphResinSpikesComponent> ent, ref StartCollideEvent args)
    {
        var target = args.OtherEntity;
        if (!HasComp<MobStateComponent>(target) || HasComp<XenomorphComponent>(target)
            || ent.Comp.Hits.TryGetValue(target, out var until) && until > _timing.CurTime) return;
        ent.Comp.Hits[target] = _timing.CurTime + TimeSpan.FromSeconds(1);
        _damage.TryChangeDamage(target, new DamageSpecifier { DamageDict = { ["Piercing"] = 8 }, ArmorPenetration = .5f }, origin: ent);
    }
}
