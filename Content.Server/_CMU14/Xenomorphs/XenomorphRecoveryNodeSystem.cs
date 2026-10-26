using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared._White.Xenomorphs.Plasma.Components;
using Content.Server._White.Xenomorphs.Plasma;
using Robust.Shared.Map;



using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphRecoveryNodeSystem : EntitySystem
{
    [Dependency] private IGameTiming _time = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;

    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private XenomorphHealerSystem _heal = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private MobStateSystem _mob = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedDoAfterSystem _doafter = default!;
    [Dependency] private Content.Shared.Interaction.SharedInteractionSystem _interaction = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphRecoveryNodeComponent, XenomorphRecoveryNodeRecoverDoAfterEvent>(OnRecoveryDoAfter);

    }

    public override void Update(float frameTime)
    {
        if (_net.IsClient)
            return;

        var curTime = _time.CurTime;
        var recoverNodes = EntityQueryEnumerator<XenomorphRecoveryNodeComponent>();

        while (recoverNodes.MoveNext(out var ent, out var comp))
        {
            if (comp.NextHealAt < curTime && comp.HealDoAfter == null)
            {
                TryHealRandomXeno((ent, comp));
            }
        }
    }

    private void TryHealRandomXeno(Entity<XenomorphRecoveryNodeComponent> recoveryNode)
    {
        var (ent, comp) = recoveryNode;
        var nearbyEntities = _lookup.GetEntitiesInRange(ent, comp.HealRange);
        var possibleTargets = new List<EntityUid>();
        foreach (var nearbyEntity in nearbyEntities)
        {
            if (!HasComp<XenomorphLineageComponent>(nearbyEntity) ||
                !HasComp<XenomorphComponent>(nearbyEntity) ||
                (!TryComp<XenomorphRestComponent>(nearbyEntity, out var rest) || !rest.Resting) ||
                !TryComp<DamageableComponent>(nearbyEntity, out var damageComp) ||
                damageComp.TotalDamage <= 0 ||
                !HasComp<MobStateComponent>(nearbyEntity) ||
                _mob.IsDead(nearbyEntity))
            {
                continue;
            }

            possibleTargets.Add(nearbyEntity);
        }

        recoveryNode.Comp.NextHealAt = _time.CurTime + recoveryNode.Comp.HealCooldown;

        if (possibleTargets.Count == 0)
            return;

        var selectedTarget = _random.Pick(possibleTargets);
        var recover = new DoAfterArgs(EntityManager, recoveryNode, recoveryNode.Comp.HealCooldown, new XenomorphRecoveryNodeRecoverDoAfterEvent(), recoveryNode, selectedTarget)
        {
            BreakOnMove = true,
            MovementThreshold = 0.5f,
            DuplicateCondition = DuplicateConditions.SameEvent,

        };

        if (_doafter.TryStartDoAfter(recover, out var id))
        {
            recoveryNode.Comp.HealDoAfter = id;
            Spawn("DumontHiveHealBusy", new EntityCoordinates(selectedTarget, System.Numerics.Vector2.Zero));
            _popup.PopupEntity(Loc.GetString("xenomorph-hive-recovery-start"), selectedTarget, selectedTarget);
            _popup.PopupEntity(Loc.GetString("xenomorph-hive-recovery-other", ("target", selectedTarget)), selectedTarget, Filter.PvsExcept(selectedTarget), true);
        }
    }

    private void OnRecoveryDoAfter(Entity<XenomorphRecoveryNodeComponent> recoveryNode, ref XenomorphRecoveryNodeRecoverDoAfterEvent args)
    {
        recoveryNode.Comp.HealDoAfter = null;

        if (args.Handled || args.Cancelled || args.Target == null)
            return;

        if (_mob.IsDead(args.Target.Value) || !_interaction.InRangeUnobstructed(recoveryNode.Owner, args.Target.Value, recoveryNode.Comp.HealRange)) return;
        _heal.Heal(args.Target.Value, recoveryNode.Comp.HealAmount);
    }
}
