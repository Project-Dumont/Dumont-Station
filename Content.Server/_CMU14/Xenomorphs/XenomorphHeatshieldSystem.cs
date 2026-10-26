using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using Content.Server.Atmos.EntitySystems;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphHeatshieldSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private FlammableSystem _flammable = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphHeatshieldComponent, IgnitedEvent>(OnIgnited);
        SubscribeLocalEvent<XenomorphHeatshieldComponent, ExtinguishedEvent>(OnExtinguished);
        SubscribeLocalEvent<XenomorphHeatshieldComponent, XenomorphSelfImmolateActionEvent>(OnImmolate);
        SubscribeLocalEvent<XenomorphHeatshieldComponent, XenomorphSelfImmolateDoAfterEvent>(OnImmolateDoAfter);
        SubscribeLocalEvent<XenomorphHeatshieldComponent, XenomorphThermoregulationActionEvent>(OnThermoregulate);
    }

    private void OnIgnited(Entity<XenomorphHeatshieldComponent> ent, ref IgnitedEvent args)
        => SetFireAction(ent, true);

    private void OnExtinguished(Entity<XenomorphHeatshieldComponent> ent, ref ExtinguishedEvent args)
        => SetFireAction(ent, false);

    private void SetFireAction(EntityUid uid, bool burning)
    {
        foreach (var action in _actions.GetActions(uid))
        {
            if (_actions.GetEvent(action) is XenomorphSelfImmolateActionEvent)
                _actions.SetToggled(action.Owner, burning);
        }
    }

    private void OnImmolate(Entity<XenomorphHeatshieldComponent> ent, ref XenomorphSelfImmolateActionEvent args)
    {
        if (args.Handled || !_mobs.IsAlive(ent) || !_blocker.CanInteract(ent, null)
            || !_plasma.HasPlasma(ent, 50))
            return;
        args.Handled = true;
        _flammable.AdjustFireStacks(ent, 8);
        if (HasComp<OnFireComponent>(ent))
        {
            _flammable.Ignite(ent, ent);
            return;
        }
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(1.5),
            new XenomorphSelfImmolateDoAfterEvent(), ent)
        {
            BreakOnMove = true,
            NeedHand = false,
            DuplicateCondition = DuplicateConditions.SameEvent,
        });
    }

    private void OnImmolateDoAfter(Entity<XenomorphHeatshieldComponent> ent, ref XenomorphSelfImmolateDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || !_mobs.IsAlive(ent) || !_blocker.CanInteract(ent, null))
            return;
        args.Handled = true;
        _flammable.Ignite(ent, ent);
    }

    private void OnThermoregulate(Entity<XenomorphHeatshieldComponent> ent, ref XenomorphThermoregulationActionEvent args)
    {
        if (args.Handled || !_mobs.IsAlive(ent) || !_blocker.CanInteract(ent, null))
            return;
        if (!HasComp<OnFireComponent>(ent))
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-thermoregulation-not-burning"), ent, ent);
            return;
        }
        if (!_plasma.HasPlasma(ent, 40))
            return;
        args.Handled = true;
        ent.Comp.Thermoregulating = true;
        _actions.SetToggled(args.Action.Owner, true);
        ent.Comp.ExpiresAt = _timing.CurTime + TimeSpan.FromSeconds(5);
        Dirty(ent);
        _movement.RefreshMovementSpeedModifiers(ent);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphHeatshieldComponent>();
        while (query.MoveNext(out var uid, out var heatshield))
        {
            if (!heatshield.Thermoregulating || _timing.CurTime < heatshield.ExpiresAt)
                continue;
            heatshield.Thermoregulating = false;
            foreach (var action in _actions.GetActions(uid))
            {
                if (_actions.GetEvent(action) is XenomorphThermoregulationActionEvent)
                    _actions.SetToggled(action.Owner, false);
            }
            Dirty(uid, heatshield);
            _flammable.Extinguish(uid);
            _movement.RefreshMovementSpeedModifiers(uid);
        }
    }
}
