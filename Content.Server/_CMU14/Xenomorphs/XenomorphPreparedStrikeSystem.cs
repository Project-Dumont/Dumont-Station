using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Jittering;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Timing;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphPreparedStrikeComponent : Component
{
    [DataField] public bool Paralyzing;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan Until;
    public bool Active;
}

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphDelayedParalysisComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan At;
}

[RegisterComponent]
public sealed partial class XenomorphCripplingDamageComponent : Component;

public sealed partial class XenomorphPreparedStrikeSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MovementModStatusSystem _slow = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private SharedJitteringSystem _jitter = default!;
    [Dependency] private XenomorphRageSystem _rage = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphPreparedStrikeComponent, XenomorphPreparedStrikeActionEvent>(OnAction);
        SubscribeLocalEvent<XenomorphPreparedStrikeComponent, MeleeHitEvent>(OnHit);
        SubscribeLocalEvent<XenomorphCripplingDamageComponent, DamageModifyEvent>(OnDamage, before: new[] { typeof(XenomorphArmorSystem) });
    }

    private void OnAction(Entity<XenomorphPreparedStrikeComponent> ent, ref XenomorphPreparedStrikeActionEvent args)
    {
        if (args.Handled || !_mobs.IsAlive(ent) || !_blocker.CanAttack(ent)
            || !_plasma.ChangePlasmaAmount(ent, ent.Comp.Paralyzing ? -50 : -20)) return;
        ent.Comp.Active = true;
        ent.Comp.Until = _timing.CurTime + TimeSpan.FromSeconds(5);
        _actions.SetToggled(args.Action.Owner, true);
        args.Handled = true;
    }

    private void SetInactive(Entity<XenomorphPreparedStrikeComponent> ent)
    {
        ent.Comp.Active = false;
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphPreparedStrikeActionEvent) _actions.SetToggled(action.Owner, false);
    }

    private void OnHit(Entity<XenomorphPreparedStrikeComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.User != ent.Owner || !ent.Comp.Active || ent.Comp.Until <= _timing.CurTime || !_mobs.IsAlive(ent)) return;
        foreach (var target in args.HitEntities)
        {
            if (!HasComp<MobStateComponent>(target) || HasComp<XenomorphComponent>(target) || _mobs.IsDead(target)
                || ent.Comp.Paralyzing && (HasComp<SiliconComponent>(target) || HasComp<XenomorphDelayedParalysisComponent>(target))) continue;
            if (ent.Comp.Paralyzing)
            {
                var paralysis = EnsureComp<XenomorphDelayedParalysisComponent>(target);
                paralysis.At = _timing.CurTime + TimeSpan.FromSeconds(4);
                _jitter.DoJitter(target, TimeSpan.FromSeconds(4), true);
                _rage.Daze(target);
                Comp<XenomorphDazedComponent>(target).Until = _timing.CurTime + TimeSpan.FromSeconds(8);
            }
            else
            {
                EnsureComp<XenomorphCripplingDamageComponent>(target);
                var speed = TryComp<MovementSpeedModifierComponent>(target, out var movement) ? Math.Max(movement.CurrentSprintSpeed, movement.BaseSprintSpeed) : 3.5f;
                _slow.TryUpdateMovementSpeedModDuration(target, "XenomorphStompSlowdown", TimeSpan.FromSeconds(5), 1 / (1 + speed * .2f));
            }
            SetInactive(ent);
            break;
        }
    }

    private void OnDamage(Entity<XenomorphCripplingDamageComponent> ent, ref DamageModifyEvent args)
    {
        args.Damage *= 1.2f;
        RemCompDeferred<XenomorphCripplingDamageComponent>(ent);
    }

    public override void Update(float frameTime)
    {
        var strikes = EntityQueryEnumerator<XenomorphPreparedStrikeComponent>();
        while (strikes.MoveNext(out var uid, out var strike))
            if (strike.Active && (strike.Until <= _timing.CurTime || !_mobs.IsAlive(uid))) SetInactive((uid, strike));
        var victims = EntityQueryEnumerator<XenomorphDelayedParalysisComponent>();
        while (victims.MoveNext(out var uid, out var victim))
        {
            if (victim.At > _timing.CurTime) continue;
            RemCompDeferred<XenomorphDelayedParalysisComponent>(uid);
            if (!_mobs.IsDead(uid)) _stun.TryUpdateParalyzeDuration(uid, TimeSpan.FromSeconds(4));
        }
    }
}
