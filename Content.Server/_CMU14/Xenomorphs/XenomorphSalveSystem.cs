using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Atmos.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;
using System.Numerics;
using Robust.Shared.Map;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Content.Shared.ActionBlocker;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphSalveSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototypes = default!;
    private static readonly ProtoId<DamageGroupPrototype>[] HealGroups = ["Brute", "Burn"];
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private IGameTiming _timing = default!;

    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphComponent, XenomorphSalveActionEvent>(OnSalve);
    }

    private void OnSalve(Entity<XenomorphComponent> ent, ref XenomorphSalveActionEvent args)
    {
        if (args.Handled || args.Target == ent.Owner || !_mobState.IsAlive(ent) || !_blocker.CanConsciouslyPerformAction(ent)
            || TryComp<XenomorphRestComponent>(ent, out var rest) && rest.Resting
            || !HasComp<XenomorphLineageComponent>(args.Target)
            || !TryComp<XenomorphComponent>(args.Target, out var target)
            || _mobState.IsDead(args.Target) || IsOnFire(args.Target)
            || !TryComp<DamageableComponent>(args.Target, out var damage) || damage.TotalDamage <= 0
            || !_interaction.InRangeUnobstructed(ent.Owner, args.Target, range: 2f))
            return;

        FixedPoint2 amount = target.Caste == "Larva" ? 15 : 100;
        var sameStrain = MetaData(ent).EntityPrototype == MetaData(args.Target).EntityPrototype;
        var selfDamage = target.Caste == "Larva" || sameStrain ? amount : amount * 0.75;
        if (!_plasma.ChangePlasmaAmount(ent, -amount * 2))
            return;

        _damage.TryChangeDamage(ent, new DamageSpecifier { DamageDict = { ["Blunt"] = selfDamage } },
            ignoreResistances: true, origin: ent);
        var salve = EnsureComp<XenomorphSalveComponent>(args.Target);
        salve.Stacks.Add(new XenomorphSalveStack
        {
            Amount = amount / 5,
            NextHeal = _timing.CurTime + TimeSpan.FromSeconds(1),
        });
        Spawn("DumontXenomorphSalveHealEffect", new EntityCoordinates(args.Target, Vector2.Zero));
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_drool1.ogg"), args.Target);
        if (TryComp<XenomorphHealerComponent>(ent, out var healer))
        {
            healer.SalvingUntil = _timing.CurTime + TimeSpan.FromSeconds(5);
            if (target.Caste != "Larva" && !sameStrain)
                healer.Transferred = Math.Min(healer.MaxTransferred, healer.Transferred + (int)selfDamage.Float());
            Dirty(ent, healer);
        }
        args.Handled = true;
    }

    private bool IsOnFire(EntityUid uid) => TryComp<FlammableComponent>(uid, out var fire) && fire.OnFire;

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphSalveComponent, DamageableComponent>();
        while (query.MoveNext(out var uid, out var salve, out var damage))
        {
            if (_mobState.IsDead(uid))
            {
                RemCompDeferred<XenomorphSalveComponent>(uid);
                continue;
            }

            for (var i = salve.Stacks.Count - 1; i >= 0; i--)
            {
                var stack = salve.Stacks[i];
                if (_timing.CurTime < stack.NextHeal)
                    continue;

                stack.NextHeal = _timing.CurTime + TimeSpan.FromSeconds(1);
                if (damage.TotalDamage > 0)
                {
                    var remaining = stack.Amount;
                    var healing = new DamageSpecifier();
                    foreach (var group in HealGroups)
                    {
                        var types = _prototypes.Index(group).DamageTypes;
                        var total = FixedPoint2.Zero;
                        foreach (var type in types)
                        {
                            if (damage.Damage.DamageDict.TryGetValue(type, out var value) && value > 0)
                                total += value;
                        }
                        if (total <= 0 || remaining <= 0)
                            continue;

                        var amount = FixedPoint2.Min(total, remaining);
                        foreach (var type in types)
                        {
                            if (damage.Damage.DamageDict.TryGetValue(type, out var value) && value > 0)
                                healing.DamageDict[type] = -(amount * value / total);
                        }
                        remaining -= amount;
                    }
                    _damage.TryChangeDamage(uid, healing, ignoreResistances: true, targetPart: TargetBodyPart.All,
                        splitDamage: SplitDamageBehavior.SplitEnsureAll);
                }
                if (--stack.Charges <= 0)
                    salve.Stacks.RemoveAt(i);
            }
            if (salve.Stacks.Count == 0)
                RemCompDeferred<XenomorphSalveComponent>(uid);
        }
    }
}
