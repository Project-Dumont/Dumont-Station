using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using System.Numerics;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphDesignBuildingComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan End;
    public EntityUid User;
    public EntityUid? Effect;
    public bool Thick;
}

[RegisterComponent]
public sealed partial class XenomorphWeedboundComponent : Component
{
    public EntityUid? Weed;
    [DataField] public bool Thick;
}

[RegisterComponent]
public sealed partial class XenomorphThickResinComponent : Component;

public sealed partial class XenomorphDesignerSystem : EntitySystem
{
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphReflectiveResinComponent, Content.Shared.Projectiles.ProjectileReflectAttemptEvent>(OnReflect);
        SubscribeLocalEvent<XenomorphReflectiveResinComponent, Content.Shared.Weapons.Ranged.Events.HitScanReflectAttemptEvent>(OnReflectHitscan);
        SubscribeLocalEvent<XenomorphDesignerComponent, XenomorphDesignActionEvent>(OnDesign);
        SubscribeLocalEvent<XenomorphDesignerComponent, XenomorphDesignDoAfterEvent>(OnDesigned);
        SubscribeLocalEvent<XenomorphDesignerComponent, XenomorphDesignToggleActionEvent>(OnToggle);
        SubscribeLocalEvent<XenomorphDesignerComponent, XenomorphDesignRemoveActionEvent>(OnRemove);
        SubscribeLocalEvent<XenomorphDesignerComponent, XenomorphDesignThickenActionEvent>(OnThicken);
        SubscribeLocalEvent<XenomorphDesignerComponent, XenomorphDesignThickenDoAfterEvent>(OnThickened);
        SubscribeLocalEvent<XenomorphDesignerComponent, XenomorphDesignSurgeActionEvent>(OnSurge);
        SubscribeLocalEvent<XenomorphDesignNodeComponent, EntityTerminatingEvent>(OnNodeRemoved);
        SubscribeLocalEvent<XenomorphDesignNodeComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<XenomorphDesignBuildingComponent, ComponentRemove>(OnBuildingRemoved);
        SubscribeLocalEvent<XenomorphDesignSurgingComponent, ComponentRemove>(OnSurgeRemoved);
        SubscribeLocalEvent<XenomorphWeedboundComponent, Content.Shared.Destructible.DestructionEventArgs>(OnBoundDestroyed);
    }

    private bool Ready(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanConsciouslyPerformAction(uid)
        && (!TryComp<XenomorphRestComponent>(uid, out var rest) || !rest.Resting);

    private EntityCoordinates Snap(EntityCoordinates coordinates)
    {
        if (_transform.GetGrid(coordinates) is not { } grid) return coordinates;
        var local = _transform.ToCoordinates(grid, _transform.ToMapCoordinates(coordinates));
        return new EntityCoordinates(grid, MathF.Floor(local.X) + .5f, MathF.Floor(local.Y) + .5f);
    }

    private bool Reach(EntityUid user, EntityCoordinates coordinates, float range)
        => Ready(user) && Transform(user).GridUid is { } grid && _transform.GetGrid(coordinates) == grid
            && _interaction.InRangeUnobstructed(user, coordinates, range: range);

    private bool Clear(EntityCoordinates coordinates)
        => _turf.GetTileRef(coordinates) is { } tile && !tile.Tile.IsEmpty
            && !_turf.IsTileBlocked(tile, CollisionGroup.Impassable | CollisionGroup.MidImpassable | CollisionGroup.HighImpassable);

    private EntityUid? Weed(EntityCoordinates coordinates)
    {
        foreach (var weed in _lookup.GetEntitiesInRange<XenomorphWeedWallSpreadComponent>(coordinates, .4f, LookupFlags.All))
            if (!TerminatingOrDeleted(weed)) return weed;
        return null;
    }

    private IEnumerable<Entity<XenomorphDesignNodeComponent>> Nodes(EntityCoordinates coordinates)
        => _lookup.GetEntitiesInRange<XenomorphDesignNodeComponent>(coordinates, .4f, LookupFlags.All)
            .Where(n => !TerminatingOrDeleted(n));

    private static int Cost(XenomorphDesignType kind) => kind switch
    { XenomorphDesignType.Optimized => 50, XenomorphDesignType.Flexible => 60, _ => 70 };

    private bool CanDesign(EntityUid user, EntityCoordinates coordinates, XenomorphDesignType kind)
        => Reach(user, coordinates, 15) && Clear(coordinates) && Weed(coordinates) != null && !Nodes(coordinates).Any()
            && _plasma.HasPlasma(user, Cost(kind));

    private void OnDesign(Entity<XenomorphDesignerComponent> ent, ref XenomorphDesignActionEvent args)
    {
        var coordinates = Snap(args.Target);
        if (args.Handled || !CanDesign(ent, coordinates, args.Kind)) return;
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, .25f,
            new XenomorphDesignDoAfterEvent { Coordinates = GetNetCoordinates(coordinates), Kind = args.Kind, Door = ent.Comp.Door }, ent)
        { NeedHand = false, BreakOnMove = true, BlockDuplicate = true, CancelDuplicate = false });
    }

    private void OnDesigned(Entity<XenomorphDesignerComponent> ent, ref XenomorphDesignDoAfterEvent args)
    {
        var coordinates = GetCoordinates(args.Coordinates);
        if (args.Cancelled || args.Handled || !CanDesign(ent, coordinates, args.Kind)
            || !_plasma.ChangePlasmaAmount(ent, -Cost(args.Kind))) return;
        var kind = args.Kind.ToString();
        var uid = Spawn($"DumontXenomorphDesign{kind}{(args.Door ? "Door" : "Wall")}", coordinates);
        var node = Comp<XenomorphDesignNodeComponent>(uid);
        node.Designer = ent;
        node.Weed = Weed(coordinates);
        Dirty(uid, node);
        ent.Comp.Nodes.RemoveAll(uid => TerminatingOrDeleted(uid));
        ent.Comp.Nodes.Add(uid);
        while (ent.Comp.Nodes.Count > ent.Comp.Limit)
        {
            QueueDel(ent.Comp.Nodes[0]);
            ent.Comp.Nodes.RemoveAt(0);
        }
        ent.Comp.Count = ent.Comp.Nodes.Count;
        Dirty(ent);
        PlayBuildSound(coordinates);
        args.Handled = true;
    }

    private void OnNodeRemoved(Entity<XenomorphDesignNodeComponent> ent, ref EntityTerminatingEvent args)
    {
        if (!TryComp<XenomorphDesignerComponent>(ent.Comp.Designer, out var designer)) return;
        designer.Nodes.Remove(ent);
        designer.Count = designer.Nodes.Count;
        Dirty(designer.Owner, designer);
    }

    private void OnToggle(Entity<XenomorphDesignerComponent> ent, ref XenomorphDesignToggleActionEvent args)
    {
        if (args.Handled || !Ready(ent)) return;
        ent.Comp.Door = !ent.Comp.Door;
        Dirty(ent);
        _actions.SetToggled(args.Action.Owner, ent.Comp.Door);
        _popup.PopupEntity(Loc.GetString(ent.Comp.Door ? "dumont-xenomorph-design-door" : "dumont-xenomorph-design-wall"), ent, ent);
        args.Handled = true;
    }

    private void OnRemove(Entity<XenomorphDesignerComponent> ent, ref XenomorphDesignRemoveActionEvent args)
    {
        if (args.Handled || !Reach(ent, args.Target, 15)) return;
        var node = Nodes(Snap(args.Target)).FirstOrDefault(n => n.Comp.Designer == ent.Owner);
        if (node.Owner == EntityUid.Invalid || !_plasma.ChangePlasmaAmount(ent, -25)) return;
        QueueDel(node);
        PlayBuildSound(Transform(node).Coordinates);
        args.Handled = true;
    }

    public void PlayBuildSound(EntityCoordinates coordinates)
        => _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_resin_build1.ogg", AudioParams.Default.WithVolume(-8)), coordinates);

    public EntityUid CreateBuildEffect(EntityCoordinates coordinates, bool door, bool thick, float duration, bool membrane = false)
    {
        var effect = Spawn($"DumontXenomorphDesignBuild{(door ? "Door" : membrane ? "Membrane" : "Wall")}{(thick ? "Thick" : "")}", coordinates);
        var visual = Comp<XenomorphResinBuildVisualComponent>(effect);
        visual.Duration = duration;
        visual.End = _timing.CurTime + TimeSpan.FromSeconds(duration);
        Dirty(effect, visual);
        return effect;
    }

    private void OnActivate(Entity<XenomorphDesignNodeComponent> ent, ref ActivateInWorldEvent args)
    {
        var user = args.User;
        if (args.Handled || ent.Comp.Kind != XenomorphDesignType.Construct || HasComp<XenomorphDesignBuildingComponent>(ent)
            || !HasComp<XenomorphComponent>(user) || !Reach(user, Transform(ent).Coordinates, 1.6f)
            || TerminatingOrDeleted(ent.Comp.Weed) || !Clear(Transform(ent).Coordinates)
            || !_plasma.ChangePlasmaAmount(user, -70)) return;
        var thick = HasComp<XenomorphResinWhispererComponent>(user) || HasComp<XenomorphDesignerComponent>(user)
            || TryComp<XenomorphComponent>(user, out var xeno) && xeno.Caste.Id == "Hivelord";
        var build = EnsureComp<XenomorphDesignBuildingComponent>(ent);
        build.User = user;
        build.Thick = thick;
        build.End = _timing.CurTime + TimeSpan.FromSeconds(4);
        build.Effect = CreateBuildEffect(Transform(ent).Coordinates, ent.Comp.Door, thick, 4);
        var direction = _transform.GetWorldPosition(ent) - _transform.GetWorldPosition(user);
        if (direction.LengthSquared() > .001f)
        {
            var local = (-_transform.GetWorldRotation(user)).RotateVec(Vector2.Normalize(direction));
            _melee.DoLunge(user, user, user, Angle.Zero, local, "WeaponArcThrust", Angle.Zero, false, predicted: false);
        }
        PlayBuildSound(Transform(ent).Coordinates);
        args.Handled = true;
    }

    private void OnBuildingRemoved(Entity<XenomorphDesignBuildingComponent> ent, ref ComponentRemove args)
    {
        if (Exists(ent.Comp.Effect)) QueueDel(ent.Comp.Effect);
    }

    private void Complete(Entity<XenomorphDesignNodeComponent> ent, XenomorphDesignBuildingComponent build)
    {
        var coordinates = Transform(ent).Coordinates;
        RemComp<XenomorphDesignBuildingComponent>(ent);
        if (!HasComp<XenomorphComponent>(build.User) || TerminatingOrDeleted(ent.Comp.Weed) || !Clear(coordinates)) return;
        var wall = Spawn($"DumontXenomorphWeedbound{(ent.Comp.Door ? "Door" : "Wall")}{(build.Thick ? "Thick" : "")}", coordinates);
        Comp<XenomorphWeedboundComponent>(wall).Weed = ent.Comp.Weed;
        QueueDel(ent);
    }

    private void OnSurgeRemoved(Entity<XenomorphDesignSurgingComponent> ent, ref ComponentRemove args)
    {
        foreach (var effect in ent.Comp.Effects) if (Exists(effect)) QueueDel(effect);
    }

    private void OnBoundDestroyed(Entity<XenomorphWeedboundComponent> ent, ref Content.Shared.Destructible.DestructionEventArgs args)
        => Spawn(ent.Comp.Thick ? "DumontXenomorphDesignResidueThick" : "DumontXenomorphDesignResidue", Transform(ent).Coordinates);

    public override void Update(float frameTime)
    {
        var nodes = EntityQueryEnumerator<XenomorphDesignNodeComponent>();
        while (nodes.MoveNext(out var uid, out var node))
        {
            if (TerminatingOrDeleted(uid)) continue;
            if (TerminatingOrDeleted(node.Weed))
            {
                node.Weed = Weed(Transform(uid).Coordinates);
                if (node.Weed == null) { QueueDel(uid); continue; }
                Dirty(uid, node);
            }
            if (TryComp<XenomorphDesignBuildingComponent>(uid, out var build) && _timing.CurTime >= build.End) Complete((uid, node), build);
        }
        var walls = EntityQueryEnumerator<XenomorphWeedboundComponent>();
        while (walls.MoveNext(out var uid, out var wall))
        {
            if (TerminatingOrDeleted(uid) || !TerminatingOrDeleted(wall.Weed)) continue;
            wall.Weed = Weed(Transform(uid).Coordinates);
            if (wall.Weed != null) continue;
            Spawn(wall.Thick ? "DumontXenomorphDesignResidueThick" : "DumontXenomorphDesignResidue", Transform(uid).Coordinates);
            QueueDel(uid);
        }
        UpdateSurges();
    }
}
