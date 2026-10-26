using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared._White.Xenomorphs.Plasma.Components;
using Content.Server._White.Xenomorphs.Plasma;
using Robust.Shared.Map;




using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphPlasmaTreeSystem : EntitySystem
{
    [Dependency] private IGameTiming _time = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;

    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private MobStateSystem _mob = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedDoAfterSystem _doafter = default!;
    [Dependency] private Content.Shared.Interaction.SharedInteractionSystem _interaction = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphPlasmaTreeComponent, XenomorphPlasmaTreeRecoverDoAfterEvent>(OnRecoveryDoAfter);
    }

    public override void Update(float frameTime)
    {
        if (_net.IsClient)
            return;

        var curTime = _time.CurTime;
        var plasmaTrees = EntityQueryEnumerator<XenomorphPlasmaTreeComponent>();

        while (plasmaTrees.MoveNext(out var ent, out var comp))
        {
            if (comp.NextPlasmaAt < curTime && comp.PlasmaDoAfter == null)
            {
                TryGivePlasmaRandomXeno((ent, comp));
            }
        }
    }

    private void TryGivePlasmaRandomXeno(Entity<XenomorphPlasmaTreeComponent> plasmaTree)
    {
        var (ent, comp) = plasmaTree;
        var nearbyEntities = _lookup.GetEntitiesInRange(ent, comp.PlasmaRange);
        var possibleTargets = new List<EntityUid>();
        foreach (var nearbyEntity in nearbyEntities)
        {
            if (!HasComp<XenomorphLineageComponent>(nearbyEntity) ||
                !HasComp<XenomorphComponent>(nearbyEntity) ||
                !(TryComp<XenomorphRestComponent>(nearbyEntity, out var rest) && rest.Resting || TryComp<XenomorphOvipositorComponent>(nearbyEntity, out var ovi) && ovi.Attached) ||
                !TryComp<PlasmaVesselComponent>(nearbyEntity, out var plasmaComp) ||
                plasmaComp.Plasma == plasmaComp.MaxPlasma ||
                !HasComp<MobStateComponent>(nearbyEntity) ||
                _mob.IsDead(nearbyEntity))
            {
                continue;
            }

            possibleTargets.Add(nearbyEntity);
        }

        plasmaTree.Comp.NextPlasmaAt = _time.CurTime + plasmaTree.Comp.PlasmaCooldown;

        if (possibleTargets.Count == 0)
            return;

        var selectedTarget = _random.Pick(possibleTargets);
        var recover = new DoAfterArgs(EntityManager,
            plasmaTree,
            plasmaTree.Comp.PlasmaCooldown,
            new XenomorphPlasmaTreeRecoverDoAfterEvent(),
            plasmaTree,
            selectedTarget)
        {
            BreakOnMove = true,
            MovementThreshold = 0.5f,
            DuplicateCondition = DuplicateConditions.SameEvent,

        };

        if (_doafter.TryStartDoAfter(recover, out var id))
        {
            plasmaTree.Comp.PlasmaDoAfter = id;
            Spawn("DumontHiveHealPlasma", new EntityCoordinates(selectedTarget, System.Numerics.Vector2.Zero));
            _popup.PopupEntity(Loc.GetString("xenomorph-hive-recovery-start"),
                selectedTarget,
                selectedTarget);
            _popup.PopupEntity(
                Loc.GetString("xenomorph-hive-recovery-other", ("target", selectedTarget)),
                selectedTarget,
                Filter.PvsExcept(selectedTarget),
                true);
        }
    }

    private void OnRecoveryDoAfter(Entity<XenomorphPlasmaTreeComponent> plasmaTree, ref XenomorphPlasmaTreeRecoverDoAfterEvent args)
    {
        plasmaTree.Comp.PlasmaDoAfter = null;

        if (args.Handled || args.Cancelled || args.Target == null)
            return;

        if (_mob.IsDead(args.Target.Value) || !_interaction.InRangeUnobstructed(plasmaTree.Owner, args.Target.Value, plasmaTree.Comp.PlasmaRange)) return;
        _plasma.ChangePlasmaAmount(args.Target.Value, plasmaTree.Comp.PlasmaAmount);
    }
}
