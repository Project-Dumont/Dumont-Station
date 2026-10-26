using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Plasma;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Actions;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class XenomorphInvisibilitySystem : EntitySystem
{
    [Dependency] private SharedPlasmaSystem _plasma = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private MovementSpeedModifierSystem _speed = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphInvisibilityComponent, XenomorphInvisibilityActionEvent>(OnAction);
        SubscribeLocalEvent<XenomorphInvisibilityComponent, MeleeHitEvent>(OnMelee);
        SubscribeLocalEvent<XenomorphInvisibilityComponent, XenomorphLeapHitEvent>(OnLeapHit);
        SubscribeLocalEvent<XenomorphInvisibilityComponent, MobStateChangedEvent>(OnMobState);
        SubscribeLocalEvent<XenomorphInvisibilityComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
    }

    private void OnAction(Entity<XenomorphInvisibilityComponent> ent, ref XenomorphInvisibilityActionEvent args)
    {
        if (_net.IsClient || args.Handled || !_mobState.IsAlive(ent) || _timing.CurTime < ent.Comp.NextUseAt
            || TryComp<XenomorphRestComponent>(ent, out var rest) && rest.Resting
            || !_plasma.ChangePlasmaAmount(ent, -ent.Comp.PlasmaCost))
            return;

        ent.Comp.Action = args.Action.Owner;
        if (ent.Comp.Active)
        {
            var remaining = Math.Clamp((ent.Comp.ExpiresAt - _timing.CurTime) / ent.Comp.Duration, 0, 1);
            SetActive(ent, false, ent.Comp.Cooldown * (1 - remaining * 0.9));
        }
        else
        {
            ent.Comp.ExpiresAt = _timing.CurTime + ent.Comp.Duration;
            SetActive(ent, true, TimeSpan.FromSeconds(0.5));
        }
        args.Handled = true;
    }

    private void OnMelee(Entity<XenomorphInvisibilityComponent> ent, ref MeleeHitEvent args)
    {
        if (_net.IsClient || !ent.Comp.Active || args.User != ent.Owner || !args.IsHit)
            return;

        foreach (var target in args.HitEntities)
        {
            if (!HasComp<XenomorphComponent>(target))
            {
                SetActive(ent, false, ent.Comp.Cooldown);
                break;
            }
        }
    }

    private void OnLeapHit(Entity<XenomorphInvisibilityComponent> ent, ref XenomorphLeapHitEvent args)
    {
        if (!_net.IsClient && ent.Comp.Active)
            SetActive(ent, false, ent.Comp.Cooldown);
    }

    private void OnMobState(Entity<XenomorphInvisibilityComponent> ent, ref MobStateChangedEvent args)
    {
        if (!_net.IsClient && ent.Comp.Active && args.NewMobState != MobState.Alive)
            SetActive(ent, false, ent.Comp.Cooldown);
    }

    private void OnSpeed(Entity<XenomorphInvisibilityComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Active)
            args.ModifySpeed(ent.Comp.SpeedMultiplier, ent.Comp.SpeedMultiplier);
    }

    private void SetActive(Entity<XenomorphInvisibilityComponent> ent, bool active, TimeSpan cooldown)
    {
        ent.Comp.Active = active;
        ent.Comp.NextUseAt = _timing.CurTime + cooldown;
        ent.Comp.PendingActionUpdate = true;
        Dirty(ent);
        _appearance.SetData(ent, XenomorphVisuals.Invisible, active);
        _speed.RefreshMovementSpeedModifiers(ent);
    }

    public override void Update(float frameTime)
    {
        if (_net.IsClient)
            return;

        var query = EntityQueryEnumerator<XenomorphInvisibilityComponent>();
        while (query.MoveNext(out var uid, out var invisibility))
        {
            if (invisibility.Active && _timing.CurTime >= invisibility.ExpiresAt)
                SetActive((uid, invisibility), false, invisibility.Cooldown);

            if (!invisibility.PendingActionUpdate)
                continue;
            invisibility.PendingActionUpdate = false;
            _actions.SetCooldown(invisibility.Action, _timing.CurTime, invisibility.NextUseAt);
            _actions.SetToggled(invisibility.Action, invisibility.Active);
        }
    }
}
