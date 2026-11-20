// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared._White.Xenomorphs;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared._Trauma.Xenomorph;

public sealed partial class QueenRoarSystem : EntitySystem
{
    [Dependency] private ExamineSystemShared _examine = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private NpcFactionSystem _faction = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<QueenRoarComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<QueenRoarComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<QueenRoarComponent, QueenRoarActionEvent>(OnQueenRoar);
        SubscribeLocalEvent<QueenRoarComponent, QueenRoarDoAfterEvent>(OnQueenRoarDoAfter);
    }

    private readonly HashSet<Entity<NpcFactionMemberComponent>> _nearbyMobs = new();

    private void OnMapInit(Entity<QueenRoarComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.RoarActionEntity, ent.Comp.RoarAction);
        Dirty(ent);
    }

    private void OnShutdown(Entity<QueenRoarComponent> ent, ref ComponentShutdown args) =>
        _actions.RemoveAction(ent.Owner, ent.Comp.RoarActionEntity);

    private void OnQueenRoar(Entity<QueenRoarComponent> ent, ref QueenRoarActionEvent args)
    {
        if (args.Handled)
            return;

        var user = args.Performer;
        var doAfter = new DoAfterArgs(EntityManager, user, ent.Comp.RoarDelay, new QueenRoarDoAfterEvent(), ent.Owner)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            NeedHand = false,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
            return;

        _popup.PopupPredicted(
            Loc.GetString("queen-roar-start"),
            Loc.GetString("queen-roar-start-others"),
            ent,
            user,
            PopupType.LargeCaution);

        _audio.PlayPredicted(ent.Comp.SoundRoarStart, ent, user);
        args.Handled = true;
    }

    private void OnQueenRoarDoAfter(Entity<QueenRoarComponent> ent, ref QueenRoarDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        _audio.PlayPredicted(ent.Comp.SoundRoar, ent.Owner, args.User);

        var coords = Transform(ent.Owner).Coordinates;
        var range = ent.Comp.RoarRange;
        _nearbyMobs.Clear();
        _lookup.GetEntitiesInRange(coords, range, _nearbyMobs, LookupFlags.Uncontained);

        foreach (var mob in _nearbyMobs)
        {
            if (_faction.IsEntityFriendly(ent.Owner, (mob.Owner, mob.Comp)))
                continue;
            if (!_examine.InRangeUnOccluded(ent, mob, range))
                continue;
            _stun.KnockdownOrStun(mob, ent.Comp.RoarStunTime);
        }

        _popup.PopupEntity(Loc.GetString("queen-roar-complete"), ent.Owner, args.User, PopupType.MediumCaution);

        args.Handled = true;
    }
}
