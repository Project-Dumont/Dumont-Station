using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphDancerSystem : EntitySystem
{
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private StandingStateSystem _standing = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;
    private readonly HashSet<Entity<MobStateComponent>> _nearby = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphDancerComponent, XenomorphDodgeActionEvent>(OnDodge);
        InitializeCombat();
    }

    private void OnDodge(Entity<XenomorphDancerComponent> ent, ref XenomorphDodgeActionEvent args)
    {
        if (args.Handled || ent.Comp.Dodging || !_mobs.IsAlive(ent) || !_blocker.CanConsciouslyPerformAction(ent)
            || TryComp<XenomorphRestComponent>(ent, out var rest) && rest.Resting
            || !_plasma.ChangePlasmaAmount(ent, -200)) return;
        ent.Comp.Dodging = true;
        ent.Comp.DodgeUntil = _timing.CurTime + TimeSpan.FromSeconds(15);
        Refresh(ent);
        _popup.PopupEntity(Loc.GetString("dumont-xenomorph-dodge-start"), ent, ent);
        args.Handled = true;
    }

    private void Refresh(Entity<XenomorphDancerComponent> ent)
    {
        Dirty(ent);
        _movement.RefreshMovementSpeedModifiers(ent);
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphDodgeActionEvent) _actions.SetToggled(action.Owner, ent.Comp.Dodging);
    }

    public override void Update(float frameTime)
    {
        UpdateCombat();
        var query = EntityQueryEnumerator<XenomorphDancerComponent>();
        while (query.MoveNext(out var uid, out var dancer))
        {
            if (!dancer.Dodging) continue;
            if (_timing.CurTime >= dancer.DodgeUntil)
            {
                dancer.Dodging = false;
                dancer.InCrowd = false;
                Refresh((uid, dancer));
                _popup.PopupEntity(Loc.GetString("dumont-xenomorph-dodge-end"), uid, uid);
                continue;
            }
            _nearby.Clear();
            _lookup.GetEntitiesInRange(Transform(uid).Coordinates, 1.5f, _nearby);
            var crowd = false;
            foreach (var mob in _nearby)
            {
                if (mob.Owner == uid || HasComp<XenomorphComponent>(mob) || !_mobs.IsAlive(mob) || _standing.IsDown(mob)) continue;
                crowd = true;
                break;
            }
            if (dancer.InCrowd == crowd) continue;
            dancer.InCrowd = crowd;
            Refresh((uid, dancer));
        }
    }
}
