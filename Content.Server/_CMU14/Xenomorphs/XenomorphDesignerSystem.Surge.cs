using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using System.Numerics;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.Projectiles;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphDesignSurgingComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan End;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan Next;
    public bool Active;
    public EntityUid Action;
    public readonly List<EntityUid> Nodes = new();
    public readonly List<EntityUid> Effects = new();
}

[RegisterComponent]
public sealed partial class XenomorphReflectiveResinComponent : Component
{
    [DataField] public float Chance = .75f;
}

[RegisterComponent]
public sealed partial class XenomorphReflectedProjectileComponent : Component
{
    public readonly HashSet<EntityUid> Walls = new();
    public EntityUid LastWall;
    public MapCoordinates Position;
    public float Remaining = 10;
}

public sealed partial class XenomorphDesignerSystem
{
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private IRobustRandom _random = default!;

    private bool CanThicken(EntityUid user, EntityUid target)
        => Ready(user) && Exists(target) && !HasComp<XenomorphWeedboundComponent>(target)
            && !HasComp<XenomorphThickResinComponent>(target)
            && (MetaData(target).EntityPrototype?.ID is "WallResin" or "ResinDoor")
            && Transform(user).GridUid == Transform(target).GridUid
            && _interaction.InRangeUnobstructed(user, target, range: 10) && _plasma.HasPlasma(user, 60);

    private void OnThicken(Entity<XenomorphDesignerComponent> ent, ref XenomorphDesignThickenActionEvent args)
    {
        if (args.Handled || !CanThicken(ent, args.Target)) return;
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, 1,
            new XenomorphDesignThickenDoAfterEvent(), ent, args.Target)
        { NeedHand = false, DistanceThreshold = 10, BreakOnMove = false, BlockDuplicate = true, CancelDuplicate = false });
    }

    private void OnThickened(Entity<XenomorphDesignerComponent> ent, ref XenomorphDesignThickenDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target || !CanThicken(ent, target)
            || !_plasma.ChangePlasmaAmount(ent, -60)) return;
        var coordinates = Transform(target).Coordinates;
        var rotation = Transform(target).LocalRotation;
        var door = HasComp<DoorComponent>(target);
        Del(target);
        var thickened = Spawn(door ? "DumontXenomorphResinDoorThick" : "DumontXenomorphResinWallThick", coordinates);
        _transform.SetLocalRotation(thickened, rotation);
        PlayBuildSound(coordinates);
        args.Handled = true;
    }

    private void OnSurge(Entity<XenomorphDesignerComponent> ent, ref XenomorphDesignSurgeActionEvent args)
    {
        if (args.Handled || !Ready(ent) || !_plasma.HasPlasma(ent, 250)
            || TryComp<XenomorphDesignSurgingComponent>(ent, out var current) && (current.Active || current.Next > _timing.CurTime)) return;
        var origin = _transform.GetMapCoordinates(ent);
        var nodes = ent.Comp.Nodes.Where(uid => !TerminatingOrDeleted(uid) && TryComp<XenomorphDesignNodeComponent>(uid, out var design) && design.Designer == ent.Owner
            && _transform.GetMapCoordinates(uid).MapId == origin.MapId
            && Vector2.Distance(_transform.GetWorldPosition(uid), origin.Position) <= 7 && Clear(Transform(uid).Coordinates)).ToArray();
        if (nodes.Length == 0)
        {
            _popup.PopupEntity(Loc.GetString("dumont-xenomorph-design-surge-none"), ent, ent);
            return;
        }
        if (!_plasma.ChangePlasmaAmount(ent, -250)) return;
        var surge = EnsureComp<XenomorphDesignSurgingComponent>(ent);
        surge.Active = true;
        surge.End = _timing.CurTime + TimeSpan.FromSeconds(1);
        surge.Next = _timing.CurTime + TimeSpan.FromSeconds(30);
        surge.Action = args.Action.Owner;
        surge.Nodes.AddRange(nodes);
        foreach (var node in nodes) surge.Effects.Add(CreateBuildEffect(Transform(node).Coordinates, false, true, 1));
        _actions.SetToggled(args.Action.Owner, true);
        args.Handled = true;
    }

    private void UpdateSurges()
    {
        var query = EntityQueryEnumerator<XenomorphDesignSurgingComponent>();
        while (query.MoveNext(out var user, out var surge))
        {
            if (!surge.Active || _timing.CurTime < surge.End) continue;
            foreach (var node in surge.Nodes)
            {
                if (TerminatingOrDeleted(node) || !TryComp<XenomorphDesignNodeComponent>(node, out var design)
                    || design.Designer != user || !Clear(Transform(node).Coordinates)) continue;
                Spawn("DumontXenomorphReflectiveResinWallUnstable", Transform(node).Coordinates);
                QueueDel(node);
            }
            foreach (var effect in surge.Effects) if (Exists(effect)) QueueDel(effect);
            surge.Effects.Clear();
            surge.Nodes.Clear();
            surge.Active = false;
            _actions.SetToggled(surge.Action, false);
        }
        var reflected = EntityQueryEnumerator<XenomorphReflectedProjectileComponent>();
        while (reflected.MoveNext(out var uid, out var projectile))
        {
            var coordinates = _transform.GetMapCoordinates(uid);
            if (coordinates.MapId != projectile.Position.MapId) { QueueDel(uid); continue; }
            projectile.Remaining -= Vector2.Distance(coordinates.Position, projectile.Position.Position);
            projectile.Position = coordinates;
            if (projectile.Remaining <= 0) QueueDel(uid);
        }
    }

    private void OnReflectHitscan(Entity<XenomorphReflectiveResinComponent> ent, ref Content.Shared.Weapons.Ranged.Events.HitScanReflectAttemptEvent args)
    {
        if (args.Reflected || !_random.Prob(ent.Comp.Chance)) return;
        args.Direction = _random.NextAngle(Angle.FromDegrees(-30), Angle.FromDegrees(30)).Opposite().RotateVec(args.Direction);
        if (args.Damage is { } damage) args.Damage = damage * .5f;
        args.Reflected = true;
    }

    private void OnReflect(Entity<XenomorphReflectiveResinComponent> ent, ref ProjectileReflectAttemptEvent args)
    {
        if (args.Cancelled || !TryComp<PhysicsComponent>(args.ProjUid, out var body)) return;
        if (TryComp<XenomorphReflectedProjectileComponent>(args.ProjUid, out var old) && old.Walls.Contains(ent))
        {
            args.Cancelled = old.LastWall == ent.Owner;
            return;
        }
        if (!_random.Prob(ent.Comp.Chance)) return;
        var rotation = _random.NextAngle(Angle.FromDegrees(-30), Angle.FromDegrees(30)).Opposite();
        var velocity = rotation.RotateVec(_physics.GetMapLinearVelocity(args.ProjUid, component: body));
        _physics.SetLinearVelocity(args.ProjUid, velocity, body: body);
        _transform.SetWorldRotation(args.ProjUid, rotation + _transform.GetWorldRotation(args.ProjUid));
        var reflected = EnsureComp<XenomorphReflectedProjectileComponent>(args.ProjUid);
        reflected.Walls.Add(ent);
        reflected.LastWall = ent;
        reflected.Position = _transform.GetMapCoordinates(args.ProjUid);
        reflected.Remaining = 10;
        args.Component.Damage *= .5f;
        args.Component.IgnoreShooter = false;
        Dirty(args.ProjUid, args.Component);
        args.Cancelled = true;
    }
}
