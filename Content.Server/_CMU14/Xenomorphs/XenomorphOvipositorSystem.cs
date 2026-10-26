using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared.Actions;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared._White.Xenomorphs.Egg;
using Content.Server.DoAfter;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphOvipositorSystem : EntitySystem
{
    [Dependency] private XenomorphRoyalSystem _royal = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    private readonly HashSet<Entity<XenomorphEggComponent>> _eggs = new();
    private readonly HashSet<Entity<XenomorphPortableEggComponent>> _portableEggs = new();
    private static readonly Vector2[] EggOffsets = [new(-1, -1), new(0, -1), new(1, -1), new(-1, 0), new(1, 0), new(-1, 1), new(0, 1), new(1, 1)];

    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private TransformSystem _transform = default!;
    [Dependency] private AppearanceSystem _appearance = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphOvipositorComponent, XenomorphOvipositorActionEvent>(OnAction);
        SubscribeLocalEvent<XenomorphOvipositorComponent, XenomorphOvipositorDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<XenomorphOvipositorComponent, MobStateChangedEvent>(OnMobState);
        SubscribeLocalEvent<XenomorphOvipositorComponent, AnchorStateChangedEvent>(OnAnchor);
    }

    private void OnAction(Entity<XenomorphOvipositorComponent> ent, ref XenomorphOvipositorActionEvent args)
    {
        if (args.Handled || !_mobState.IsAlive(ent))
            return;

        var attach = !ent.Comp.Attached;
        if (attach && (_timing.CurTime < ent.Comp.ReadyAt || !TryComp<XenomorphComponent>(ent, out var xeno) || !xeno.OnWeed
            || !_plasma.HasPlasma(ent, ent.Comp.PlasmaCost)
            || Transform(ent).GridUid == null
            || TryComp<XenomorphRestComponent>(ent, out var rest) && rest.Resting))
            return;

        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent,
            attach ? ent.Comp.AttachDelay : ent.Comp.DetachDelay,
            new XenomorphOvipositorDoAfterEvent { Attach = attach }, ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        });
    }

    private void OnDoAfter(Entity<XenomorphOvipositorComponent> ent, ref XenomorphOvipositorDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || !_mobState.IsAlive(ent) || ent.Comp.Attached == args.Attach)
            return;

        if (args.Attach)
        {
            if (!TryComp<XenomorphComponent>(ent, out var xeno) || !xeno.OnWeed
                || Transform(ent).GridUid == null
                || !_plasma.HasPlasma(ent, ent.Comp.PlasmaCost)
                || TryComp<XenomorphRestComponent>(ent, out var rest) && rest.Resting)
                return;

            _transform.AnchorEntity(ent, Transform(ent));
            if (!Transform(ent).Anchored)
                return;

            _plasma.ChangePlasmaAmount(ent, -ent.Comp.PlasmaCost);
            ent.Comp.NextEgg = _timing.CurTime + ent.Comp.EggDelay;
        }
        else
        {
            _transform.Unanchor(ent, Transform(ent));
            _physics.TrySetBodyType(ent, BodyType.KinematicController);
        }

        ent.Comp.Attached = args.Attach;
        ent.Comp.ReadyAt = args.Attach ? _timing.CurTime : _timing.CurTime + TimeSpan.FromMinutes(5);
        _royal.UpdateOvipositorActions(ent);
        SetActionActive(ent, args.Attach);
        Dirty(ent);
        _appearance.SetData(ent, XenomorphVisuals.Ovipositor, args.Attach);
        args.Handled = true;
    }

    private void OnAnchor(Entity<XenomorphOvipositorComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (args.Anchored || !ent.Comp.Attached)
            return;

        ent.Comp.Attached = false;
        ent.Comp.ReadyAt = _timing.CurTime + TimeSpan.FromMinutes(5);
        _royal.UpdateOvipositorActions(ent);
        SetActionActive(ent, false);
        Dirty(ent);
        _physics.TrySetBodyType(ent, BodyType.KinematicController);
        _appearance.SetData(ent, XenomorphVisuals.Ovipositor, false);
    }

    private void OnMobState(Entity<XenomorphOvipositorComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Alive || !ent.Comp.Attached)
            return;

        ent.Comp.Attached = false;
        ent.Comp.ReadyAt = _timing.CurTime + TimeSpan.FromMinutes(5);
        _royal.UpdateOvipositorActions(ent);
        SetActionActive(ent, false);
        Dirty(ent);
        _transform.Unanchor(ent, Transform(ent));
        _physics.TrySetBodyType(ent, BodyType.KinematicController);
        _appearance.SetData(ent, XenomorphVisuals.Ovipositor, false);
    }

    private void SetActionActive(EntityUid uid, bool attached)
    {
        foreach (var action in _actions.GetActions(uid))
        {
            if (_actions.GetEvent(action) is XenomorphOvipositorActionEvent)
            {
                _actions.SetToggled(action.Owner, attached);
                if (!attached && TryComp<XenomorphOvipositorComponent>(uid, out var ovi))
                    _actions.SetCooldown(action.Owner, ovi.ReadyAt - _timing.CurTime);
            }
        }
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphOvipositorComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var ovi, out var xform))
        {
            if (!ovi.Attached || !xform.Anchored || !_mobState.IsAlive(uid) || _timing.CurTime < ovi.NextEgg)
                continue;

            ovi.NextEgg = _timing.CurTime + ovi.EggDelay;
            foreach (var offset in EggOffsets)
            {
                var coordinates = xform.Coordinates.Offset(offset);
                var tile = _turf.GetTileRef(coordinates);
                if (tile is not { } turf || turf.Tile.IsEmpty
                    || _turf.IsTileBlocked(turf, CollisionGroup.Impassable))
                    continue;

                _eggs.Clear();
                _lookup.GetEntitiesInRange(coordinates, 0.75f, _eggs);
                _portableEggs.Clear();
                _lookup.GetEntitiesInRange(coordinates, 0.75f, _portableEggs);
                if (_eggs.Count != 0 || _portableEggs.Count != 0)
                    continue;

                Spawn(ovi.Egg, coordinates);
                break;
            }
        }
    }
}
