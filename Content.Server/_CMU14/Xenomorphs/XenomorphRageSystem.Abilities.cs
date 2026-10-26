using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using System.Numerics;
using Content.Server.DoAfter;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Atmos.Components;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Effects;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphRageSystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedColorFlashEffectSystem _flash = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private MovementModStatusSystem _slow = default!;

    private void InitializeAbilities()
    {
        SubscribeLocalEvent<XenomorphRageComponent, XenomorphApprehendActionEvent>(OnApprehend);
        SubscribeLocalEvent<XenomorphRageComponent, XenomorphClotheslineActionEvent>(OnClothesline);
        SubscribeLocalEvent<XenomorphRageComponent, XenomorphEviscerateActionEvent>(OnEviscerate);
        SubscribeLocalEvent<XenomorphRageComponent, XenomorphEviscerateDoAfterEvent>(OnEviscerated);
        SubscribeLocalEvent<XenomorphDazedComponent, ComponentRemove>(OnDazeRemoved);
    }

    private bool CanUse(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanAttack(uid)
        && !_containers.IsEntityInContainer(uid)
        && !(TryComp<XenomorphRestComponent>(uid, out var rest) && rest.Resting);

    private void QueueHeal(Entity<XenomorphRageComponent> ent, float amount)
    {
        if (TryComp<FlammableComponent>(ent, out var fire) && fire.OnFire) return;
        ent.Comp.PendingHeal += amount;
        ent.Comp.HealAt = _timing.CurTime + TimeSpan.FromSeconds(0.05);
    }

    private void GiveAura(Entity<XenomorphRageComponent> ent, Color color, float width, float duration)
    {
        ent.Comp.AuraColor = color;
        ent.Comp.AuraWidth = width;
        ent.Comp.AuraUntil = _timing.CurTime + TimeSpan.FromSeconds(duration);
        Dirty(ent);
    }

    private void ToggleApprehend(EntityUid uid, bool toggled)
    {
        foreach (var action in _actions.GetActions(uid))
            if (_actions.GetEvent(action) is XenomorphApprehendActionEvent)
                _actions.SetToggled(action.AsNullable(), toggled);
    }

    private void OnApprehend(Entity<XenomorphRageComponent> ent, ref XenomorphApprehendActionEvent args)
    {
        if (args.Handled || !CanUse(ent)) return;
        ent.Comp.ApprehendUntil = _timing.CurTime + TimeSpan.FromSeconds(5);
        ent.Comp.ApprehendHit = false;
        GiveAura(ent, Color.FromHex("#522020"), 1, 5);
        ToggleApprehend(ent, true);
        Refresh(ent);
        _popup.PopupEntity(Loc.GetString("xenomorph-apprehend-active"), ent, ent);
        args.Handled = true;
    }

    private void ApprehendHit(Entity<XenomorphRageComponent> ent, MeleeHitEvent args)
    {
        if (ent.Comp.ApprehendUntil <= _timing.CurTime || ent.Comp.ApprehendHit) return;
        foreach (var target in args.HitEntities.Where(Enemy))
        {
            if (TryComp<MovementSpeedModifierComponent>(target, out var speed))
                _slow.TryUpdateMovementSpeedModDuration(target, "XenomorphStompSlowdown", TimeSpan.FromSeconds(5),
                    1 / (1 + Math.Max(speed.CurrentSprintSpeed, speed.BaseSprintSpeed) * 0.8f));
            ent.Comp.ApprehendHit = true;
            _popup.PopupEntity(Loc.GetString("xenomorph-apprehend-hit"), target, ent);
            break;
        }
    }

    private void OnClothesline(Entity<XenomorphRageComponent> ent, ref XenomorphClotheslineActionEvent args)
    {
        if (args.Handled || !CanUse(ent) || !Enemy(args.Target)
            || !_interaction.InRangeUnobstructed(ent.Owner, args.Target, 1.5f)) return;
        if (Transform(args.Target).Anchored) return;
        _pulling.StopAllPulls(args.Target);
        var damage = new DamageSpecifier { DamageDict = { ["Blunt"] = 6.66, ["Slash"] = 6.67, ["Piercing"] = 6.67 } };
        _damage.TryChangeDamage(args.Target, damage, origin: ent);
        _flash.RaiseEffect(Color.Red, new List<EntityUid> { args.Target }, Filter.Pvs(args.Target, entityManager: EntityManager));
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_claw_block.ogg"), ent);
        Spawn("DumontXenomorphPunchEffect", new EntityCoordinates(args.Target, Vector2.Zero));
        var offset = _transform.GetWorldPosition(args.Target) - _transform.GetWorldPosition(ent);
        _melee.DoLunge(ent, ent, ent, Angle.Zero, (-_transform.GetWorldRotation(ent)).RotateVec(offset), "WeaponArcThrust", Angle.Zero, false, predicted: false);
        var enraged = ent.Comp.Rage >= 2;
        QueueHeal(ent, enraged ? 220 : 95);
        if (offset.LengthSquared() > 0)
            _throwing.TryThrow(args.Target, Vector2.Normalize(offset) * (enraged ? 3 : 2), 12, user: ent, pushbackRatio: 0, doSpin: false);
        Daze(args.Target);
        args.Handled = true;
    }

    public void Daze(EntityUid uid)
    {
        var dazed = EnsureComp<XenomorphDazedComponent>(uid);
        dazed.Until = _timing.CurTime + TimeSpan.FromSeconds(4);
        foreach (var action in _actions.GetActions(uid))
        {
            if (!HasComp<XenomorphDazeableActionComponent>(action) || !action.Comp.Enabled) continue;
            dazed.DisabledActions.Add(action);
            _actions.SetEnabled(action.AsNullable(), false);
        }
    }

    private void OnDazeRemoved(Entity<XenomorphDazedComponent> ent, ref ComponentRemove args)
    {
        foreach (var action in _actions.GetActions(ent))
            if (ent.Comp.DisabledActions.Contains(action)) _actions.SetEnabled(action.AsNullable(), true);
    }

    private void ToggleEviscerate(EntityUid uid, bool toggled)
    {
        foreach (var action in _actions.GetActions(uid))
            if (_actions.GetEvent(action) is XenomorphEviscerateActionEvent)
                _actions.SetToggled(action.AsNullable(), toggled);
    }

    private void OnEviscerate(Entity<XenomorphRageComponent> ent, ref XenomorphEviscerateActionEvent args)
    {
        if (args.Handled || !CanUse(ent)) return;
        var rage = ent.Comp.Rage;
        if (rage <= 0)
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-eviscerate-no-rage"), ent, ent);
            return;
        }
        var windup = new[] { 2f, 1.8f, 1.6f, 1.4f, 1f }[rage - 1];
        var ev = new XenomorphEviscerateDoAfterEvent { Rage = rage };
        if (!_doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, windup, ev, ent.Owner)
            { BreakOnMove = true, MovementThreshold = 0.5f, Hidden = true, NeedHand = false })) return;
        _slow.TryUpdateMovementSpeedModDuration(ent, "XenomorphStompSlowdown", TimeSpan.FromSeconds(windup), 0);
        ToggleEviscerate(ent, true);
        ChangeRage(ent, -1);
        _popup.PopupEntity(Loc.GetString("xenomorph-eviscerate-windup"), ent, ent);
        args.Handled = true;
    }

    private void OnEviscerated(Entity<XenomorphRageComponent> ent, ref XenomorphEviscerateDoAfterEvent args)
    {
        ToggleEviscerate(ent, false);
        if (args.Cancelled || args.Handled || !CanUse(ent)) return;
        args.Handled = true;
        EnsureComp<XenomorphSweepingComponent>(ent);
        _pulling.StopAllPulls(ent);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_tail_swipe1.ogg"), ent);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_roar1.ogg"), ent);
        var range = args.Rage >= 4 ? 2.5f : 1.5f;
        var damage = new[]
        {
            new DamageSpecifier { DamageDict = { ["Blunt"] = 1.66, ["Slash"] = 1.67, ["Piercing"] = 1.67 } },
            new DamageSpecifier { DamageDict = { ["Blunt"] = 3.33, ["Slash"] = 3.33, ["Piercing"] = 3.34 } },
            new DamageSpecifier { DamageDict = { ["Blunt"] = 8.33, ["Slash"] = 8.33, ["Piercing"] = 8.34 } },
            new DamageSpecifier { DamageDict = { ["Blunt"] = 15, ["Slash"] = 15, ["Piercing"] = 15 } },
            new DamageSpecifier { DamageDict = { ["Blunt"] = 28.33, ["Slash"] = 28.33, ["Piercing"] = 28.34 } },
        }[args.Rage - 1];
        var count = 0;
        foreach (var target in _lookup.GetEntitiesInRange<MobStateComponent>(_transform.GetMoverCoordinates(ent), range))
        {
            if (!Enemy(target) || !_interaction.InRangeUnobstructed(ent.Owner, target.Owner, range)) continue;
            _pulling.StopAllPulls(target);
            _damage.TryChangeDamage(target, damage, origin: ent);
            _flash.RaiseEffect(Color.Red, new List<EntityUid> { target }, Filter.Pvs(target, entityManager: EntityManager));
            _audio.PlayPvs(new SoundPathSpecifier(range > 1.5f ? "/Audio/_RMC14/Effects/gibbed.ogg" : "/Audio/_RMC14/Xeno/alien_claw_block.ogg"), target);
            if (range > 1.5f) _stun.TryUpdateParalyzeDuration(target, TimeSpan.FromSeconds(1.85));
            count++;
        }
        QueueHeal(ent, Math.Min(count * 50, 250));
    }

    private void UpdateAbilities()
    {
        var dazedQuery = EntityQueryEnumerator<XenomorphDazedComponent>();
        while (dazedQuery.MoveNext(out var uid, out var dazed))
            if (dazed.Until <= _timing.CurTime) RemCompDeferred<XenomorphDazedComponent>(uid);
        var query = EntityQueryEnumerator<XenomorphRageComponent>();
        while (query.MoveNext(out var uid, out var rage))
        {
            if (rage.ApprehendUntil == TimeSpan.Zero || rage.ApprehendUntil > _timing.CurTime) continue;
            rage.ApprehendUntil = TimeSpan.Zero;
            ToggleApprehend(uid, false);
            Refresh((uid, rage));
        }
    }
}
