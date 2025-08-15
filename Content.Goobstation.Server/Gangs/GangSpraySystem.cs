using Content.Goobstation.Server.Gangs.Roles;
using Content.Goobstation.Shared.Gangs;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Map.Components;

namespace Content.Goobstation.Server.Gangs;

public sealed class GangSpraySystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GangSprayComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<GangSprayComponent, GangSprayDoAfterEvent>(OnDoAfterComplete);
    }

    private void OnAfterInteract(EntityUid uid, GangSprayComponent comp, AfterInteractEvent args)
    {
        if (!args.CanReach
            || args.Target == null
            || args.Handled)
            return;

        EntityUid? gangEntity = null;
        if (TryComp<GangMemberComponent>(args.User, out var memberComp))
            gangEntity = memberComp.GangId;
        else if (TryComp<GangLeaderComponent>(args.User, out var leaderComp))
            gangEntity = leaderComp.GangId;
        else if (TryComp<GangLeaderRoleComponent>(args.User, out var roleComp))
            gangEntity = roleComp.GangId;

        if (gangEntity == null
            || !_entMan.EntityExists(gangEntity.Value)
            || gangEntity.Value == EntityUid.Invalid)
        {
            _popup.PopupEntity(Loc.GetString("gang-spray-cant"), args.User, args.User);
            return;
        }

        if (!HasComp<MapGridComponent>(args.Target.Value) && !_tag.HasTag(args.Target.Value, "Wall"))
            return;

        var doAfterEvent = new GangSprayDoAfterEvent(GetNetEntity(gangEntity.Value));


        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, comp.SprayTime, doAfterEvent, uid, target: args.Target, used: uid)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            MovementThreshold = 0.5f
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            return;

        _popup.PopupEntity(Loc.GetString("gang-spray-start", ("target", args.Target.Value)), args.User, args.User);
        args.Handled = true;
    }

    private void OnDoAfterComplete(EntityUid uid, GangSprayComponent comp, GangSprayDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled
            || args.Args.Target == null)
            return;

        var user = args.Args.User;

        if (!_entMan.TryGetEntity(args.GangEntity, out var gangEntity)
            || gangEntity == EntityUid.Invalid)
        {
            _popup.PopupEntity(Loc.GetString("gang-spray-failed"), user, user);
            return;
        }

        var coords = Transform(args.Args.Target.Value).Coordinates;
        RemoveOldGraffiti(coords);

        var randomIndex = _random.Next(0, comp.MaxGraffitiPrototypes);
        var prototypeId = $"GangSign{randomIndex}"; // its hardcoded, but im so fucking done with it

        if (!_prototype.HasIndex<EntityPrototype>(prototypeId))
            return;

        var graffiti = Spawn(prototypeId, coords);
        var graffitiComp = EnsureComp<GangGraffitiComponent>(graffiti);
        var gangUid = _entMan.GetEntity(args.GangEntity);
        graffitiComp.GangId = gangUid;

        _popup.PopupEntity(Loc.GetString("gang-spray-success"), user, user);
        args.Handled = true;
    }

    private void RemoveOldGraffiti(EntityCoordinates coords)
    {
        var query = EntityQueryEnumerator<GangGraffitiComponent, TransformComponent>();
        while (query.MoveNext(out var graffitiUid, out _, out var xform))
        {
            if (xform.Coordinates.Equals(coords))
            {
                QueueDel(graffitiUid);
            }
        }
    }
}
