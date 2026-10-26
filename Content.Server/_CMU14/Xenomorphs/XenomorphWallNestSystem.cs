using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared.ActionBlocker;
using Content.Shared.Mobs.Systems;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Tag;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphWallNestSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private TransformSystem _transform = default!;
    [Dependency] private TagSystem _tags = default!;

    [Dependency] private PopupSystem _popup = default!;

    private static readonly ProtoId<TagPrototype> WallTag = "Wall";
    private static readonly EntProtoId Nest = "DumontXenomorphWallNest";

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphComponent, XenomorphWallNestActionEvent>(OnAction);
        SubscribeLocalEvent<XenomorphComponent, XenomorphWallNestDoAfterEvent>(OnDoAfter);
    }

    private bool CanBuild(EntityUid user, EntityUid wall)
    {
        string? error = null;
        if (!_mobState.IsAlive(user) || !_blocker.CanInteract(user, wall))
            return false;
        if (!_tags.HasTag(wall, WallTag) || !Transform(wall).Anchored
            || Transform(user).GridUid == null || Transform(user).GridUid != Transform(wall).GridUid
            || !_interaction.InRangeUnobstructed(user, wall, range: 1.6f))
            error = "xenomorph-wall-nest-unreachable";
        else if (!TryComp<XenomorphWallResinComponent>(wall, out var resin) || !Exists(resin.Overlay))
            error = "xenomorph-wall-nest-no-resin";
        else if (Exists(resin.Nest))
            error = "xenomorph-wall-nest-occupied";
        else if (!_plasma.HasPlasma(user, 100))
            error = "xenomorph-wall-nest-plasma";

        if (error == null)
            return true;
        _popup.PopupEntity(Loc.GetString(error), user, user);
        return false;
    }

    private void OnAction(Entity<XenomorphComponent> ent, ref XenomorphWallNestActionEvent args)
    {
        if (args.Handled)
            return;

        var wall = HasComp<XenomorphWallResinOverlayComponent>(args.Target)
            ? Transform(args.Target).ParentUid
            : args.Target;
        if (!Exists(wall) || !CanBuild(ent, wall))
            return;

        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, 3,
            new XenomorphWallNestDoAfterEvent(), ent, target: wall)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        });
    }

    private void OnDoAfter(Entity<XenomorphComponent> ent, ref XenomorphWallNestDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } wall || !CanBuild(ent, wall))
            return;

        var localUser = _transform.ToCoordinates(wall, _transform.GetMapCoordinates(ent));
        var delta = localUser.Position;
        var offset = Math.Abs(delta.X) > Math.Abs(delta.Y)
            ? new Vector2(Math.Sign(delta.X), 0)
            : new Vector2(0, Math.Sign(delta.Y));
        if (!_plasma.ChangePlasmaAmount(ent, -100))
            return;

        var nest = Spawn(Nest, new EntityCoordinates(wall, offset * 0.65f));
        Comp<XenomorphWallResinComponent>(wall).Nest = nest;
        args.Handled = true;
    }

}
